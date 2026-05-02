using BT.Domain.Features.Banking.Contracts;
using BT.Domain.Features.HR.Contracts;
using BT.Domain.Features.IAM.Contracts;
using BT.Domain.Shared.Contracts;
using BT.Domain.Shared.Contracts.Common;
using BT.Domain.Features.Banking.Customers.Contracts.Repositories;
using BT.Domain.Features.HR.Employees.Contracts.Repositories;
using BT.Domain.Features.IAM.Users.Contracts.Repositories;
using BT.Domain.Shared.Contracts.Repositories;
using BT.Persistence.Contracts.Implementations.Interfaces;
using BT.Persistence.Contracts.Implementations.Repositories;
using BT.Persistence.DataContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace BT.Persistence.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistenceServices(this IServiceCollection services, IConfiguration configuration)
    {
        try
        {
            services.AddDBContext(configuration);
            AddServices(services);

            return services;

        }
        catch (Exception)
        {
            throw;
        }
    }

    private static IServiceCollection AddDBContext(this IServiceCollection services,IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' not found. " +
                "Set it via User Secrets (development) or environment variable " +
                "ConnectionStrings__DefaultConnection (staging/production).");
 
        services.AddDbContextPool<DBContext>(options =>
        {
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
 
                sqlOptions.CommandTimeout(30);
 
                // Migrations live in the Persistence assembly
                sqlOptions.MigrationsAssembly(Assembly.GetExecutingAssembly().FullName);
            });
 
            // Development-only: log parameter values and detailed EF errors.
            // Controlled by environment, not by a hardcoded string check.
            // Set via appsettings.Development.json or ASPNETCORE_ENVIRONMENT env var.
            // EnableSensitiveDataLogging and EnableDetailedErrors are set in
            // appsettings.Development.json Serilog overrides — EF picks up
            // the log level from the registered ILoggerFactory automatically.
 
            // NOTE: Do NOT set UseQueryTrackingBehavior(NoTracking) globally.
            // NoTracking breaks SaveChanges for all Add/Update/Delete operations.
            // Use .AsNoTracking() explicitly in read-only query handlers instead.

        });
 
        return services;
    }

    private static IServiceCollection AddServices(this IServiceCollection services)
    {
        // Register domain-specific unit of work interfaces
        services.AddScoped<IBankingUnitOfWork, UnitOfWork>();
        services.AddScoped<IHrUnitOfWork, UnitOfWork>();
        services.AddScoped<IIamUnitOfWork, UnitOfWork>();
        services.AddScoped<ISharedUnitOfWork, UnitOfWork>();

        services.AddTransient(typeof(IRepository<>), typeof(Repository<>));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<ILookupRepository, LookupRepository>();
        services.AddScoped<IFailedMessageRepository, FailedMessageRepository>();
        services.AddScoped<ISessionRepository, SessionRepository>();

        return services;
    }


}

