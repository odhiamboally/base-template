using BT.UI.Blazor.Components;
using BT.UI.Blazor.Configuration;
using BT.UI.Blazor.Features.Banking.Customers.Contracts.Implementations;
using BT.UI.Blazor.Features.HR.Departments.Contracts.Implementations;
using BT.UI.Blazor.Features.HR.Employees.Contracts.Implementations;
using BT.UI.Blazor.Features.IAM.Users.Contracts.Implementations;
using BT.UI.Blazor.Features.IAM.Users.Contracts.Interfaces;
using BT.UI.Blazor.Features.IAM.Users.Implementations;
using BT.UI.Blazor.Features.Shared.BackendApi.Contracts.Implementations;
using BT.UI.Blazor.Features.Shared.BackendApi.Contracts.Interfaces;
using BT.UI.Blazor.Features.Shared.Lookups.Contracts.Implementations;
using BT.UI.Rcl.Features.Banking.Customers.Contracts.Interfaces;
using BT.UI.Rcl.Features.HR.Departments.Contracts.Interfaces;
using BT.UI.Rcl.Features.HR.Employees.Contracts.Interfaces;
using BT.UI.Rcl.Features.IAM.Users.Contracts.Interfaces;
using BT.UI.Rcl.Features.Shared.Lookups.Contracts.Interfaces;
using Microsoft.Extensions.Options;
using MudBlazor.Services;
using Microsoft.Azure.StackExchangeRedis;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);
var cacheSettings = builder.Configuration.GetSection(CacheSettings.SectionName).Get<CacheSettings>();
var redisConnection = cacheSettings?.ConnectionString;
var useDistributedTokenStore = !string.IsNullOrWhiteSpace(redisConnection);

BT.UI.Blazor.Features.Shared.Messaging.UserMessageSanitizer.IsDevelopment = builder.Environment.IsDevelopment();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents(options =>
    {
        options.DetailedErrors = builder.Environment.IsDevelopment()
            && builder.Configuration.GetValue("DetailedErrors", false);
    });

builder.Services.AddMudServices();

builder.Services
    .AddOptions<BackendApiSettings>()
    .Bind(builder.Configuration.GetSection(BackendApiSettings.SectionName))
    .ValidateDataAnnotations()
    .Validate(settings => Uri.TryCreate(settings.BaseUrl, UriKind.Absolute, out _), "Backend API base URL must be absolute.")
    .ValidateOnStart();

builder.Services
    .AddOptions<SessionLifecycleSettings>()
    .Bind(builder.Configuration.GetSection(SessionLifecycleSettings.SectionName))
    .ValidateDataAnnotations()
    .Validate(
        settings => settings.WarningBeforeTimeoutMinutes < settings.IdleTimeoutMinutes,
        "Session lifecycle warning must happen before the idle timeout.")
    .Validate(
        settings => settings.KeepAliveIntervalMinutes < settings.IdleTimeoutMinutes,
        "Session lifecycle keep-alive interval must be shorter than the idle timeout.")
    .ValidateOnStart();

builder.Services
    .AddOptions<CacheSettings>()
    .Bind(builder.Configuration.GetSection(CacheSettings.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

// Keep the existing client-side protected storage implementation for UI use,
// and register a server-side in-memory token store for background operations.
builder.Services.AddScoped<ITokenStorage, TokenStorage>();
builder.Services.AddScoped<DistributedTokenStore>();
builder.Services.AddScoped<ServerTokenStore>();

if (useDistributedTokenStore)
{
    if (cacheSettings != null && cacheSettings.UseEntraId)
    {
        builder.Services.AddStackExchangeRedisCache(options =>
        {
            var configOptions = StackExchange.Redis.ConfigurationOptions.Parse(redisConnection);
            configOptions.Protocol = StackExchange.Redis.RedisProtocol.Resp3;
            configOptions.Password = null; // Clear password to ensure Entra ID token auth is preferred

            var credentialOptions = new Azure.Identity.DefaultAzureCredentialOptions();
            if (!string.IsNullOrWhiteSpace(cacheSettings.PrincipalId))
            {
                credentialOptions.ManagedIdentityClientId = cacheSettings.PrincipalId;
            }

            configOptions.ConfigureForAzureWithTokenCredentialAsync(
                new Azure.Identity.DefaultAzureCredential(credentialOptions)).GetAwaiter().GetResult();

            options.ConfigurationOptions = configOptions;
            options.InstanceName = "bt-ui:";
        });
    }
    else
    {
        builder.Services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnection;
            options.InstanceName = "bt-ui:";
        });
    }
    builder.Services.AddScoped<IServerTokenStore, DistributedTokenStore>();
}
else
{
    builder.Services.AddScoped<IServerTokenStore, ServerTokenStore>();
}

builder.Services.AddScoped<IAuthSession, AuthSession>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IIamAdminService, IamAdminService>();
builder.Services.AddScoped<ILookupService, LookupService>();
builder.Services.AddSingleton<IAuthenticatorQrCodeService, AuthenticatorQrCodeService>();
var httpClientBuilder = builder.Services.AddHttpClient<IBackendApiClient, BackendApiClient>((serviceProvider, client) =>
{
    var settings = serviceProvider.GetRequiredService<IOptions<BackendApiSettings>>().Value;
    client.BaseAddress = new Uri(settings.BaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});

if (builder.Environment.IsDevelopment())
{
    httpClientBuilder.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    });
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
