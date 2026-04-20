using BT.Domain.Contracts.Interfaces.Common;
using BT.Domain.Contracts.Interfaces.Repositories;
using BT.Persistence.Shared.DataContext;
using BT.Persistence.Shared.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace BT.Persistence.Shared.Extensions;

public static class SharedPersistenceDI
{
    public static IServiceCollection AddSharedPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("SharedConnection")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("SharedConnection (or DefaultConnection) not found.");

        services.AddDbContextPool<SharedDbContext>(options =>
        {
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
                sqlOptions.CommandTimeout(30);
                sqlOptions.MigrationsAssembly(Assembly.GetExecutingAssembly().FullName);
            });
        });

        services.AddScoped<ILookupRepository, SharedLookupRepository>();
        services.AddScoped<IEmailTemplateRepository, SharedEmailTemplateRepository>();
        services.AddScoped<IFailedMessageRepository, SharedFailedMessageRepository>();
        services.AddScoped<ISharedUnitOfWork, SharedUnitOfWork>();

        return services;
    }
}
