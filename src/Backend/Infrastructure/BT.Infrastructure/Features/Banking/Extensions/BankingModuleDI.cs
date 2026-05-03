using BT.Application.Features.Banking.Customers.IntegrationEvents;
using BT.Application.Features.Shared.Notifications.Contracts.Interfaces;
using BT.Infrastructure.Features.Banking.Customers.EmailComposers;
using BT.Persistence.Features.Banking.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BT.Infrastructure.Features.Banking.Extensions;

public static class BankingModuleDI
{
    public static IServiceCollection AddBankingModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddBankingPersistence(configuration);
        services.AddScoped<IEmailComposer<CustomerCreatedIntegrationEvent>, CustomerWelcomeEmailComposer>();
        return services;
    }
}
