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

var builder = WebApplication.CreateBuilder(args);

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

// Protected browser storage is the durable browser source. The scoped server
// store only bridges periods where JS interop is unavailable within a circuit.
builder.Services.AddScoped<ITokenStorage, TokenStorage>();
builder.Services.AddScoped<IServerTokenStore, ServerTokenStore>();

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
