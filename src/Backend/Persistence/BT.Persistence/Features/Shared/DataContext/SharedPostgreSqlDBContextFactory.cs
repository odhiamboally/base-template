using BT.Persistence.Common.DesignTime;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BT.Persistence.Features.Shared.DataContext;

public class SharedPostgreSqlDBContextFactory : IDesignTimeDbContextFactory<SharedPostgreSqlDBContext>
{
    public SharedPostgreSqlDBContext CreateDbContext(string[] args)
    {
        var configuration = DesignTimeConfigurationFactory.Create();
        var optionsBuilder = new DbContextOptionsBuilder<SharedPostgreSqlDBContext>();
        var connectionString = DesignTimeConfigurationFactory.GetConnectionString(configuration, "SharedConnection");

        optionsBuilder.UseNpgsql(
            connectionString,
            pgOptions => DesignTimeConfigurationFactory.ConfigurePostgreSql(pgOptions, "__EFMigrationsHistory_Shared"));
        return new SharedPostgreSqlDBContext(optionsBuilder.Options, null!, null!);
    }
}
