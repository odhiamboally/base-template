using BT.Domain.Contracts.Interfaces.Common;
using BT.Domain.Contracts.Interfaces.Repositories;
using BT.Persistence.Banking.DataContext;
using BT.Persistence.Banking.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace BT.Persistence.Banking.Extensions;

public static class BankingPersistenceDI
{
    public static IServiceCollection AddBankingPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("BankingConnection")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("BankingConnection (or DefaultConnection) not found.");

        services.AddDbContextPool<BankingDbContext>(options =>
        {
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
                sqlOptions.CommandTimeout(30);
                sqlOptions.MigrationsAssembly(Assembly.GetExecutingAssembly().FullName);
            });
        });

        services.AddScoped<ICustomerRepository, BankingCustomerRepository>();
        services.AddScoped<IBankingUnitOfWork, BankingUnitOfWork>();

        return services;
    }
}
