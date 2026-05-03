using BT.Application.Features.HR.Employees.IntegrationEvents;
using BT.Application.Features.Shared.Notifications.Contracts.Interfaces;
using BT.Infrastructure.Features.HR.Employees.EmailComposers;
using BT.Persistence.Features.HR.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BT.Infrastructure.Features.HR.Extensions;

public static class HrModuleDI
{
    public static IServiceCollection AddHrModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHrPersistence(configuration);
        services.AddScoped<IEmailComposer<EmployeeCreatedIntegrationEvent>, EmployeeWelcomeEmailComposer>();
        return services;
    }
}
