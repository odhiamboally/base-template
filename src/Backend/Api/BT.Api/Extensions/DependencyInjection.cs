using Asp.Versioning;
using BT.Api.Middleware;
using BT.Application.Exceptions;
using BT.Domain.Exceptions;
using BT.Infrastructure.Configuration;
using BT.Infrastructure.Messaging.Consumers;
using BT.Persistence.Shared.DataContext;
using FluentValidation;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using Serilog.Core;
using Serilog.Events;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text.Json;
using System.Threading.RateLimiting;
using BT.Infrastructure.Logging;

namespace BT.Api.Extensions;

internal static class DependencyInjection
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        var assembly = typeof(Program).Assembly;

        services.AddExceptionHandler<ApiExceptionHandler>();
        services.AddProblemDetails();
        services.AddRouting(options => options.LowercaseUrls = true);
        services.AddWebEncoders();
        services.AddHttpClient();
        services.AddHttpContextAccessor();
        services.AddApiVersioning(config =>
        {
            config.DefaultApiVersion = new ApiVersion(1, 0);
            config.AssumeDefaultVersionWhenUnspecified = true;
            config.ReportApiVersions = true;
            config.ApiVersionReader = new UrlSegmentApiVersionReader();
        });

        ConfigureHttpResilience(services, configuration);
        ConfigureDataProtection(services, configuration);
        ConfigureCustomRateLimiting(services);

        services.AddValidatorsFromAssembly(assembly);

        return services;
    }

    private static void ConfigureAuthentication(IServiceCollection services, IConfiguration configuration)
    {
        try
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

            services.Configure<IdentityOptions>(options =>
            {
                ConfigureIdentityOptions(options);
            });


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
        catch (Exception)
        {
            throw;
        }
    }

    private static void ConfigureHttpResilience(IServiceCollection services, IConfiguration configuration)
    {
        var resilienceSettings = configuration.GetSection(ResilienceSettings.SectionName).Get<ResilienceSettings>() ?? new ResilienceSettings();
        if (!resilienceSettings.Enabled)
        {
            return;
        }

        services.ConfigureHttpClientDefaults(http =>
        {
            http.AddStandardResilienceHandler();
        });

    }

    private static void ConfigureDataProtection(IServiceCollection services, IConfiguration configuration)
    {
        var dataProtection = services.AddDataProtection()
            .SetApplicationName("LlanCore.BaseTemplate.API");

        var keysPath = configuration["DataProtection:KeysPath"];
        if (!string.IsNullOrWhiteSpace(keysPath))
        {
            var directoryInfo = new DirectoryInfo(keysPath);
            if (!directoryInfo.Exists)
            {
                directoryInfo.Create();
            }

            dataProtection.PersistKeysToFileSystem(directoryInfo);
        }

        if (OperatingSystem.IsWindows())
        {
            dataProtection.ProtectKeysWithDpapi();
        }
    }

    private static void ConfigureCustomRateLimiting(IServiceCollection services)
    {
        try
        {
            services.AddRateLimiter(options =>
            {
                options.AddFixedWindowLimiter("LoginPolicy", configureOptions =>
                {
                    configureOptions.PermitLimit = 10;       // More reasonable for login
                    configureOptions.Window = TimeSpan.FromMinutes(2); // Longer window
                    configureOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    configureOptions.QueueLimit = 3;
                });

                options.AddFixedWindowLimiter("AuthPolicy", configureOptions =>
                {
                    configureOptions.PermitLimit = 5;        // 5 attempts
                    configureOptions.Window = TimeSpan.FromMinutes(1); // per minute
                    configureOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    configureOptions.QueueLimit = 2;         // Allow 2 queued requests
                });

                options.AddFixedWindowLimiter("ApiPolicy", configureOptions =>
                {
                    configureOptions.PermitLimit = 100;      // 100 requests
                    configureOptions.Window = TimeSpan.FromMinutes(1); // per minute
                    configureOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    configureOptions.QueueLimit = 10;
                });

                options.AddFixedWindowLimiter("PasswordResetPolicy", configureOptions =>
                {
                    configureOptions.PermitLimit = 3;        // Only 3 attempts
                    configureOptions.Window = TimeSpan.FromMinutes(15); // per 15 minutes
                    configureOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    configureOptions.QueueLimit = 0;          // No queuing for security
                });

                options.AddFixedWindowLimiter("TwoFactorPolicy", configureOptions =>
                {
                    configureOptions.PermitLimit = 10;       // 10 attempts
                    configureOptions.Window = TimeSpan.FromMinutes(5); // per 5 minutes
                    configureOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    configureOptions.QueueLimit = 3;
                });

                options.AddFixedWindowLimiter("FileUploadPolicy", configureOptions =>
                {
                    configureOptions.PermitLimit = 20;       // 20 uploads
                    configureOptions.Window = TimeSpan.FromHours(1); // per hour
                    configureOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    configureOptions.QueueLimit = 5;
                });

                options.AddFixedWindowLimiter("RefreshTokenPolicy", configureOptions =>
                {
                    configureOptions.PermitLimit = 10;
                    configureOptions.Window = TimeSpan.FromMinutes(1); // per hour
                    configureOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    configureOptions.QueueLimit = 2;
                });



                // Rate limit exceeded response
                options.OnRejected = async (context, token) =>
                {
                    context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    context.HttpContext.Response.ContentType = "application/json";

                    var retryAfter = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfterValue)
                        ? retryAfterValue.TotalSeconds.ToString(CultureInfo.InvariantCulture)
                        : "60";

                    context.HttpContext.Response.Headers.RetryAfter = retryAfter;

                    var errorResponse = new
                    {
                        error = "rate_limit_exceeded",
                        message = $"Rate limit exceeded. Try again in {retryAfter} seconds.",
                        retryAfter
                    };

                    await context.HttpContext.Response.WriteAsync(
                        JsonSerializer.Serialize(errorResponse), token).ConfigureAwait(false);
                };


            });
        }
        catch (Exception)
        {
            //ToDo: Log the exception for debugging purposes
            throw;
        }

    }

    public static IServiceCollection ConfigureOutBoxMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {
            // 1. Add the EF Outbox
            x.AddEntityFrameworkOutbox<SharedDbContext>(o =>
            {
                o.UseSqlServer();
                o.UseBusOutbox(); // Integrates with the IPublishEndpoint

                // How long a message ID is kept to prevent processing it twice (Default: 24 hours)
                o.DuplicateDetectionWindow = TimeSpan.FromHours(24);

                // How often the background service checks for expired records to delete
                // Note: In newer MT versions, this is often managed by the QueryDelay
                o.QueryDelay = TimeSpan.FromSeconds(30);

                // Limits how many records are deleted in a single cleanup batch to avoid DB locks
                o.QueryMessageLimit = 100;
            });

            x.AddConsumer<CustomerCreatedEventConsumer>();
            //x.AddConsumer<CustomerUpdatedNotificationConsumer>();
            //x.AddConsumer<CustomerDeletedAuditConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(configuration["RabbitMq:Host"] ?? "localhost", "/");
                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }

    public static IServiceCollection ConfigureOutBoxMessagingWithGlobalRetry(this IServiceCollection services, IConfiguration configuration)
    {
        var messagingSettings = configuration.GetSection(MessagingSettings.SectionName).Get<MessagingSettings>() ?? new MessagingSettings();
        var assembly = typeof(CustomerCreatedEventConsumer).Assembly;

        services.AddMassTransit(x =>
        {
            // EF Outbox
            x.AddEntityFrameworkOutbox<SharedDbContext>(o =>
            {
                o.UseSqlServer();
                o.UseBusOutbox();
                o.DuplicateDetectionWindow = TimeSpan.FromHours(24);
                o.QueryDelay = TimeSpan.FromSeconds(30);
                o.QueryMessageLimit = 100;
            });

            // Register all consumers from assembly
            x.AddConsumers(assembly);

            // Global retry configuration
            if (string.Equals(messagingSettings.Transport, "AzureServiceBus", StringComparison.OrdinalIgnoreCase))
            {
                x.UsingAzureServiceBus((context, cfg) =>
                {
                    var connectionString = messagingSettings.AzureServiceBus.ConnectionString;
                    if (string.IsNullOrWhiteSpace(connectionString))
                        throw new InvalidOperationException("Messaging:AzureServiceBus:ConnectionString is required when Messaging:Transport is AzureServiceBus");

                    cfg.Host(connectionString);

                    cfg.UseMessageRetry(r =>
                    {
                        r.Exponential
                        (
                            retryLimit: 5,
                            minInterval: TimeSpan.FromSeconds(2),
                            maxInterval: TimeSpan.FromMinutes(2),
                            intervalDelta: TimeSpan.FromSeconds(10)
                        );

                        r.Handle<EmailServiceException>();
                        r.Handle<TimeoutException>();
                        r.Handle<HttpRequestException>();
                        r.Handle<SqlException>();

                        r.Ignore<ArgumentException>();
                        r.Ignore<InvalidEmailAddressException>();
                        r.Ignore<DomainException>();
                    });

                    cfg.ConfigureEndpoints(context);
                    cfg.UseMessageScope(context);
                    cfg.UseConsumeFilter(typeof(LoggingConsumeFilter<>), context);
                    cfg.ConnectConsumerConfigurationObserver(new ConsumerLoggingObserver());
                });
            }
            else
            {
                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(
                        messagingSettings.RabbitMq.Host,
                        messagingSettings.RabbitMq.VirtualHost,
                        h =>
                        {
                            h.Username(messagingSettings.RabbitMq.Username);
                            h.Password(messagingSettings.RabbitMq.Password);
                        });

                    cfg.UseMessageRetry(r =>
                    {
                        r.Exponential
                        (
                            retryLimit: 5,
                            minInterval: TimeSpan.FromSeconds(2),
                            maxInterval: TimeSpan.FromMinutes(2),
                            intervalDelta: TimeSpan.FromSeconds(10)
                        );

                        r.Handle<EmailServiceException>();
                        r.Handle<TimeoutException>();
                        r.Handle<HttpRequestException>();
                        r.Handle<SqlException>();
                        r.Handle<RabbitMqConnectionException>();

                        r.Ignore<ArgumentException>();
                        r.Ignore<InvalidEmailAddressException>();
                        r.Ignore<DomainException>();
                    });

                    cfg.ConfigureEndpoints(context);
                    cfg.UseMessageScope(context);
                    cfg.UseConsumeFilter(typeof(LoggingConsumeFilter<>), context);
                    cfg.ConnectConsumerConfigurationObserver(new ConsumerLoggingObserver());
                });
            }
        });

        return services;
    }

    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
    {
        try
        {
            return app.Use(async (context, next) =>
            {
                context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
                context.Response.Headers.Append("X-Frame-Options", "DENY");
                context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
                context.Response.Headers.Append("Permissions-Policy", "camera=(), microphone=(), geolocation=()");
                context.Response.Headers.Append("Content-Security-Policy", "default-src 'self'; frame-ancestors 'none'; object-src 'none'; base-uri 'self';");

                // API-specific headers
                context.Response.Headers.Append("X-API-Version", "1.0");
                context.Response.Headers.Append("Cache-Control", "no-cache, no-store, must-revalidate");

                await next().ConfigureAwait(false);
            });
        }
        catch (Exception)
        {

            throw;
        }

    }

    private static void ConfigureIdentityOptions(IdentityOptions options)
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

    private static void ConfigureJwtBearer(JwtBearerOptions options, JwtSettings jwtSettings)
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
                    //ToDo: Log other JWT validation errors

                    // Check for NotBefore issues specifically
                    if (context.Exception.Message.Contains("NotBefore") || context.Exception.Message.Contains("not yet valid"))
                    {
                        //ToDo: Log
                    }
                }

                return Task.CompletedTask;


            }
        };
    }




}
