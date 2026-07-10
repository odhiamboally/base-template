using Microsoft.EntityFrameworkCore;

namespace BT.Persistence.Features.HR.DataContext;

/// <summary>
/// Derived DbContext used exclusively for generating and routing PostgreSQL migrations.
/// </summary>
public class HrPostgreSqlDBContext : HrDBContext
{
    public HrPostgreSqlDBContext(DbContextOptions<HrPostgreSqlDBContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(HrPostgreSqlDBContext).Assembly,
            type => type.Namespace?.StartsWith("BT.Persistence.Features.HR.EntityConfigurations.PostgreSql", StringComparison.Ordinal) == true);
    }
}
