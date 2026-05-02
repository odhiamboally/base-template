using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace BT.Persistence.Features.Banking.DataContext;

public class BankingDBContextFactory : IDesignTimeDbContextFactory<BankingDBContext>
{
    public BankingDBContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<BankingDBContext>();
        var connectionString = configuration.GetConnectionString("BankingConnection")
            ?? configuration.GetConnectionString("DefaultConnection");

        optionsBuilder.UseSqlServer(connectionString);
        return new BankingDBContext(optionsBuilder.Options);
    }
}
