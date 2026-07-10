using Microsoft.EntityFrameworkCore;

namespace BT.Persistence.Features.IAM.DataContext;

/// <summary>
/// Derived DbContext used exclusively for generating and routing SQL Server migrations.
/// </summary>
public class IamSqlServerDBContext : IamDBContext
{
    public IamSqlServerDBContext(DbContextOptions<IamSqlServerDBContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(
            typeof(IamSqlServerDBContext).Assembly,
            type => type.Namespace?.StartsWith("BT.Persistence.Features.IAM.EntityConfigurations.SqlServer", StringComparison.Ordinal) == true);
    }
}
