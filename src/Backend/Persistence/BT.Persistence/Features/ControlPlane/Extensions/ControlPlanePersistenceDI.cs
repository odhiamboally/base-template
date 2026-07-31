using BT.Domain.Features.ControlPlane.Contracts;
using BT.Domain.Features.ControlPlane.Tenants.Contracts.Repositories;
using BT.Persistence.Features.ControlPlane.DataContext;
using BT.Persistence.Features.ControlPlane.Tenants.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Reflection;

namespace BT.Persistence.Features.ControlPlane.Extensions;

public static class ControlPlanePersistenceDI
{
    public static IServiceCollection AddControlPlanePersistence(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        var connectionString = configuration.GetConnectionString("ControlPlaneConnection")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("ControlPlaneConnection (or DefaultConnection) not found.");

        services.Configure<BT.Persistence.Common.Configuration.DatabaseSettings>(configuration.GetSection(BT.Persistence.Common.Configuration.DatabaseSettings.SectionName));
        var dbSettings = configuration.GetSection(BT.Persistence.Common.Configuration.DatabaseSettings.SectionName).Get<BT.Persistence.Common.Configuration.DatabaseSettings>() ?? new BT.Persistence.Common.Configuration.DatabaseSettings();

        void ConfigureDbContextOptions(DbContextOptionsBuilder options)
        {
            if (dbSettings.Provider.Equals("PostgreSql", StringComparison.OrdinalIgnoreCase))
            {
                options.UseNpgsql(connectionString, pgOptions =>
                {
                    pgOptions.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(30), errorCodesToAdd: null);
                    pgOptions.CommandTimeout(30);
                    pgOptions.MigrationsAssembly(Assembly.GetExecutingAssembly().FullName);
                    pgOptions.MigrationsHistoryTable("__EFMigrationsHistory_ControlPlane");
                }).ReplaceService<Microsoft.EntityFrameworkCore.Migrations.IMigrationsSqlGenerator, BT.Persistence.Features.Shared.Migrations.Generators.IdempotentNpgsqlMigrationsSqlGenerator>();
            }
            else
            {
                options.UseSqlServer(connectionString, sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
                    sqlOptions.CommandTimeout(30);
                    sqlOptions.MigrationsAssembly(Assembly.GetExecutingAssembly().FullName);
                    sqlOptions.MigrationsHistoryTable("__EFMigrationsHistory_ControlPlane");
                }).ReplaceService<Microsoft.EntityFrameworkCore.Migrations.IMigrationsSqlGenerator, BT.Persistence.Features.Shared.Migrations.Generators.IdempotentSqlServerMigrationsSqlGenerator>();
            }

            if (environment?.IsDevelopment() == true || environment?.IsStaging() == true)
            {
                options.EnableSensitiveDataLogging();
            }
        }

        if (dbSettings.Provider.Equals("PostgreSql", StringComparison.OrdinalIgnoreCase))
        {
            services.AddDbContext<ControlPlaneDBContext, ControlPlanePostgreSqlDBContext>(ConfigureDbContextOptions);
        }
        else
        {
            services.AddDbContext<ControlPlaneDBContext, ControlPlaneSqlServerDBContext>(ConfigureDbContextOptions);
        }

        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<IDeploymentStampRepository, DeploymentStampRepository>();
        services.AddScoped<IControlPlaneUnitOfWork, ControlPlaneUnitOfWork>();

        return services;
    }
}
