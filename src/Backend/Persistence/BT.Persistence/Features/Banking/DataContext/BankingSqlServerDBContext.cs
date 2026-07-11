using Microsoft.EntityFrameworkCore;

namespace BT.Persistence.Features.Banking.DataContext;

/// <summary>
/// Derived DbContext used exclusively for generating and routing SQL Server migrations.
/// </summary>
public class BankingSqlServerDBContext : BankingDBContext
{
    public BankingSqlServerDBContext(DbContextOptions<BankingSqlServerDBContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(BankingSqlServerDBContext).Assembly,
            type => type.Namespace?.StartsWith("BT.Persistence.Features.Banking.EntityConfigurations.SqlServer", StringComparison.Ordinal) == true);
    }
}
