using BT.Domain.Features.Banking.Contracts;
using BT.Domain.Features.HR.Contracts;
using BT.Domain.Features.IAM.Contracts;
using BT.Domain.Shared.Contracts;
using BT.Domain.Shared.Contracts.Common;
using BT.Domain.Features.IAM.Users.Entities;
using BT.Domain.Features.IAM.Menus.Entities;
using BT.Domain.Features.IAM.Permissions.Entities;
using BT.Domain.Features.IAM.ReferenceData.Entities;
using BT.Domain.Shared.Entities;
using BT.Persistence.Common;
using BT.Persistence.Logging;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BT.Persistence.Features.IAM.DataContext;

public class IamDBContext(
    DbContextOptions<IamDBContext> options,
    ICurrentTenantProvider? tenantProvider = null,
    ICurrentActorProvider? actorProvider = null,
    ILogger<IamDBContext>? logger = null
) : IdentityDbContext<AppUser, AppRole, string>(options), ITenantFilteredDBContext
{
    public DbSet<AppUserProfile> AppUserProfiles { get; set; }
    public DbSet<AppUserTotpSecret> AppUserTotpSecrets { get; set; }
    public DbSet<AppUserSession> AppUserSessions { get; set; }
    public DbSet<AppUserDevice> AppUserDevices { get; set; }
    public DbSet<TempTotpSecret> TempTotpSecrets { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<MenuItem> MenuItems { get; set; }
    public DbSet<PermissionContext> PermissionContexts { get; set; }
    public DbSet<PermissionResource> PermissionResources { get; set; }
    public DbSet<PermissionAction> PermissionActions { get; set; }
    public DbSet<MenuPlacement> MenuPlacements { get; set; }
    public DbSet<MenuIcon> MenuIcons { get; set; }
    public DbSet<MenuRoute> MenuRoutes { get; set; }
    public Guid CurrentTenantId => tenantProvider?.TenantId ?? Guid.Empty;

    private List<IDomainEvent> _collectedDomainEvents = [];
    public IReadOnlyList<IDomainEvent>? GetCollectedDomainEvents() => _collectedDomainEvents?.AsReadOnly();
    public void ClearCollectedDomainEvents() => _collectedDomainEvents?.Clear();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(
            typeof(IamDBContext).Assembly,
            type => type.Namespace?.StartsWith("BT.Persistence.Features.IAM", StringComparison.Ordinal) == true);

        DBContextHelper.ApplyStandardModelConventions(builder, this);
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
                PersistenceLogDefinitions.LogDBContextSaveChangesError(logger, nameof(IamDBContext), ex);
            _collectedDomainEvents?.Clear();
            throw;
        }
    }
}
