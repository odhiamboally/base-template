using BT.Persistence.HR.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BT.Infrastructure.HR.Extensions;

public static class HrModuleDI
{
    public static IServiceCollection AddHrModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHrPersistence(configuration);
        return services;
    }
}
