using Microsoft.EntityFrameworkCore;

namespace BT.Persistence.Features.HR.DataContext;

/// <summary>
/// Derived DbContext used exclusively for generating and routing SQL Server migrations.
/// </summary>
public class HrSqlServerDBContext : HrDBContext
{
    public HrSqlServerDBContext(DbContextOptions<HrSqlServerDBContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(HrSqlServerDBContext).Assembly,
            type => type.Namespace?.StartsWith("BT.Persistence.Features.HR.EntityConfigurations.SqlServer", StringComparison.Ordinal) == true);
    }
}
