using BT.Application.Contracts.Interfaces.Common;
using BT.Application.Utilities;
using BT.SharedKernel.Dtos.Common;
using BT.SharedKernel.Features.Shared.TenantSettings.Dtos;
using MediatR;

namespace BT.Application.Features.Shared.TenantSettings.QueryHandlers;

public record GetTenantSettingByKeyQuery(string Key) 
    : IRequest<AppResponse<TenantSettingResponse>>, ICachableRequest
{
    public string CacheGroup => "tenant-settings";
    public string Discriminator => CacheKeys.Entity("tenant-settings", Key);
    public string? CacheUserId => null;
    public bool IsVersioned => false;
}
