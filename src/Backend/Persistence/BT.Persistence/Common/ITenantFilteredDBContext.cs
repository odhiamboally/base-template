namespace BT.Persistence.Common;

internal interface ITenantFilteredDBContext
{
    Guid CurrentTenantId { get; }
}
