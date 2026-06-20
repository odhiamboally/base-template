using BT.Persistence.Common.DesignTime;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BT.Persistence.Features.Banking.DataContext;

public class BankingDBContextFactory : IDesignTimeDbContextFactory<BankingDBContext>
{
    public BankingDBContext CreateDbContext(string[] args)
    {
        var configuration = DesignTimeConfigurationFactory.Create();
        var optionsBuilder = new DbContextOptionsBuilder<BankingDBContext>();
        var connectionString = DesignTimeConfigurationFactory.GetConnectionString(configuration, "BankingConnection");

        optionsBuilder.UseSqlServer(connectionString, DesignTimeConfigurationFactory.ConfigureSqlServer);
        return new BankingDBContext(optionsBuilder.Options);
    }
}
