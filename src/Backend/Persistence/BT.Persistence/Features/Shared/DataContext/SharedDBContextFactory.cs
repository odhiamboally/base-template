using BT.Persistence.Common.DesignTime;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BT.Persistence.Features.Shared.DataContext;

public class SharedDBContextFactory : IDesignTimeDbContextFactory<SharedDBContext>
{
    public SharedDBContext CreateDbContext(string[] args)
    {
        var configuration = DesignTimeConfigurationFactory.Create();
        var optionsBuilder = new DbContextOptionsBuilder<SharedDBContext>();
        var connectionString = DesignTimeConfigurationFactory.GetConnectionString(configuration, "SharedConnection");

        optionsBuilder.UseSqlServer(connectionString);
        return new SharedDBContext(optionsBuilder.Options);
    }
}
