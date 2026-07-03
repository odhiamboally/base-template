using BT.Domain.Features.Banking.Contracts;
using BT.Domain.Features.HR.Contracts;
using BT.Domain.Features.IAM.Contracts;
using BT.Domain.Shared.Contracts;
using BT.Domain.Shared.Contracts.Common;
using BT.Domain.Features.HR.Employees.Entities;
using BT.Domain.Features.HR.Departments.Entities;
using BT.Domain.Shared.Entities;
using BT.Persistence.Common;
using BT.Persistence.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BT.Persistence.Features.HR.DataContext;

public class HrDBContext(
    DbContextOptions<HrDBContext> options,
    ICurrentTenantProvider? tenantProvider = null,
    ICurrentActorProvider? actorProvider = null,
    ILogger<HrDBContext>? logger = null
) : DbContext(options), ITenantFilteredDBContext
{
    public DbSet<Employee> Employees { get; set; }
    public DbSet<EmployeeNumberSequence> EmployeeNumberSequences { get; set; }
    public DbSet<Department> Departments { get; set; }
    public Guid CurrentTenantId => tenantProvider?.TenantId ?? Guid.Empty;

    private List<IDomainEvent> _collectedDomainEvents = [];
    public IReadOnlyList<IDomainEvent>? GetCollectedDomainEvents() => _collectedDomainEvents?.AsReadOnly();
    public void ClearCollectedDomainEvents() => _collectedDomainEvents?.Clear();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(HrDBContext).Assembly,
            type => type.Namespace?.StartsWith("BT.Persistence.Features.HR", StringComparison.Ordinal) == true);

        DBContextHelper.ApplyStandardModelConventions(modelBuilder, this);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var domainEvents = DBContextHelper.CollectDomainEvents(ChangeTracker);
            DBContextHelper.ClearDomainEventsFromAggregates(ChangeTracker);
            DBContextHelper.UpdateAuditAndSoftDelete(ChangeTracker, actorProvider?.ActorId ?? ICurrentActorProvider.SystemActor, CurrentTenantId);
            var result = await base.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            _collectedDomainEvents ??= [];
            _collectedDomainEvents.AddRange(domainEvents);
            return result;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            foreach (var entry in ex.Entries)
            {
                var entityId = entry.Entity is BaseEntity b ? b.Id.ToString() : "(unknown)";
                if (logger is not null)
                    PersistenceLogDefinitions.LogConcurrencyConflict(logger, entry.Entity.GetType().Name, entityId);
                _ = await entry.GetDatabaseValuesAsync(cancellationToken).ConfigureAwait(false);
            }
            _collectedDomainEvents?.Clear();
            throw;
        }
        catch (DbUpdateException ex)
        {
            foreach (var entry in ex.Entries)
                if (logger is not null)
                    PersistenceLogDefinitions.LogDatabaseError(logger, entry.Entity.GetType().Name, ex);
            _collectedDomainEvents?.Clear();
            throw;
        }
        catch (Exception ex)
        {
            if (logger is not null)
                PersistenceLogDefinitions.LogDBContextSaveChangesError(logger, nameof(HrDBContext), ex);
            _collectedDomainEvents?.Clear();
            throw;
        }
    }
}
