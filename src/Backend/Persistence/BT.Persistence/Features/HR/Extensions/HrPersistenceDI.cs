using BT.Domain.Features.Banking.Contracts;
using BT.Domain.Features.HR.Contracts;
using BT.Domain.Features.IAM.Contracts;
using BT.Domain.Shared.Contracts;
using BT.Domain.Shared.Contracts.Common;
using BT.Domain.Features.Banking.Customers.Contracts.Repositories;
using BT.Domain.Features.HR.Employees.Contracts.Repositories;
using BT.Domain.Features.IAM.Users.Contracts.Repositories;
using BT.Domain.Shared.Contracts.Repositories;
using BT.Persistence.Features.HR.DataContext;
using BT.Persistence.Features.HR.Employees.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace BT.Persistence.Features.HR.Extensions;

public static class HrPersistenceDI
{
    public static IServiceCollection AddHrPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("HrConnection")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("HrConnection (or DefaultConnection) not found.");

        services.AddDbContextPool<HrDBContext>(options =>
        {
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
                sqlOptions.CommandTimeout(30);
                sqlOptions.MigrationsAssembly(Assembly.GetExecutingAssembly().FullName);
            });
        });

        services.AddScoped<IEmployeeRepository, HrEmployeeRepository>();
        services.AddScoped<IHrUnitOfWork, HrUnitOfWork>();

        return services;
    }
}
