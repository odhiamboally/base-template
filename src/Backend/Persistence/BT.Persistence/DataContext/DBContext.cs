using BT.Domain.Banking.Contracts;
using BT.Domain.HR.Contracts;
using BT.Domain.IAM.Contracts;
using BT.Domain.Shared.Contracts;
using BT.Domain.Shared.Contracts.Common;
using BT.Domain.Banking.Entities;
using BT.Domain.HR.Entities;
using BT.Domain.IAM.Entities;
using BT.Domain.Shared.Entities;
using BT.Domain.Banking.Lookups;
using BT.Domain.Shared.Lookups;
using BT.Persistence.Seeds;
using BT.Persistence.Logging;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Data.Common;
using System.Data;

namespace BT.Persistence.DataContext;

public class DBContext(
    DbContextOptions<DBContext> options,
    ILogger<DBContext>? logger = null
    
) : DbContext(options)
{
    
    #region Sets

    public DbSet<AppUserProfile> AppUserProfiles { get; set; }
    public DbSet<AppUserTotpSecret> AppUserTotpSecrets { get; set; }
    public DbSet<AppUserSession> AppUserSessions { get; set; }
    public DbSet<AppUserDevice> AppUserDevices { get; set; }
    public DbSet<Customer> Clients { get; set; }
    public DbSet<Director> Directors { get; set; }
    public DbSet<Employee> Employees { get; set; }
    public DbSet<EmailTemplate> EmailTemplates { get; set; }
    public DbSet<FailedMessage> FailedMessages { get; set; }
    public DbSet<TempTotpSecret> TempTotpSecrets { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    //public DbSet<OutboxMessage> OutboxMessages { get; set; }


    public DbSet<CustomerStatusLookup> ClientStatuses { get; set; }
    public DbSet<CustomerTypeLookup> ClientTypes { get; set; }
    public DbSet<SegmentTypeLookup> SegmentTypes { get; set; }
    public DbSet<SubSegmentTypeLookup> SubSegmentTypes { get; set; }
    public DbSet<LineOfBusinessLookup> LinesOfBusiness { get; set; }
    public DbSet<IdentificationTypeLookup> IdentificationTypes { get; set; }
    public DbSet<DirectorRelationTypeLookup> DirectorRelationTypes { get; set; }
    public DbSet<OutboxMessageTypeLookup> OutboxMessageTypes { get; set; }
    public DbSet<FailedMessageStatusLookup> FailedMessageStatuses { get; set; }

    #endregion

    List<IDomainEvent> _collectedDomainEvents = [];

    public IReadOnlyList<IDomainEvent>? GetCollectedDomainEvents() => _collectedDomainEvents?.AsReadOnly();

    public void ClearCollectedDomainEvents() => _collectedDomainEvents?.Clear();



    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        base.OnModelCreating(modelBuilder);

        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .HasQueryFilter(CreateSoftDeleteFilter(entityType.ClrType));
            }

            if (typeof(ICursorPaginable).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType).HasKey(nameof(ICursorPaginable.Id));

                modelBuilder.Entity(entityType.ClrType)
                    .HasIndex(nameof(ICursorPaginable.CreatedAt), nameof(ICursorPaginable.Id))
                    .HasDatabaseName($"IX_{entityType.GetTableName()}_CreatedAt_Id");
            }
        }
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DBContext).Assembly);

        modelBuilder.Entity<Employee>().HasData(EmployeeSeed.GetSeedData());

    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Collect domain events from aggregates BEFORE any changes
            var domainEvents = CollectDomainEvents();

            // Clear them from aggregates so they don't fire twice
            ClearDomainEventsFromAggregates();

            UpdateAuditAndSoftDelete(GetCurrentUserId());

            var result = await base.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            // Store events for later publishing (outbox will handle)
            if (_collectedDomainEvents is not null)
            {
                _collectedDomainEvents.AddRange(domainEvents);
            }

            return result;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            // Log which entities were involved
            foreach (var entry in ex.Entries)
            {
                var entityTypeName = entry.Entity.GetType().Name;
                var entityId = entry.Entity is BaseEntity baseEntity ? baseEntity.Id.ToString() : "(unknown)";
                
                if (logger is not null)
                {
                    PersistenceLogDefinitions.LogConcurrencyConflict(logger, entityTypeName, entityId);
                }

                _ = await entry.GetDatabaseValuesAsync(cancellationToken).ConfigureAwait(false);
            }

            _collectedDomainEvents?.Clear();

            throw;
        }
        catch (DbUpdateException ex)
        {
            foreach (var entry in ex.Entries)
            {
                var entityTypeName = entry.Entity.GetType().Name;
                var entityId = entry.Entity is BaseEntity baseEntity ? baseEntity.Id.ToString() : "(unknown)";

                if (logger is not null)
                {
                    PersistenceLogDefinitions.LogDatabaseError(logger, entityTypeName, ex);
                }
            }

            _collectedDomainEvents?.Clear();

            throw;
        }
        catch (Exception)
        {
            _collectedDomainEvents?.Clear();

            throw;
        }
    }

    private static string GetCurrentUserId() => "System"; //ToDo: Replace with ICurrentUserProvider service later

    private static LambdaExpression CreateSoftDeleteFilter(Type type)
    {
        var parameter = Expression.Parameter(type, "e");
        var property = Expression.Call(
            typeof(EF),
            nameof(EF.Property),
            [typeof(bool)],
            parameter,
            Expression.Constant(nameof(ISoftDeletable.IsDeleted)));

        var comparison = Expression.Equal(property, Expression.Constant(false));
        return Expression.Lambda(comparison, parameter);
    }

    private void UpdateAuditAndSoftDelete(string userId)
    {
        var now = DateTimeOffset.UtcNow;

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            var entityType = entry.Entity.GetType().Name;
            var entityId = entry.Entity.Id;

            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = now;
                    entry.Entity.CreatedBy = userId;
                    entry.Entity.UpdatedAt = null;
                    entry.Entity.UpdatedBy = null;
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = now;
                    entry.Entity.UpdatedBy = userId;
                    break;

                case EntityState.Deleted:
                    if (entry.Entity is ISoftDeletable softDeletable)
                    {
                        entry.State = EntityState.Modified;
                        softDeletable.IsDeleted = true;
                        softDeletable.DeletedAt = now;
                        softDeletable.DeletedBy = userId;

                        if (logger is not null)
                            PersistenceLogDefinitions.LogSoftDelete(logger, entityType, entityId, userId);
                    }
                    
                    break;
            }
        }
    }

    private List<IDomainEvent> CollectDomainEvents()
    {
        var domainEvents = new List<IDomainEvent>();

        foreach (var entry in ChangeTracker.Entries<IHasDomainEvents>())
        {
            if (entry.Entity.DomainEvents.Count != 0)
            {
                domainEvents.AddRange(entry.Entity.DomainEvents);
            }
        }

        return domainEvents;
    }

    private void ClearDomainEventsFromAggregates()
    {
        foreach (var entry in ChangeTracker.Entries<IHasDomainEvents>())
        {
            entry.Entity.ClearDomainEvents();
        }
    }
    
}

