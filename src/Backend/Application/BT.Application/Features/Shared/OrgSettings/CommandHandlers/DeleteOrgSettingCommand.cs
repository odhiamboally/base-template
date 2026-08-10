using BT.Application.Contracts.Interfaces.Common;
using BT.SharedKernel.Dtos.Common;
using MediatR;

namespace BT.Application.Features.Shared.OrgSettings.CommandHandlers;

public record DeleteOrgSettingCommand(Guid Id, string UserId) 
    : IRequest<AppResponse<bool>>, ICacheInvalidatorRequest
{
    public string CacheGroup => "tenant-settings";
}
