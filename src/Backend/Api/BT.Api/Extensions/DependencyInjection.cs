using Asp.Versioning;

using Azure.Identity;
using Azure.Storage.Blobs;

using BT.Api.Common.Authorization;
using BT.Api.Configuration;
using BT.Api.Logging;
using BT.Api.Middleware;
using BT.Application.Exceptions;
using BT.Domain.Exceptions;
using BT.Infrastructure.Configuration;
using BT.Infrastructure.Logging;
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
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.IdentityModel.Tokens;

using Serilog.Core;
using Serilog.Events;
using StackExchange.Redis;

using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Reflection;
using System.Text.Json;
using System.Threading.RateLimiting;

namespace BT.Api.Extensions;

internal static partial class DependencyInjection
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        var assembly = typeof(Program).Assembly;

        services.AddExceptionHandler<ApiExceptionHandler>();
        services.AddProblemDetails();
        services.AddRouting(options => options.LowercaseUrls = true);
        services.AddWebEncoders();
        services.AddHttpClient();
        services.AddHttpContextAccessor();
        services.AddSignalR();
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
        ConfigureDataProtection(services, configuration, environment);
        ConfigureResponseCompression(services, configuration);
        ConfigureCustomRateLimiting(services, configuration);

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

    private static void ConfigureDataProtection(IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.AddOptions<DataProtectionSettings>()
            .Bind(configuration.GetSection(DataProtectionSettings.SectionName))
            .Validate(
                settings => !string.IsNullOrWhiteSpace(settings.ApplicationName),
                "DataProtection:ApplicationName is required and must remain stable after protected data has been issued.")
            .ValidateOnStart();

        var dpSettings = configuration.GetSection(DataProtectionSettings.SectionName).Get<DataProtectionSettings>();
        var applicationName = string.IsNullOrWhiteSpace(dpSettings?.ApplicationName)
            ? "BaseTemplate"
            : dpSettings.ApplicationName.Trim();

        var dataProtectionBuilder = services.AddDataProtection()
            .SetApplicationName(applicationName);

        if (dpSettings?.UseExternalKeyStore == true && !string.IsNullOrWhiteSpace(dpSettings.BlobKeyUri))
        {
            var blobClient = new BlobClient(new Uri(dpSettings.BlobKeyUri), new DefaultAzureCredential());
            dataProtectionBuilder.PersistKeysToAzureBlobStorage(blobClient);
            ConfigureDataProtectionKeyEncryption(dataProtectionBuilder, dpSettings);
            return;
        }

        if (dpSettings?.UseExternalKeyStore == true && !string.IsNullOrWhiteSpace(dpSettings.RedisKeyRingConnectionString))
        {
            var key = string.IsNullOrWhiteSpace(dpSettings.RedisKeyRingKey)
                ? "DataProtection-Keys"
                : dpSettings.RedisKeyRingKey.Trim();

            dataProtectionBuilder.PersistKeysToStackExchangeRedis(
                () => ConnectionMultiplexer.Connect(dpSettings.RedisKeyRingConnectionString),
                key);
            ConfigureDataProtectionKeyEncryption(dataProtectionBuilder, dpSettings);
            return;
        }

        var keysPath = !string.IsNullOrWhiteSpace(dpSettings?.KeysPath)
            ? dpSettings.KeysPath
            : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "BTApi", "DataProtection-Keys");

        Directory.CreateDirectory(keysPath);
        dataProtectionBuilder.PersistKeysToFileSystem(new DirectoryInfo(keysPath));

        if (OperatingSystem.IsWindows())
        {
            dataProtectionBuilder.ProtectKeysWithDpapi();
        }

    }

    private static void ConfigureDataProtectionKeyEncryption(IDataProtectionBuilder dataProtectionBuilder, DataProtectionSettings? settings)
    {
        var mode = GetKeyEncryptionMode(settings);

        switch (mode)
        {
            case KeyEncryptionMode.None:
                return;

            case KeyEncryptionMode.KeyVault:
                ConfigureKeyVaultEncryption(dataProtectionBuilder, settings);
                return;

            case KeyEncryptionMode.Certificate:
                ConfigureCertificateEncryption(dataProtectionBuilder, settings);
                return;

            case KeyEncryptionMode.Auto:
                ConfigureAutoEncryption(dataProtectionBuilder, settings);
                return;

            case KeyEncryptionMode.Invalid:
            default:
                throw new InvalidOperationException(
                    $"DataProtection:KeyEncryptionMode '{settings?.KeyEncryptionMode}' is not supported. " +
                    "Supported values: Auto, KeyVault, Certificate, None.");
        }
    }

    private static KeyEncryptionMode GetKeyEncryptionMode(DataProtectionSettings? settings)
    {
        if (string.IsNullOrWhiteSpace(settings?.KeyEncryptionMode))
        {
            return KeyEncryptionMode.Auto;
        }

        return settings.KeyEncryptionMode.Trim() switch
        {
            var mode when mode.Equals("Auto", StringComparison.OrdinalIgnoreCase) => KeyEncryptionMode.Auto,
            var mode when mode.Equals("None", StringComparison.OrdinalIgnoreCase) => KeyEncryptionMode.None,
            var mode when mode.Equals("KeyVault", StringComparison.OrdinalIgnoreCase) => KeyEncryptionMode.KeyVault,
            var mode when mode.Equals("Certificate", StringComparison.OrdinalIgnoreCase) => KeyEncryptionMode.Certificate,
            _ => KeyEncryptionMode.Invalid
        };
    }

    private static void ConfigureAutoEncryption(IDataProtectionBuilder dataProtectionBuilder, DataProtectionSettings? settings)
    {
        if (!string.IsNullOrWhiteSpace(settings?.KeyVaultKeyIdentifier))
        {
            ConfigureKeyVaultEncryption(dataProtectionBuilder, settings);
            return;
        }

        if (!string.IsNullOrWhiteSpace(settings?.CertificateThumbprint))
        {
            ConfigureCertificateEncryption(dataProtectionBuilder, settings);
        }
    }

    private static void ConfigureKeyVaultEncryption(IDataProtectionBuilder dataProtectionBuilder, DataProtectionSettings? settings)
    {
        if (string.IsNullOrWhiteSpace(settings?.KeyVaultKeyIdentifier))
        {
            throw new InvalidOperationException(
                "DataProtection:KeyVaultKeyIdentifier is required when DataProtection:KeyEncryptionMode is KeyVault.");
        }

        dataProtectionBuilder.ProtectKeysWithAzureKeyVault(
            new Uri(settings.KeyVaultKeyIdentifier),
            new DefaultAzureCredential());
    }

    private static void ConfigureCertificateEncryption(IDataProtectionBuilder dataProtectionBuilder, DataProtectionSettings? settings)
    {
        if (string.IsNullOrWhiteSpace(settings?.CertificateThumbprint))
        {
            throw new InvalidOperationException(
                "DataProtection:CertificateThumbprint is required when DataProtection:KeyEncryptionMode is Certificate.");
        }

        ProtectDataProtectionKeysWithCertificate(dataProtectionBuilder, settings.CertificateThumbprint);
    }

    private static void ProtectDataProtectionKeysWithCertificate(
        IDataProtectionBuilder dataProtectionBuilder,
        string certificateThumbprint)
    {
        var thumbprint = certificateThumbprint.Replace(" ", string.Empty);
        var logger = NullLoggerFactory.Instance.CreateLogger("DataProtection");

        using var store = new System.Security.Cryptography.X509Certificates.X509Store(
            System.Security.Cryptography.X509Certificates.StoreName.My,
            System.Security.Cryptography.X509Certificates.StoreLocation.CurrentUser);

        store.Open(System.Security.Cryptography.X509Certificates.OpenFlags.ReadOnly);
        var cert = store.Certificates
            .Find(System.Security.Cryptography.X509Certificates.X509FindType.FindByThumbprint, thumbprint, validOnly: false)
            .OfType<System.Security.Cryptography.X509Certificates.X509Certificate2>()
            .FirstOrDefault();
        store.Close();

        if (cert is null)
        {
            DataProtectionLogging.CertificateNotFound(logger, thumbprint);
            throw new InvalidOperationException($"Data Protection certificate '{thumbprint}' was not found in CurrentUser/My.");
        }

        if (!cert.HasPrivateKey)
        {
            DataProtectionLogging.CertificateNoPrivateKey(logger, thumbprint);
            throw new InvalidOperationException($"Data Protection certificate '{thumbprint}' does not include a private key.");
        }

        dataProtectionBuilder.ProtectKeysWithCertificate(cert);
        DataProtectionLogging.CertificateLoaded(logger, thumbprint);
    }
    private static void ConfigureResponseCompression(IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<ResponseCompressionSettings>()
            .Bind(configuration.GetSection(ResponseCompressionSettings.SectionName))
            .ValidateOnStart();

        var settings = configuration.GetSection(ResponseCompressionSettings.SectionName).Get<ResponseCompressionSettings>()
            ?? new ResponseCompressionSettings();

        if (!settings.Enabled)
        {
            return;
        }

        services.AddResponseCompression(options =>
        {
            options.EnableForHttps = settings.EnableForHttps;
            options.Providers.Add<BrotliCompressionProvider>();
            options.Providers.Add<GzipCompressionProvider>();
        });

        services.Configure<BrotliCompressionProviderOptions>(options =>
        {
            options.Level = CompressionLevel.Fastest;
        });

        services.Configure<GzipCompressionProviderOptions>(options =>
        {
            options.Level = CompressionLevel.Fastest;
        });
    }

    private static void ConfigureCustomRateLimiting(IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<RateLimitSettings>()
            .Bind(configuration.GetSection(RateLimitSettings.SectionName))
            .Validate(HasValidRateLimitSettings, "RateLimiting policies must have PermitLimit > 0, WindowSeconds > 0, and QueueLimit >= 0.")
            .ValidateOnStart();

        var settings = configuration.GetSection(RateLimitSettings.SectionName).Get<RateLimitSettings>()
            ?? new RateLimitSettings();

        services.AddRateLimiter(options =>
            {
                AddFixedWindowPolicy(options, "LoginPolicy", settings.LoginPolicy, settings.Enabled);
                AddFixedWindowPolicy(options, "AuthPolicy", settings.AuthPolicy, settings.Enabled);
                AddFixedWindowPolicy(options, "ApiPolicy", settings.ApiPolicy, settings.Enabled);
                AddFixedWindowPolicy(options, "PasswordResetPolicy", settings.PasswordResetPolicy, settings.Enabled);
                AddFixedWindowPolicy(options, "TwoFactorPolicy", settings.TwoFactorPolicy, settings.Enabled);
                AddFixedWindowPolicy(options, "FileUploadPolicy", settings.FileUploadPolicy, settings.Enabled);
                AddFixedWindowPolicy(options, "RefreshTokenPolicy", settings.RefreshTokenPolicy, settings.Enabled);

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

    private static void AddFixedWindowPolicy(
        RateLimiterOptions options,
        string policyName,
        RateLimitPolicySettings policy,
        bool enabled)
    {
        options.AddFixedWindowLimiter(policyName, configureOptions =>
        {
            configureOptions.PermitLimit = enabled ? policy.PermitLimit : int.MaxValue;
            configureOptions.Window = TimeSpan.FromSeconds(policy.WindowSeconds);
            configureOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            configureOptions.QueueLimit = enabled ? policy.QueueLimit : int.MaxValue;
        });
    }

    private static bool HasValidRateLimitSettings(RateLimitSettings settings)
    {
        return IsValidPolicy(settings.LoginPolicy) &&
               IsValidPolicy(settings.AuthPolicy) &&
               IsValidPolicy(settings.ApiPolicy) &&
               IsValidPolicy(settings.PasswordResetPolicy) &&
               IsValidPolicy(settings.TwoFactorPolicy) &&
               IsValidPolicy(settings.FileUploadPolicy) &&
               IsValidPolicy(settings.RefreshTokenPolicy);
    }

    private static bool IsValidPolicy(RateLimitPolicySettings policy)
    {
        return policy is not null &&
               policy.PermitLimit > 0 &&
               policy.WindowSeconds > 0 &&
               policy.QueueLimit >= 0;
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
            var messagingTransport = GetMessagingTransport(messagingSettings);
            switch (messagingTransport)
            {
                case MessagingTransport.AzureServiceBus:
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
                    break;

                case MessagingTransport.RabbitMq:
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
                    break;

                case MessagingTransport.Invalid:
                default:
                    throw new InvalidOperationException(
                        $"Messaging:Transport '{messagingSettings.Transport}' is not supported. " +
                        "Supported values: RabbitMq, AzureServiceBus.");
            }
        });

        return services;
    }

    private static MessagingTransport GetMessagingTransport(MessagingSettings settings)
    {
        if (string.IsNullOrWhiteSpace(settings.Transport))
        {
            return MessagingTransport.RabbitMq;
        }

        return settings.Transport.Trim() switch
        {
            var transport when transport.Equals("RabbitMq", StringComparison.OrdinalIgnoreCase) => MessagingTransport.RabbitMq,
            var transport when transport.Equals("AzureServiceBus", StringComparison.OrdinalIgnoreCase) => MessagingTransport.AzureServiceBus,
            _ => MessagingTransport.Invalid
        };
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

    private enum KeyEncryptionMode
    {
        Auto,
        None,
        KeyVault,
        Certificate,
        Invalid
    }

    private enum MessagingTransport
    {
        RabbitMq,
        AzureServiceBus,
        Invalid
    }
}
