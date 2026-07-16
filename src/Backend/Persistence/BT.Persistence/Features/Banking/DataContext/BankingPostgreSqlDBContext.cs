using BT.Domain.Shared.Contracts.Common;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BT.Persistence.Features.Banking.DataContext;

/// <summary>
/// Derived DbContext used exclusively for generating and routing PostgreSQL migrations.
/// </summary>
public class BankingPostgreSqlDBContext : BankingDBContext
{
    public BankingPostgreSqlDBContext(
        DbContextOptions<BankingPostgreSqlDBContext> options,
        ICurrentTenantProvider? tenantProvider = null,
        ICurrentActorProvider? actorProvider = null,
        ILogger<BankingDBContext>? logger = null)
        : base(options, tenantProvider, actorProvider, logger)
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
