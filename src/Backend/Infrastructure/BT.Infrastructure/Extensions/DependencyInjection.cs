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
using BT.SharedKernel.Configurations;
using FluentValidation;
using FluentEmail.MailKitSmtp;
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

namespace BT.Infrastructure.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddSharedInfrastructure(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));

            services.AddSingleton(JsonSerializerOptionsFactory.Create());
            services.Configure<ObservabilitySettings>(configuration.GetSection(ObservabilitySettings.SectionName));
            services.Configure<AuthProviderSettings>(configuration.GetSection(AuthProviderSettings.SectionName));
            services.Configure<ApiSettings>(configuration.GetSection(nameof(ApiSettings)));

            var cacheSettings = configuration.GetSection("CacheSettings").Get<CacheSettings>() 
                ?? throw new InvalidOperationException("CacheSettings not found.");

            services.Configure<SessionSettings>(configuration.GetSection("SecuritySettings:SessionSettings"));
            ConfigureDistributedCache(services, configuration, cacheSettings);
            ConfigureMailKitWithSmtp(services, configuration);
            ConfigureSms(services, configuration);
            ConfigureQuartz(services, configuration);
            ConfigureSerilogEnrichers(services);
            var observabilitySettings = configuration.GetSection(ObservabilitySettings.SectionName).Get<ObservabilitySettings>() ?? new ObservabilitySettings();
            var authProviderSettings = configuration.GetSection(AuthProviderSettings.SectionName).Get<AuthProviderSettings>() ?? new AuthProviderSettings();
            ConfigureObservability(services, configuration, environment, observabilitySettings);
            AddServices(services, authProviderSettings);
            
        return services;
    }

    private static IServiceCollection AddServices(this IServiceCollection services, AuthProviderSettings authProviderSettings)
    {
        services.AddScoped<IIntegrationEventPublisher, MassTransitIntegrationEventPublisher>();
        services.AddHttpClient<IApiService, ApiService>();

        if (!authProviderSettings.Enabled)
        {
            return services;
        }

        if (!string.Equals(authProviderSettings.Provider, "AspNetCoreIdentity", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Unsupported AuthProvider: {authProviderSettings.Provider}");
        }

        services.AddScoped<IEmailService, FluentMailService>();
        services.AddScoped<ISmsService, SmsService>();
        services.AddScoped<IBackgroundJobService, BackgroundJobService>();
        services.AddScoped<IEncryptionService, EncryptionService>();

        return services;
    }

    internal static void ConfigureAuthentication(IServiceCollection services, IConfiguration configuration)
    {
        var jwtSettings = new JwtSettings();
            configuration.GetSection("JwtSettings").Bind(jwtSettings);

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
        if (!string.IsNullOrWhiteSpace(cacheSettings.Azure?.ConnectionString))
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = cacheSettings.Azure!.ConnectionString;
            });
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
        ArgumentNullException.ThrowIfNull(observabilitySettings, nameof(observabilitySettings));
        if (!observabilitySettings.Enabled)
        {
            return services;
        }

        var connectionString = observabilitySettings.AzureMonitor.ConnectionString;

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            Console.WriteLine(
                "[WARN] Observability:AzureMonitor:ConnectionString not configured. " +
                "OpenTelemetry export to Azure Monitor is disabled. " +
                "Ensure Key Vault secret 'Observability--AzureMonitor--ConnectionString' is set.");

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
        var emailSettings = configuration.GetSection("EmailSettings").Get<EmailSettings>()
            ?? throw new InvalidOperationException("EmailSettings section not found in configuration");

        services
            .AddOptions<EmailSettings>()
            .Bind(configuration.GetSection(EmailSettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services
            .AddFluentEmail(emailSettings.FromAddress, emailSettings.DisplayName)
            .AddMailKitSender(new SmtpClientOptions
            {
                Server = emailSettings.Host,
                Port = emailSettings.Port,
                User = emailSettings.Username,
                Password = emailSettings.Password,
                UseSsl = emailSettings.EnableSsl,
                RequiresAuthentication = !string.IsNullOrWhiteSpace(emailSettings.Username)
            });

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


}

































































