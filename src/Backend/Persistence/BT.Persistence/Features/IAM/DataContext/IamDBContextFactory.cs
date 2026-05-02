using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace BT.Persistence.Features.IAM.DataContext;

public class IamDbContextFactory : IDesignTimeDbContextFactory<IamDBContext>
{
    public IamDBContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<IamDBContext>();
        var connectionString = configuration.GetConnectionString("IamConnection")
            ?? configuration.GetConnectionString("DefaultConnection");

        optionsBuilder.UseSqlServer(connectionString);
        return new IamDBContext(optionsBuilder.Options);
    }
}
