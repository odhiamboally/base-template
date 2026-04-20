using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace BT.Persistence.HR.DataContext;

public class HrDbContextFactory : IDesignTimeDbContextFactory<HrDbContext>
{
    public HrDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<HrDbContext>();
        var connectionString = configuration.GetConnectionString("HrConnection")
            ?? configuration.GetConnectionString("DefaultConnection");

        optionsBuilder.UseSqlServer(connectionString);
        return new HrDbContext(optionsBuilder.Options);
    }
}
