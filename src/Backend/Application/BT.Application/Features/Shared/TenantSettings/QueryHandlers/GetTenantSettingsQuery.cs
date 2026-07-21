using BT.Application.Contracts.Interfaces.Common;
using BT.SharedKernel.Dtos.Common;
using BT.SharedKernel.Features.Shared.TenantSettings.Dtos;
using MediatR;
using System.Collections.Generic;

namespace BT.Application.Features.Shared.TenantSettings.QueryHandlers;

public record GetTenantSettingsQuery 
    : IRequest<AppResponse<IEnumerable<TenantSettingResponse>>>, ICachableRequest
{
    public string CacheGroup => "tenant-settings";
    public string Discriminator => "all";
    public string? CacheUserId => null;
    public bool IsVersioned => false;
}
