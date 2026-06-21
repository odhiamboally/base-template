using Azure.Monitor.OpenTelemetry.AspNetCore;
using BT.Application.Contracts.Interfaces.Common;
using BT.Application.Features.IAM.Users.Contracts.Interfaces;
using BT.Application.Features.Shared.Notifications.Contracts.Interfaces;
using BT.Domain.Features.Banking.Contracts;
using BT.Domain.Features.HR.Contracts;
using BT.Domain.Features.IAM.Contracts;
using BT.Domain.Shared.Contracts;
using BT.Domain.Shared.Contracts.Common;
using BT.Domain.Features.Banking.Customers.Contracts.Repositories;
using BT.Domain.Features.HR.Employees.Contracts.Repositories;
using BT.Domain.Features.IAM.Users.Contracts.Repositories;
using BT.Domain.Shared.Contracts.Repositories;
using BT.Infrastructure.Configuration;
using BT.Infrastructure.Contracts.Implementations.Common;
using BT.Infrastructure.Contracts.Implementations.Caching;
using BT.Infrastructure.Features.IAM.Users.Contracts.Implementations.Services;
using BT.Infrastructure.Features.IAM.Users.Contracts.Implementations.Storage;
using BT.Infrastructure.Features.Shared.Notifications.Contracts.Implementations.Services;
using BT.Infrastructure.Contracts.Interfaces;
using BT.Infrastructure.Features.Banking.Customers.EmailComposers;
using BT.Infrastructure.Features.HR.Employees.EmailComposers;
using BT.Infrastructure.Logging;
using BT.Infrastructure.Logging.Enrichers;
using BT.Infrastructure.Middleware;
using BT.Infrastructure.Utilities;
using BT.Application.Features.Banking.Customers.IntegrationEvents;
using BT.Application.Features.HR.Employees.IntegrationEvents;
using BT.Application.Features.IAM.Users.IntegrationEvents;
using FluentValidation;
using MailKit.Net.Smtp;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using MediatR;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Quartz;
using Quartz.Simpl;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Text;
using System.Text.Json;
using Twilio;
using BT.Infrastructure.Features.IAM.AspNetCoreIdentity.CommandHandlers;
using Microsoft.Azure.StackExchangeRedis;
using StackExchange.Redis;
using Microsoft.Extensions.Caching.StackExchangeRedis;

namespace BT.Infrastructure.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddSharedInfrastructure(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));

            services.AddSingleton(JsonSerializerOptionsFactory.Create());
            services.Configure<ObservabilitySettings>(configuration.GetSection(ObservabilitySettings.SectionName));
            services.Configure<AuthProviderSettings>(configuration.GetSection(AuthProviderSettings.SectionName));
            services.Configure<ApiSettings>(configuration.GetSection(ApiSettings.SectionName));
            services.Configure<IamProvisioningSettings>(configuration.GetSection(IamProvisioningSettings.SectionName));
            services.Configure<MfaSettings>(configuration.GetSection(MfaSettings.SectionName));
            services
                .AddOptions<ProfileImageStorageSettings>()
                .Bind(configuration.GetSection(ProfileImageStorageSettings.SectionName))
                .ValidateDataAnnotations()
                .Validate(
                    IsValidProfileImageStorageProvider,
                    "ProfileImageStorage:Provider must be Local, Azurite, or AzureBlob. Blob providers require ContainerUri or ConnectionString plus ContainerName.")
                .ValidateOnStart();
            services.AddOptions<TenantSettings>()
                .Bind(configuration.GetSection(TenantSettings.SectionName))
                .ValidateDataAnnotations()
                .Validate(settings => settings.DefaultTenantId != Guid.Empty, "Tenant:DefaultTenantId must be configured.")
                .ValidateOnStart();

            var cacheSettings = configuration.GetSection(CacheSettings.SectionName).Get<CacheSettings>()
                ?? throw new InvalidOperationException("CacheSettings not found.");

            services.Configure<SessionSettings>(configuration.GetSection(SessionSettings.SectionName));
            ConfigureDistributedCache(services, configuration, cacheSettings);
            ConfigureMailKitWithSmtp(services, configuration);
            ConfigureSms(services, configuration);
            ConfigureSerilogEnrichers(services);
            var observabilitySettings = configuration.GetSection(ObservabilitySettings.SectionName).Get<ObservabilitySettings>() ?? new ObservabilitySettings();
            var authProviderSettings = configuration.GetSection(AuthProviderSettings.SectionName).Get<AuthProviderSettings>() ?? new AuthProviderSettings();
            var messagingSettings = configuration.GetSection(MessagingSettings.SectionName).Get<MessagingSettings>() ?? new MessagingSettings();
            var backgroundJobSettings = configuration.GetSection(BackgroundJobSettings.SectionName).Get<BackgroundJobSettings>() ?? new BackgroundJobSettings();
            ConfigureObservability(services, configuration, environment, observabilitySettings);
            if (backgroundJobSettings.Enabled)
            {
                ConfigureQuartz(services, configuration);
            }

            AddServices(services, authProviderSettings, messagingSettings, backgroundJobSettings);
            
        return services;
    }

    private static IServiceCollection AddServices(
        this IServiceCollection services,
        AuthProviderSettings authProviderSettings,
        MessagingSettings messagingSettings,
        BackgroundJobSettings backgroundJobSettings)
    {
        if (messagingSettings.Enabled)
        {
            services.AddScoped<IIntegrationEventPublisher, MassTransitIntegrationEventPublisher>();
        }
        else
        {
            services.AddScoped<IIntegrationEventPublisher, NoOpIntegrationEventPublisher>();
        }

        services.AddHttpClient<IApiService, ApiService>();
        services.AddScoped<ICurrentTenantProvider, CurrentTenantProvider>();
        services.AddScoped<ICurrentActorProvider, CurrentActorProvider>();
        services.AddScoped<LocalProfilePictureStorage>();
        services.AddScoped<AzureBlobProfilePictureStorage>();
        services.AddScoped<IProfilePictureStorage>(sp =>
        {
            var settings = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<ProfileImageStorageSettings>>().Value;
            return GetProfileImageStorageProvider(settings) switch
            {
                ProfileImageStorageProvider.Local => sp.GetRequiredService<LocalProfilePictureStorage>(),
                ProfileImageStorageProvider.Azurite => sp.GetRequiredService<AzureBlobProfilePictureStorage>(),
                ProfileImageStorageProvider.AzureBlob => sp.GetRequiredService<AzureBlobProfilePictureStorage>(),
                _ => throw new InvalidOperationException(
                    $"ProfileImageStorage:Provider '{settings.Provider}' is not supported. " +
                    "Supported values: Local, Azurite, AzureBlob.")
            };
        });

        if (!authProviderSettings.Enabled)
        {
            return services;
        }

        if (GetAuthProvider(authProviderSettings) is not AuthProvider.AspNetCoreIdentity)
        {
            throw new InvalidOperationException(
                $"AuthProvider:Provider '{authProviderSettings.Provider}' is not supported. " +
                "Supported values: AspNetCoreIdentity.");
        }

        services.AddScoped<IEmailService, FluentMailService>();
        services.AddScoped<ISmsService, SmsService>();
        services.AddScoped<IBackgroundJobService>(_ =>
            backgroundJobSettings.Enabled
                ? new BackgroundJobService(
                    _.GetRequiredService<ISchedulerFactory>(),
                    _.GetRequiredService<ILogger<BackgroundJobService>>())
                : new NoOpBackgroundJobService());
        services.AddScoped<IEncryptionService, EncryptionService>();

        return services;
    }

    internal static void ConfigureAuthentication(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

        var jwtSettings = new JwtSettings();
            configuration.GetSection(JwtSettings.SectionName).Bind(jwtSettings);

            services.AddSingleton(jwtSettings);

            if (jwtSettings == null)
                throw new InvalidOperationException("JwtSettings not found in configuration");

            services.AddSingleton(sp =>
            {
                var jwtSettings = sp.GetRequiredService<JwtSettings>();

                return new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = jwtSettings.GetSymmetricSecurityKey(),
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtSettings.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(jwtSettings.ClockSkew),
                    RequireExpirationTime = true,
                    RequireSignedTokens = true,
                };
            });

            services.Configure<IdentityOptions>(ConfigureIdentityOptions);

            // Authentication with JWT
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                ConfigureJwtBearer(options, jwtSettings);
            });

        services.Configure<DataProtectionTokenProviderOptions>(options =>
        {
            options.TokenLifespan = TimeSpan.FromHours(24); // 24 hours for email tokens
        });
    }

    internal static void ConfigureIdentityOptions(IdentityOptions options)
    {
        options.ClaimsIdentity.UserNameClaimType = "Username";

        // User settings
        options.User.AllowedUserNameCharacters =
            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";

        options.User.RequireUniqueEmail = true;

        // Password settings
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequiredLength = 8;
        options.Password.RequiredUniqueChars = 1;

        // Lockout settings
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.AllowedForNewUsers = true;

        // Sign-in settings
        options.SignIn.RequireConfirmedEmail = false;
        options.SignIn.RequireConfirmedPhoneNumber = false;
        options.SignIn.RequireConfirmedAccount = false;

        // Token Providers
        options.Tokens.AuthenticatorTokenProvider = TokenOptions.DefaultAuthenticatorProvider;
        options.Tokens.PasswordResetTokenProvider = TokenOptions.DefaultEmailProvider;
        options.Tokens.EmailConfirmationTokenProvider = TokenOptions.DefaultEmailProvider;
        options.Tokens.ChangeEmailTokenProvider = TokenOptions.DefaultEmailProvider;
    }

    internal static void ConfigureJwtBearer(JwtBearerOptions options, JwtSettings jwtSettings)
    {
        options.SaveToken = true;
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ClockSkew = TimeSpan.FromMinutes(1),
            ValidIssuer = jwtSettings?.Issuer,
            ValidAudience = jwtSettings?.Audience,
            IssuerSigningKey = jwtSettings?.GetSymmetricSecurityKey(),
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            RequireExpirationTime = true,

            // CUSTOM LIFETIME VALIDATOR to handle NotBefore issues
            LifetimeValidator = (notBefore, expires, token, parameters) =>
            {
                var now = DateTime.UtcNow;

                // Check expiration (required)
                if (expires.HasValue && expires.Value < now)
                {
                    return false; // Token expired
                }

                // Check not before (lenient - allow if missing or if time is close)
                if (notBefore.HasValue && notBefore.Value > now.AddMinutes(1))
                {
                    return false; // Token not yet valid (with 1 min tolerance)
                }

                return true; // Token is valid
            }
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                if (context.Exception is SecurityTokenExpiredException)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    context.Response.ContentType = "application/json";

                    var expirationTime = context.Exception is SecurityTokenExpiredException expiredException
                        ? expiredException.Expires.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture)
                        : string.Empty; 

                    var errorMessage = new
                    {
                        context.Response.StatusCode,
                        Message = "Token has expired.",
                        ExpirationTime = expirationTime,
                        CurrentTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture),
                        Error = context.Exception.Message
                    };

                    return context.Response.WriteAsync(JsonSerializer.Serialize(errorMessage));
                }
                else
                {
                    var loggerFactory = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>();
                    var logger = loggerFactory.CreateLogger("JwtAuthentication");

                    // Check for NotBefore issues specifically
                    if (context.Exception.Message.Contains("NotBefore", StringComparison.OrdinalIgnoreCase) || 
                    context.Exception.Message.Contains("not yet valid", StringComparison.OrdinalIgnoreCase))
                    {
                        ServiceLogDefinitions.LogJwtAuthenticationFailed(logger, "Token is not yet valid", context.Exception);
                    }
                    else
                    {
                        ServiceLogDefinitions.LogJwtAuthenticationFailed(logger, "Token validation failed", context.Exception);
                    }
                }

                return Task.CompletedTask;
            }
        };
    }

    private static void ConfigureDistributedCache(IServiceCollection services, IConfiguration configuration, CacheSettings cacheSettings)
    {
        var provider = GetCacheProvider(cacheSettings);

        switch (provider)
        {
            case CacheProvider.Memory:
                services.AddDistributedMemoryCache();
                break;

            case CacheProvider.Redis:
                ValidateConnectionString(cacheSettings.Redis?.ConnectionString, "Redis");
                RegisterRedisCache(services, cacheSettings.Redis?.ConnectionString!);
                break;

            case CacheProvider.AzureManagedRedis:
                ValidateConnectionString(cacheSettings.Azure?.ConnectionString, "AzureManagedRedis");
                if (cacheSettings.Azure?.UseEntraId == true)
                {
                    RegisterAzureManagedRedisWithEntraId(services, cacheSettings.Azure);
                }
                else
                {
                    RegisterRedisCache(services, cacheSettings.Azure!.ConnectionString!);
                }
                break;

            case CacheProvider.Invalid:
            default:
                throw new InvalidOperationException(
                    $"CacheSettings:Provider '{cacheSettings.Provider}' is not supported. " +
                    "Supported values: Auto, Memory, Redis, AzureManagedRedis.");
        }

        services.AddHybridCache(options =>
        {
            options.DefaultEntryOptions = new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromHours(1)
            };
            options.MaximumPayloadBytes = 1024 * 1024; // 1MB limit for L2
        });

        services.AddSingleton<ICacheService, HybridCacheService>();

        services.AddOutputCache(options =>
        {
            options.AddPolicy("LookupCachePolicy", builder =>
                builder.Expire(TimeSpan.FromMinutes(5)).Tag("lookups"));
        });
    }

    private static void RegisterRedisCache(IServiceCollection services, string connectionString)
    {
        services.AddSingleton<StackExchange.Redis.IConnectionMultiplexer>(_ =>
            StackExchange.Redis.ConnectionMultiplexer.Connect(connectionString));

        RegisterStackExchangeRedisCache(services);
    }

    private static void RegisterAzureManagedRedisWithEntraId(
        IServiceCollection services,
        AzureCacheSettings settings)
    {
        services.AddSingleton<StackExchange.Redis.IConnectionMultiplexer>(_ =>
        {
            var configOptions = StackExchange.Redis.ConfigurationOptions.Parse(settings.ConnectionString!);
            configOptions.Protocol = StackExchange.Redis.RedisProtocol.Resp3;
            configOptions.Password = null;

            var credentialOptions = new Azure.Identity.DefaultAzureCredentialOptions();
            if (!string.IsNullOrWhiteSpace(settings.PrincipalId))
            {
                credentialOptions.ManagedIdentityClientId = settings.PrincipalId;
            }

            configOptions.ConfigureForAzureWithTokenCredentialAsync(
                new Azure.Identity.DefaultAzureCredential(credentialOptions)).GetAwaiter().GetResult();

            return StackExchange.Redis.ConnectionMultiplexer.Connect(configOptions);
        });

        RegisterStackExchangeRedisCache(services);
    }

    private static void RegisterStackExchangeRedisCache(IServiceCollection services)
    {
        services.AddStackExchangeRedisCache(options => { });
        services.AddOptions<RedisCacheOptions>()
            .Configure<StackExchange.Redis.IConnectionMultiplexer>((options, multiplexer) =>
            {
                options.ConnectionMultiplexerFactory = () => Task.FromResult(multiplexer);
            });
    }

    private static bool IsConfiguredConnectionString(string? connectionString)
    {
        return !string.IsNullOrWhiteSpace(connectionString) &&
            !connectionString.Contains("your-", StringComparison.OrdinalIgnoreCase) &&
            !connectionString.Contains("replace", StringComparison.OrdinalIgnoreCase) &&
            !connectionString.Contains("set_via", StringComparison.OrdinalIgnoreCase);
    }

    private static CacheProvider GetCacheProvider(CacheSettings settings)
    {
        var configuredProvider = settings.Provider?.Trim() ?? "Auto";
        if (configuredProvider.Equals("Auto", StringComparison.OrdinalIgnoreCase))
        {
            if (IsConfiguredConnectionString(settings.Azure?.ConnectionString))
            {
                return CacheProvider.AzureManagedRedis;
            }

            return IsConfiguredConnectionString(settings.Redis.ConnectionString)
                ? CacheProvider.Redis
                : CacheProvider.Memory;
        }

        return configuredProvider switch
        {
            var provider when provider.Equals("Memory", StringComparison.OrdinalIgnoreCase) => CacheProvider.Memory,
            var provider when provider.Equals("Redis", StringComparison.OrdinalIgnoreCase) &&
                              IsConfiguredConnectionString(settings.Redis.ConnectionString) => CacheProvider.Redis,
            var provider when provider.Equals("AzureManagedRedis", StringComparison.OrdinalIgnoreCase) &&
                              IsConfiguredConnectionString(settings.Azure?.ConnectionString) => CacheProvider.AzureManagedRedis,
            _ => CacheProvider.Invalid
        };
    }

    private static void ValidateConnectionString(string? connectionString, string providerName)
    {
        if (string.IsNullOrWhiteSpace(connectionString) ||
            connectionString.Equals("SET_VIA_USER_SECRETS", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Configuration Error: The connection string for provider '{providerName}' is missing or has a placeholder value.");
        }
    }

    private static bool IsValidProfileImageStorageProvider(ProfileImageStorageSettings settings)
    {
        return GetProfileImageStorageProvider(settings) switch
        {
            ProfileImageStorageProvider.Local => true,
            ProfileImageStorageProvider.Azurite => IsBlobProfileImageStorageConfigured(settings.Azurite),
            ProfileImageStorageProvider.AzureBlob => IsAzureBlobProfileImageStorageConfigured(settings),
            _ => false
        };
    }

    private static bool IsAzureBlobProfileImageStorageConfigured(ProfileImageStorageSettings settings)
    {
        return IsBlobProfileImageStorageConfigured(settings.AzureBlob);
    }

    private static bool IsBlobProfileImageStorageConfigured(AzureBlobProfileImageStorageSettings settings)
    {
        return !string.IsNullOrWhiteSpace(settings.ContainerUri) ||
            (!string.IsNullOrWhiteSpace(settings.ConnectionString) &&
             !string.IsNullOrWhiteSpace(settings.ContainerName));
    }

    private static ProfileImageStorageProvider GetProfileImageStorageProvider(ProfileImageStorageSettings settings)
    {
        if (string.IsNullOrWhiteSpace(settings.Provider))
        {
            return ProfileImageStorageProvider.Local;
        }

        return settings.Provider.Trim() switch
        {
            var provider when provider.Equals("Local", StringComparison.OrdinalIgnoreCase) => ProfileImageStorageProvider.Local,
            var provider when provider.Equals("Azurite", StringComparison.OrdinalIgnoreCase) => ProfileImageStorageProvider.Azurite,
            var provider when provider.Equals("AzureBlob", StringComparison.OrdinalIgnoreCase) => ProfileImageStorageProvider.AzureBlob,
            _ => ProfileImageStorageProvider.Invalid
        };
    }

    private static AuthProvider GetAuthProvider(AuthProviderSettings settings)
    {
        if (string.IsNullOrWhiteSpace(settings.Provider))
        {
            return AuthProvider.AspNetCoreIdentity;
        }

        return settings.Provider.Trim() switch
        {
            var provider when provider.Equals("AspNetCoreIdentity", StringComparison.OrdinalIgnoreCase) => AuthProvider.AspNetCoreIdentity,
            _ => AuthProvider.Invalid
        };
    }

    private static void ConfigureSerilogEnrichers(IServiceCollection services)
    {
        services.AddSingleton<ILogEventEnricher, CorrelationIdEnricher>();
        services.AddSingleton<ILogEventEnricher, IPAddressEnricher>();
    }

    /// <summary>
    /// Registers OpenTelemetry tracing, metrics, and Azure Monitor export.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Architecture decision:</b> We use the Azure Monitor OpenTelemetry Distro
    /// (<c>Azure.Monitor.OpenTelemetry.AspNetCore</c>). The Distro is
    /// Microsoft's strategic direction — it follows the OTel standard, meaning
    /// you can swap the exporter (Azure Monitor → Jaeger/Grafana/etc.) by changing
    /// one line.
    /// </para>
    /// <para>
    /// <b>What the Distro includes automatically</b> (no extra packages needed):
    /// ASP.NET Core request tracing, HttpClient tracing, SQLClient tracing,
    /// Azure resource detection (App Service, VM, Container Apps), live metrics.
    /// </para>
    /// <para>
    /// <b>What we add on top:</b>
    /// Runtime metrics (GC, ThreadPool, memory) via
    /// <c>OpenTelemetry.Instrumentation.Runtime</c>.
    /// </para>
    /// <para>
    /// <b>Serilog relationship:</b> Serilog continues to handle log routing
    /// (Console, Seq, File). The OTel pipeline handles traces and metrics.
    /// Logs flow to Azure Monitor via Serilog's ILogger integration with
    /// <c>ReadFrom.Services()</c> — not via a Serilog sink.
    /// </para>
    /// </remarks>
    public static IServiceCollection ConfigureObservability(
        this IServiceCollection services, 
        IConfiguration configuration, 
        IWebHostEnvironment environment, 
        ObservabilitySettings observabilitySettings)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(observabilitySettings, nameof(observabilitySettings));
        if (!observabilitySettings.Enabled)
        {
            return services;
        }

        var connectionString = observabilitySettings.AzureMonitor.ConnectionString;
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            connectionString =
                configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"] ??
                configuration["ApplicationInsights:ConnectionString"];
        }

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            Console.WriteLine(
                "[WARN] Observability:AzureMonitor:ConnectionString not configured. " +
                "OpenTelemetry export to Azure Monitor is disabled. " +
                "Set 'Observability--AzureMonitor--ConnectionString', 'ApplicationInsights--ConnectionString', or App Service 'APPLICATIONINSIGHTS_CONNECTION_STRING'.");

            return services;
        }

        var otelBuilder = services.AddOpenTelemetry();

        otelBuilder.ConfigureResource(resource => resource
            .AddService(serviceName: observabilitySettings.ServiceName, serviceVersion: "1.0.0")
            .AddAttributes(new Dictionary<string, object>
            {
                ["deployment.environment"] = environment.EnvironmentName,
                ["service.namespace"] = observabilitySettings.ServiceNamespace
            }));
            

        otelBuilder.WithTracing(tracing =>
        {
            tracing
                .AddAspNetCoreInstrumentation(options =>
                {
                    options.Filter = ctx =>
                        !ctx.Request.Path.StartsWithSegments("/health") &&
                        !ctx.Request.Path.StartsWithSegments("/metrics");
                })
                .AddHttpClientInstrumentation()
                .AddSqlClientInstrumentation(options =>
                {
                    options.RecordException = true;
                })
                .AddSource("BT.Cache");
                

        });

        otelBuilder.WithMetrics(metrics =>
        {
            metrics
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation()
                .AddMeter("BT.Cache");
        });

        otelBuilder.UseAzureMonitor(options =>
        {
            options.ConnectionString = connectionString;
        });

        return services;
    }

    private static void ConfigureSms(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SmsSettings>(configuration.GetSection(SmsSettings.SectionName));

        var smsSettings = configuration.GetSection(SmsSettings.SectionName).Get<SmsSettings>();
        var twilio = smsSettings?.Twilio;

        if (twilio is not null &&
            !string.IsNullOrWhiteSpace(twilio.AccountSid) &&
            !string.IsNullOrWhiteSpace(twilio.AuthToken))
        {
            TwilioClient.Init(twilio.AccountSid, twilio.AuthToken);
        }


    }

    private static void ConfigureMailKitWithSmtp(IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddOptions<EmailSettings>()
            .Bind(configuration.GetSection(EmailSettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddTransient<ISmtpClient, SmtpClient>();
    }

    private static void ConfigureQuartz(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection string not found in configuration");

        services.AddQuartz(q =>
        {
            // Use a unique ID for this scheduler instance
            q.SchedulerId = "BT_Q_Scheduler";

            q.UseJobFactory<MicrosoftDependencyInjectionJobFactory>();

            q.UsePersistentStore(s =>
            {
                s.UseProperties = true;

                s.UseNewtonsoftJsonSerializer();

                s.UseSqlServer(sq =>
                {
                    sq.ConnectionString = connectionString;
                });

            });
        });

        services.AddQuartzHostedService(opt =>
        {
            opt.WaitForJobsToComplete = true;
        });

    }

    public static IApplicationBuilder UseInfrastructureLoggingMiddleware(this IApplicationBuilder app)
    {
        // Serilog request logging must be first — it wraps the entire pipeline
        // so it can measure the full request duration including auth, routing etc.
        app.UseSerilogRequestLogging(options =>
        {
            options.MessageTemplate =
                "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";

            // Log level based on outcome — errors and 5xx are Error, 4xx are Warning
            options.GetLevel = (httpContext, elapsed, ex) => ex != null
                ? LogEventLevel.Error
                : httpContext.Response.StatusCode >= 500
                    ? LogEventLevel.Error
                    : httpContext.Response.StatusCode >= 400
                        ? LogEventLevel.Warning
                        : LogEventLevel.Information;

            // Enrich each request log with contextual properties
            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                diagnosticContext.Set("UserId", httpContext.User.FindFirst("sub")?.Value ?? "anonymous");
                diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"].FirstOrDefault());
                diagnosticContext.Set("RemoteIpAddress", httpContext.Connection.RemoteIpAddress?.ToString());
                diagnosticContext.Set("CorrelationId", httpContext.TraceIdentifier);
                    
            };
        });

        // Custom middleware — adds X-Correlation-ID response header,
        // optionally logs request bodies at Debug level
        app.UseMiddleware<LoggingMiddleware>();

        return app;
    }

    private enum AuthProvider
    {
        AspNetCoreIdentity,
        Invalid
    }

    private enum ProfileImageStorageProvider
    {
        Local,
        Azurite,
        AzureBlob,
        Invalid
    }

    private enum CacheProvider
    {
        Memory,
        Redis,
        AzureManagedRedis,
        Invalid
    }
}

































































