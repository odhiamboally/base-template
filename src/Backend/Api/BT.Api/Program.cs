using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using BT.Api.Extensions;
using BT.Api.Utilities;
using BT.Application.Extensions;
using BT.Infrastructure.Banking.Extensions;
using BT.Infrastructure.Extensions;
using BT.Infrastructure.HR.Extensions;
using BT.Infrastructure.IAM.Extensions;
using BT.Persistence.Extensions;
using BT.Persistence.Shared.Extensions;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Events;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

// ── Bootstrap logger ─────────────────────────────────────────────────────────
// Captures startup errors (before the host and appsettings are loaded).
// Replaced by the fully-configured logger once UseSerilog() runs below.
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting application");

    var builder = WebApplication.CreateBuilder(args);

    // ── Key Vault — production secrets ────────────────────────────────────────
    // Only runs outside Development — User Secrets handle dev.
    // DefaultAzureCredential tries, in order:
    //   1. Environment variables (AZURE_CLIENT_ID etc.) — CI/CD pipelines
    //   2. Workload Identity — AKS pods
    //   3. Managed Identity — Azure App Service / Azure VM (zero config needed)
    //   4. Visual Studio credential
    //   5. Azure CLI credential — works on your machine after `az login`
    // In production on Azure App Service with Managed Identity enabled,
    // option 3 fires automatically — no credentials stored anywhere.
    if (!builder.Environment.IsDevelopment())
    {
        var keyVaultUri = new Uri(
            builder.Configuration["KeyVault:Uri"]
            ?? throw new InvalidOperationException("KeyVault:Uri is not configured."));

        builder.Configuration.AddAzureKeyVault(keyVaultUri, new DefaultAzureCredential());
    }

    // ── Serilog — THE only place it is configured ─────────────────────────────
    // ReadFrom.Configuration reads appsettings.json + appsettings.{Environment}.json.
    // ReadFrom.Services allows enrichers that need DI (e.g. IHttpContextAccessor).
    // Do NOT call services.AddLogging(...AddSerilog) anywhere else — doing so
    // creates a second logger instance and loses enrichers set here.
    builder.Host.UseSerilog((context, services, configuration) =>
    {
        configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithEnvironmentUserName()
            .Enrich.WithProperty("Application", builder.Environment.ApplicationName)
            .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName);
    });

    // ── Services ─────────────────────────────────────────────────────────────
    builder.Services.AddApiServices(builder.Configuration);
    builder.Services.AddApplicationServices(builder.Configuration);
    builder.Services.AddSharedInfrastructure(builder.Configuration, builder.Environment);
    builder.Services.AddIamModule(builder.Configuration, builder.Environment);
    builder.Services.AddHrModule(builder.Configuration);
    builder.Services.AddBankingModule(builder.Configuration);
    builder.Services.AddSharedPersistence(builder.Configuration);
    builder.Services.AddPersistenceServices(builder.Configuration);

    builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                options.JsonSerializerOptions.WriteIndented = builder.Environment.IsDevelopment();
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());

                // Add error handling for problematic types
                options.JsonSerializerOptions.IgnoreReadOnlyProperties = false;
                options.JsonSerializerOptions.IncludeFields = false;

            });

    string corsPolicy = "ApiCorsPolicy";

    builder.Services.AddCors(options =>
    {
        options.AddPolicy(corsPolicy, policy =>
        {
            var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>();

            if (allowedOrigins == null || allowedOrigins.Length == 0)
            {
                Log.Warning("No allowed origins configured for CORS policy");
                throw new InvalidOperationException("CORS policy requires at least one allowed origin to be configured.");
            }

            if (builder.Environment.IsDevelopment())
            {
                // More permissive in development
                policy.WithOrigins(allowedOrigins)
                      .AllowAnyMethod()
                      .AllowAnyHeader()
                      .AllowCredentials()
                      .SetIsOriginAllowedToAllowWildcardSubdomains()
                      .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
            }
            else
            {
                // Strict in production
                policy.WithOrigins(allowedOrigins)
                      .WithMethods("GET", "POST", "PUT", "DELETE", "PATCH")
                      .WithHeaders("Content-Type", "Authorization", "X-Requested-With")
                      .AllowCredentials();
            }
        });
    });

    builder.Services.AddOpenApi("v1", options =>  // Note: "v1" to bypass interceptor
    {
        options.AddOperationTransformer((operation, context, cancellationToken) =>
        {
            // Remove parameters with null schema
            foreach (var param in operation.Parameters?.ToList() ?? [])
            {
                if (param.Schema == null)
                    operation.Parameters?.Remove(param);
            }

            // Remove parameter descriptions with null metadata
            for (int i = context.Description.ParameterDescriptions.Count - 1; i >= 0; i--)
            {
                if (context.Description.ParameterDescriptions[i].ModelMetadata == null)
                    context.Description.ParameterDescriptions.RemoveAt(i);
            }

            return Task.CompletedTask;
        });

        options.AddDocumentTransformer((document, context, _) =>
        {
            document.Info = new()
            {
                Title = "Base Template API",
                Version = "v1.0",
                Description = """
                    Base Template API
                    
                    This API provides a production-ready foundation for:
                    - Authentication and authorization
                    - User and profile management
                    - Domain-driven module extension
                    - Documented, versioned REST endpoints
                    - Secure configuration and operational monitoring
                    """,
                Contact = new()
                {
                    Name = "LlanCore Support",
                    Email = "support@unsacco.org",
                }
            };
            return Task.CompletedTask;
        });

        options.AddSchemaTransformer<SafeSchemaTransformer>();
    });

    builder.Services.AddHealthChecks()
            .AddCheck("self-live", () => HealthCheckResult.Healthy(), tags: ["live"])
            .AddCheck("self-ready", () => HealthCheckResult.Healthy(), tags: ["ready"]);

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }
    else
    {
        app.UseExceptionHandler();
        app.UseHsts();
    }

    app.UseInfrastructureLoggingMiddleware();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }

    app.UseSecurityHeaders();

    app.UseHttpsRedirection();

    app.UseRouting();

    app.UseCors(corsPolicy);

    app.UseRateLimiter();

    //app.UsePreAuthMiddleware();

    app.UseAuthentication();

    app.UsePostAuthMiddleware();

    app.UseAuthorization();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi()
            .CacheOutput()
            .AllowAnonymous();

        app.MapScalarApiReference(options =>
        {
            options.WithTitle("Base Template API")
                   .WithTheme(ScalarTheme.Kepler);
        });

        app.MapGet("/", () => Results.Redirect("/scalar/v1"))
           .ExcludeFromDescription()
           .AllowAnonymous();
    }

    // General health check endpoint
    app.MapHealthChecks("/health", new HealthCheckOptions
    {
        ResponseWriter = async (context, report) =>
        {
            context.Response.ContentType = "application/json";
            var response = new
            {
                status = report.Status.ToString(),
                checks = report.Entries.Select(e => new
                {
                    name = e.Key,
                    status = e.Value.Status.ToString(),
                    description = e.Value.Description,
                    duration = e.Value.Duration.TotalMilliseconds
                }),

                totalDuration = report.TotalDuration.TotalMilliseconds
            };
            await context.Response.WriteAsJsonAsync(response).ConfigureAwait(false);
        }
    });

    // Detailed health check (for monitoring tools)
    app.MapHealthChecks("/health/ready", new HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("ready")
    });

    app.MapHealthChecks("/health/live", new HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("live")
    });

    app.MapControllers();

    Log.Information("BT API started successfully");

    await app.RunAsync().ConfigureAwait(false);
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    // HostAbortedException is thrown intentionally during EF migration tooling runs —
    // do not log it as a fatal crash.
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    // Flush and close all Serilog sinks before the process exits.
    await Log.CloseAndFlushAsync().ConfigureAwait(false);
}