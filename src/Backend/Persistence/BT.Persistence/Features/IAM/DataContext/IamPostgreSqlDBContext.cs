using Microsoft.EntityFrameworkCore;

namespace BT.Persistence.Features.IAM.DataContext;

/// <summary>
/// Derived DbContext used exclusively for generating and routing PostgreSQL migrations.
/// </summary>
public class IamPostgreSqlDBContext : IamDBContext
{
    public IamPostgreSqlDBContext(DbContextOptions<IamPostgreSqlDBContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(
            typeof(IamPostgreSqlDBContext).Assembly,
            type => type.Namespace?.StartsWith("BT.Persistence.Features.IAM.EntityConfigurations.PostgreSql", StringComparison.Ordinal) == true);
    }
}
