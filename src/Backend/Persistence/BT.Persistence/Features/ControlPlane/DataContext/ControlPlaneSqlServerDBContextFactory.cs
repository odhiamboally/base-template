using BT.Persistence.Common.DesignTime;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BT.Persistence.Features.ControlPlane.DataContext;

public class ControlPlaneSqlServerDBContextFactory : IDesignTimeDbContextFactory<ControlPlaneSqlServerDBContext>
{
    public ControlPlaneSqlServerDBContext CreateDbContext(string[] args)
    {
        var configuration = DesignTimeConfigurationFactory.Create();
        var optionsBuilder = new DbContextOptionsBuilder<ControlPlaneSqlServerDBContext>();
        var connectionString = DesignTimeConfigurationFactory.GetConnectionString(configuration, "ControlPlaneConnection");

        optionsBuilder.UseSqlServer(
            connectionString,
            sqlOptions => DesignTimeConfigurationFactory.ConfigureSqlServer(sqlOptions, "__EFMigrationsHistory_ControlPlane"));
        return new ControlPlaneSqlServerDBContext(optionsBuilder.Options);
    }
}
