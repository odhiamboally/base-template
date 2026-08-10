using BT.Application.Contracts.Interfaces.Common;
using BT.SharedKernel.Dtos.Common;
using BT.SharedKernel.Features.Shared.OrgSettings.Dtos;
using MediatR;

namespace BT.Application.Features.Shared.OrgSettings.CommandHandlers;

public record CreateOrgSettingCommand(CreateOrgSettingRequest Request, string UserId) 
    : IRequest<AppResponse<OrgSettingResponse>>, ICacheInvalidatorRequest
{
    public string CacheGroup => "tenant-settings";
}
