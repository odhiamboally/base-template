using BT.Domain.Contracts.Interfaces.Common;
using BT.Domain.Contracts.Interfaces.Repositories;
using BT.Persistence.IAM.DataContext;
using BT.Persistence.IAM.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace BT.Persistence.IAM.Extensions;

public static class IamPersistenceDI
{
    public static IServiceCollection AddIamPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("IamConnection")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("IamConnection (or DefaultConnection) not found.");

        services.AddDbContextPool<IamDbContext>(options =>
        {
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
                sqlOptions.CommandTimeout(30);
                sqlOptions.MigrationsAssembly(Assembly.GetExecutingAssembly().FullName);
            });
        });

        services.AddScoped<IUserRepository, IamUserRepository>();
        services.AddScoped<ISessionRepository, IamSessionRepository>();
        services.AddScoped<ITokenRepository, IamTokenRepository>();
        services.AddScoped<IAppUserProfileRepository, IamAppUserProfileRepository>();
        services.AddScoped<IAppUserTotpSecretRepository, IamAppUserTotpSecretRepository>();
        services.AddScoped<ITempTotpSecretRepository, IamTempTotpSecretRepository>();
        services.AddScoped<IIamUnitOfWork, IamUnitOfWork>();

        return services;
    }
}
