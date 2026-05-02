using BT.Persistence.Features.Banking.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BT.Infrastructure.Features.Banking.Extensions;

public static class BankingModuleDI
{
    public static IServiceCollection AddBankingModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddBankingPersistence(configuration);
        return services;
    }
}
