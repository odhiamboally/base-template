using Microsoft.EntityFrameworkCore;

namespace BT.Persistence.Features.Banking.DataContext;

/// <summary>
/// Derived DbContext used exclusively for generating and routing PostgreSQL migrations.
/// </summary>
public class BankingPostgreSqlDBContext : BankingDBContext
{
    public BankingPostgreSqlDBContext(DbContextOptions<BankingPostgreSqlDBContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(BankingPostgreSqlDBContext).Assembly,
            type => type.Namespace?.StartsWith("BT.Persistence.Features.Banking.EntityConfigurations.PostgreSql", StringComparison.Ordinal) == true);
    }
}
