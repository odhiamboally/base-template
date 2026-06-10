namespace BT.Persistence.Common;

internal interface ITenantFilteredDbContext
{
    Guid CurrentTenantId { get; }
}
