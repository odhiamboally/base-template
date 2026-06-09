using BT.Domain.Features.Banking.Contracts;
using BT.Domain.Features.HR.Contracts;
using BT.Domain.Features.IAM.Contracts;
using BT.Domain.Shared.Contracts;
using BT.Domain.Shared.Contracts.Common;
using BT.Domain.Shared.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Linq.Expressions;

namespace BT.Persistence.Common;

internal static class DbContextHelper
{
    public static void UpdateAuditAndSoftDelete(ChangeTracker changeTracker, string userId)
    {
        var now = DateTimeOffset.UtcNow;

        foreach (var entry in changeTracker.Entries<BaseEntity>())
        {
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

                    if (entry.Entity is ISoftDeletable { IsDeleted: true, DeletedAt: null } softDeleted)
                    {
                        softDeleted.DeletedAt = now;
                        softDeleted.DeletedBy = userId;
                    }

                    break;
                case EntityState.Deleted:
                    if (entry.Entity is ISoftDeletable sd)
                    {
                        entry.State = EntityState.Modified;
                        sd.IsDeleted = true;
                        sd.DeletedAt = now;
                        sd.DeletedBy = userId;
                    }
                    break;
            }
        }
    }

    public static List<IDomainEvent> CollectDomainEvents(ChangeTracker changeTracker)
    {
        var events = new List<IDomainEvent>();
        foreach (var entry in changeTracker.Entries<IHasDomainEvents>())
            if (entry.Entity.DomainEvents.Any())
                events.AddRange(entry.Entity.DomainEvents);
        return events;
    }

    public static void ClearDomainEventsFromAggregates(ChangeTracker changeTracker)
    {
        foreach (var entry in changeTracker.Entries<IHasDomainEvents>())
            entry.Entity.ClearDomainEvents();
    }

    public static LambdaExpression CreateSoftDeleteFilter(Type type)
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
}
