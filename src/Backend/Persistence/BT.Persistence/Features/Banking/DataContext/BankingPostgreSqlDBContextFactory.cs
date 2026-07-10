using BT.Persistence.Common.DesignTime;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BT.Persistence.Features.Banking.DataContext;

public class BankingPostgreSqlDBContextFactory : IDesignTimeDbContextFactory<BankingPostgreSqlDBContext>
{
    public BankingPostgreSqlDBContext CreateDbContext(string[] args)
    {
        var configuration = DesignTimeConfigurationFactory.Create();
        var optionsBuilder = new DbContextOptionsBuilder<BankingPostgreSqlDBContext>();
        var connectionString = DesignTimeConfigurationFactory.GetConnectionString(configuration, "BankingConnection");

        optionsBuilder.UseNpgsql(
            connectionString,
            pgOptions => DesignTimeConfigurationFactory.ConfigurePostgreSql(pgOptions, "__EFMigrationsHistory_Banking"));
        return new BankingPostgreSqlDBContext(optionsBuilder.Options);
    }
}
