using BT.Domain.Features.Banking.Contracts;
using BT.Domain.Features.HR.Contracts;
using BT.Domain.Features.IAM.Contracts;
using BT.Domain.Shared.Contracts;
using BT.Domain.Shared.Contracts.Common;
using BT.Domain.Shared.Entities;
using BT.Domain.Features.Banking.Customers.Lookups;
using BT.Domain.Features.Shared.Lookups.Entities;
using BT.Domain.Features.Shared.Payments.Entities;
using BT.Domain.Features.Shared.TenantSettings.Entities;
using BT.Persistence.Common;
using BT.Persistence.Logging;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BT.Persistence.Features.Shared.DataContext;

public class SharedDBContext : DbContext, ITenantFilteredDBContext
{
    private readonly ICurrentTenantProvider? _tenantProvider;
    private readonly ICurrentActorProvider? _actorProvider;
    private readonly ILogger<SharedDBContext>? _logger;

    public SharedDBContext(
        DbContextOptions<SharedDBContext> options,
        ICurrentTenantProvider tenantProvider,
        ICurrentActorProvider actorProvider,
        ILogger<SharedDBContext>? logger = null
    ) : base(options)
    {
        _tenantProvider = tenantProvider;
        _actorProvider = actorProvider;
        _logger = logger;
    }

    protected SharedDBContext(
        DbContextOptions options,
        ICurrentTenantProvider tenantProvider,
        ICurrentActorProvider actorProvider,
        ILogger<SharedDBContext>? logger = null
    ) : base(options)
    {
        _tenantProvider = tenantProvider;
        _actorProvider = actorProvider;
        _logger = logger;
    }
    public DbSet<EmailTemplate> EmailTemplates { get; set; }
    public DbSet<FailedMessage> FailedMessages { get; set; }
    public DbSet<PaymentRecord> PaymentRecords { get; set; }
    public DbSet<TenantSetting> TenantSettings { get; set; }
    public DbSet<LookupCatalogType> LookupCatalogTypes { get; set; }

    public DbSet<CustomerStatusLookup> CustomerStatuses { get; set; }
    public DbSet<CustomerTypeLookup> CustomerTypes { get; set; }
    public DbSet<SegmentTypeLookup> SegmentTypes { get; set; }
    public DbSet<SubSegmentTypeLookup> SubSegmentTypes { get; set; }
    public DbSet<LineOfBusinessLookup> LinesOfBusiness { get; set; }
    public DbSet<IdentificationTypeLookup> IdentificationTypes { get; set; }
    public DbSet<DirectorRelationTypeLookup> DirectorRelationTypes { get; set; }
    public DbSet<FailedMessageStatusLookup> FailedMessageStatuses { get; set; }
    public Guid CurrentTenantId => _tenantProvider?.TenantId ?? Guid.Empty;

    private List<IDomainEvent> _collectedDomainEvents = [];
    public IReadOnlyList<IDomainEvent>? GetCollectedDomainEvents() => _collectedDomainEvents?.AsReadOnly();
    public void ClearCollectedDomainEvents() => _collectedDomainEvents?.Clear();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        base.OnModelCreating(modelBuilder);

        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(SharedDBContext).Assembly,
            type => type.Namespace?.StartsWith("BT.Persistence.Features.Shared", StringComparison.Ordinal) == true &&
                    !(type.Namespace?.Contains("SqlServer") == true) &&
                    !(type.Namespace?.Contains("PostgreSql") == true));

        DBContextHelper.ApplyStandardModelConventions(modelBuilder, this);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var domainEvents = DBContextHelper.CollectDomainEvents(ChangeTracker);
            DBContextHelper.ClearDomainEventsFromAggregates(ChangeTracker);
            DBContextHelper.UpdateAuditAndSoftDelete(ChangeTracker, _actorProvider?.ActorId ?? ICurrentActorProvider.SystemActor, CurrentTenantId);
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
                if (_logger is not null)
                    PersistenceLogDefinitions.LogConcurrencyConflict(_logger, entry.Entity.GetType().Name, entityId);
                _ = await entry.GetDatabaseValuesAsync(cancellationToken).ConfigureAwait(false);
            }
            _collectedDomainEvents?.Clear();
            throw;
        }
        catch (DbUpdateException ex)
        {
            foreach (var entry in ex.Entries)
                if (_logger is not null)
                    PersistenceLogDefinitions.LogDatabaseError(_logger, entry.Entity.GetType().Name, ex);
            _collectedDomainEvents?.Clear();
            throw;
        }
        catch (Exception ex)
        {
            if (_logger is not null)
                PersistenceLogDefinitions.LogDBContextSaveChangesError(_logger, nameof(SharedDBContext), ex);
            _collectedDomainEvents?.Clear();
            throw;
        }
    }
}
