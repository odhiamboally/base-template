using BT.Persistence.Common.DesignTime;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BT.Persistence.Features.IAM.DataContext;

public class IamPostgreSqlDBContextFactory : IDesignTimeDbContextFactory<IamPostgreSqlDBContext>
{
    public IamPostgreSqlDBContext CreateDbContext(string[] args)
    {
        var configuration = DesignTimeConfigurationFactory.Create();
        var optionsBuilder = new DbContextOptionsBuilder<IamPostgreSqlDBContext>();
        var connectionString = DesignTimeConfigurationFactory.GetConnectionString(configuration, "IamConnection");

        optionsBuilder.UseNpgsql(
            connectionString,
            pgOptions => DesignTimeConfigurationFactory.ConfigurePostgreSql(pgOptions, "__EFMigrationsHistory_IAM"));
        return new IamPostgreSqlDBContext(optionsBuilder.Options);
    }
}
