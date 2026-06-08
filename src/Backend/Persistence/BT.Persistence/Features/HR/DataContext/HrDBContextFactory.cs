using BT.Persistence.Common.DesignTime;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BT.Persistence.Features.HR.DataContext;

public class HrDBContextFactory : IDesignTimeDbContextFactory<HrDBContext>
{
    public HrDBContext CreateDbContext(string[] args)
    {
        var configuration = DesignTimeConfigurationFactory.Create();
        var optionsBuilder = new DbContextOptionsBuilder<HrDBContext>();
        var connectionString = DesignTimeConfigurationFactory.GetConnectionString(configuration, "HrConnection");

        optionsBuilder.UseSqlServer(connectionString);
        return new HrDBContext(optionsBuilder.Options);
    }
}
