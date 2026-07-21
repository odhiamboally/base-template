using BT.Application.Contracts.Interfaces.Common;
using BT.SharedKernel.Dtos.Common;
using MediatR;

namespace BT.Application.Features.Shared.TenantSettings.CommandHandlers;

public record DeleteTenantSettingCommand(Guid Id, string UserId) 
    : IRequest<AppResponse<bool>>, ICacheInvalidatorRequest
{
    public string CacheGroup => "tenant-settings";
}
