using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace BT.Persistence.Features.Shared.DataContext;

public class SharedDBContextFactory : IDesignTimeDbContextFactory<SharedDBContext>
{
    public SharedDBContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<SharedDBContext>();
        var connectionString = configuration.GetConnectionString("SharedConnection")
            ?? configuration.GetConnectionString("DefaultConnection");

        optionsBuilder.UseSqlServer(connectionString);
        return new SharedDBContext(optionsBuilder.Options);
    }
}
