using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace BT.Persistence.Features.HR.DataContext;

public class HrDBContextFactory : IDesignTimeDbContextFactory<HrDBContext>
{
    public HrDBContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<HrDBContext>();
        var connectionString = configuration.GetConnectionString("HrConnection")
            ?? configuration.GetConnectionString("DefaultConnection");

        optionsBuilder.UseSqlServer(connectionString);
        return new HrDBContext(optionsBuilder.Options);
    }
}
