using BT.Domain.Contracts.Interfaces.Common;
using BT.Domain.Entities;
using BT.Persistence.Common;
using BT.Persistence.Logging;
using BT.Persistence.Seeds;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BT.Persistence.HR.DataContext;

public class HrDbContext(
    DbContextOptions<HrDbContext> options,
    ILogger<HrDbContext>? logger = null
) : DbContext(options)
{
    public DbSet<Employee> Employees { get; set; }

    private List<IDomainEvent> _collectedDomainEvents = [];
    public IReadOnlyList<IDomainEvent>? GetCollectedDomainEvents() => _collectedDomainEvents?.AsReadOnly();
    public void ClearCollectedDomainEvents() => _collectedDomainEvents?.Clear();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        base.OnModelCreating(modelBuilder);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType))
                modelBuilder.Entity(entityType.ClrType)
                    .HasQueryFilter(DbContextHelper.CreateSoftDeleteFilter(entityType.ClrType));

            if (typeof(ICursorPaginable).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType).HasKey(nameof(ICursorPaginable.Id));
                modelBuilder.Entity(entityType.ClrType)
                    .HasIndex(nameof(ICursorPaginable.CreatedAt), nameof(ICursorPaginable.Id))
                    .HasDatabaseName($"IX_{entityType.GetTableName()}_CreatedAt_Id");
            }
        }

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HrDbContext).Assembly);
        modelBuilder.Entity<Employee>().HasData(EmployeeSeed.GetSeedData());
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var domainEvents = DbContextHelper.CollectDomainEvents(ChangeTracker);
            DbContextHelper.ClearDomainEventsFromAggregates(ChangeTracker);
            DbContextHelper.UpdateAuditAndSoftDelete(ChangeTracker, "System");
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
        catch
        {
            _collectedDomainEvents?.Clear();
            throw;
        }
    }
}
