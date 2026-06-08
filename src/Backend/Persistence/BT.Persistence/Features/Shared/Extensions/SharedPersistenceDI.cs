using BT.Domain.Features.Banking.Contracts;
using BT.Domain.Features.HR.Contracts;
using BT.Domain.Features.IAM.Contracts;
using BT.Domain.Shared.Contracts;
using BT.Domain.Shared.Contracts.Common;
using BT.Domain.Features.Banking.Customers.Contracts.Repositories;
using BT.Domain.Features.HR.Employees.Contracts.Repositories;
using BT.Domain.Features.IAM.Users.Contracts.Repositories;
using BT.Domain.Shared.Contracts.Repositories;
using BT.Persistence.Features.Shared;
using BT.Persistence.Features.Shared.DataContext;
using BT.Persistence.Features.Shared.EmailTemplates.Repositories;
using BT.Persistence.Features.Shared.FailedMessages.Repositories;
using BT.Persistence.Features.Shared.Lookups.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace BT.Persistence.Features.Shared.Extensions;

public static class SharedPersistenceDI
{
    public static IServiceCollection AddSharedPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("SharedConnection")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("SharedConnection (or DefaultConnection) not found.");

        services.AddDbContextPool<SharedDBContext>(options =>
        {
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
                sqlOptions.CommandTimeout(30);
                sqlOptions.MigrationsAssembly(Assembly.GetExecutingAssembly().FullName);
                sqlOptions.MigrationsHistoryTable("__EFMigrationsHistory_Shared");
            });
        });

        services.AddScoped<ILookupRepository, SharedLookupRepository>();
        services.AddScoped<IEmailTemplateRepository, SharedEmailTemplateRepository>();
        services.AddScoped<IFailedMessageRepository, SharedFailedMessageRepository>();
        services.AddScoped<ISharedUnitOfWork, SharedUnitOfWork>();

        return services;
    }
}
