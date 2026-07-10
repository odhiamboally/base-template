using Microsoft.EntityFrameworkCore;

namespace BT.Persistence.Features.Shared.DataContext;

/// <summary>
/// Derived DbContext used exclusively for generating and routing PostgreSQL migrations.
/// </summary>
public class SharedPostgreSqlDBContext : SharedDBContext
{
    public SharedPostgreSqlDBContext(DbContextOptions<SharedPostgreSqlDBContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(SharedPostgreSqlDBContext).Assembly,
            type => type.Namespace?.StartsWith("BT.Persistence.Features.Shared.EntityConfigurations.PostgreSql", StringComparison.Ordinal) == true);
    }
}
