using Asp.Versioning;
using BT.Api.Common.Authorization;
using BT.Api.Logging;
using BT.Api.Middleware;
using BT.Application.Exceptions;
using BT.Domain.Exceptions;
using BT.Infrastructure.Configuration;
using BT.Infrastructure.Messaging.Consumers;
using BT.Persistence.Features.Shared.DataContext;
using FluentValidation;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Hosting;
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
        services.AddAuthorization();
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionAuthorizationPolicyProvider>();
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
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

    public static IServiceCollection ConfigureOutBoxMessagingWithGlobalRetry(this IServiceCollection services, IConfiguration configuration)
    {
        var messagingSettings = configuration.GetSection(MessagingSettings.SectionName).Get<MessagingSettings>() ?? new MessagingSettings();
        if (!messagingSettings.Enabled)
        {
            return services;
        }

        var assembly = typeof(IntegrationEventEmailConsumer<>).Assembly;

        services.AddMassTransit(x =>
        {
            // EF Outbox
            x.AddEntityFrameworkOutbox<SharedDBContext>(o =>
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
        var environment = app.ApplicationServices.GetRequiredService<IHostEnvironment>();

        return app.Use(async (context, next) =>
            {
                context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
                context.Response.Headers.Append("X-Frame-Options", "DENY");
                context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
                context.Response.Headers.Append("Permissions-Policy", "camera=(), microphone=(), geolocation=()");

                var isScalarPage = environment.IsDevelopment()
                    && context.Request.Path.StartsWithSegments("/scalar", StringComparison.OrdinalIgnoreCase);

                var contentSecurityPolicy = isScalarPage
                    ? "default-src 'self'; script-src 'self' 'unsafe-inline' 'unsafe-eval'; style-src 'self' 'unsafe-inline'; img-src 'self' data:; font-src 'self' data:; connect-src 'self'; frame-ancestors 'none'; object-src 'none'; base-uri 'self';"
                    : "default-src 'self'; frame-ancestors 'none'; object-src 'none'; base-uri 'self';";

                context.Response.Headers.Append("Content-Security-Policy", contentSecurityPolicy);

                // API-specific headers
                context.Response.Headers.Append("X-API-Version", "1.0");
                context.Response.Headers.Append("Cache-Control", "no-cache, no-store, must-revalidate");

            await next().ConfigureAwait(false);
        });
    }

}
