namespace BT.Domain.Shared.Contracts.Common;

public interface ICurrentTenantProvider
{
    Guid TenantId { get; }
}
