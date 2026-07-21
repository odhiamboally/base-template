using BT.Application.Contracts.Interfaces.Common;
using BT.SharedKernel.Dtos.Common;
using BT.SharedKernel.Features.Shared.TenantSettings.Dtos;
using MediatR;

namespace BT.Application.Features.Shared.TenantSettings.CommandHandlers;

public record CreateTenantSettingCommand(CreateTenantSettingRequest Request, string UserId) 
    : IRequest<AppResponse<TenantSettingResponse>>, ICacheInvalidatorRequest
{
    public string CacheGroup => "tenant-settings";
}
