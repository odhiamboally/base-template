using BT.Domain.Shared.Contracts.Common;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BT.Persistence.Features.Banking.DataContext;

/// <summary>
/// Derived DbContext used exclusively for generating and routing SQL Server migrations.
/// </summary>
public class BankingSqlServerDBContext : BankingDBContext
{
    public BankingSqlServerDBContext(
        DbContextOptions<BankingSqlServerDBContext> options,
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
            typeof(BankingSqlServerDBContext).Assembly,
            type => type.Namespace?.StartsWith("BT.Persistence.Features.Banking.EntityConfigurations.SqlServer", StringComparison.Ordinal) == true);
    }
}
