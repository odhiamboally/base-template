using BT.Application.Contracts.Interfaces.Common;
using BT.Domain.Features.Shared.Contracts;
using BT.Domain.Features.Shared.OrgSettings.Entities;
using BT.SharedKernel.Dtos.Common;
using BT.SharedKernel.Features.Shared.OrgSettings.Dtos;
using BT.Application.Features.Shared.OrgSettings.Mappings;
using MediatR;
using System.Net;

namespace BT.Application.Features.Shared.OrgSettings.CommandHandlers;

internal sealed class CreateOrgSettingHandler(
    ISharedUnitOfWork unitOfWork,
    IEncryptionService encryptionService)
    : IRequestHandler<CreateOrgSettingCommand, AppResponse<OrgSettingResponse>>
{
    public async Task<AppResponse<OrgSettingResponse>> Handle(CreateOrgSettingCommand request, CancellationToken cancellationToken)
    {
        var existingSetting = await unitOfWork.OrgSettingRepository.FirstOrDefaultAsync(x => x.Key == request.Request.Key, cancellationToken).ConfigureAwait(false);
        
        if (existingSetting != null)
        {
            return AppResponses.Failure<OrgSettingResponse>($"A setting with key '{request.Request.Key}' already exists.");
        }

        var setting = new OrgSetting(
            request.Request.Key,
            OrgSettingMapping.IsSensitiveKey(request.Request.Key)
                ? encryptionService.Encrypt(request.Request.Value)
                : request.Request.Value,
            request.UserId,
            request.Request.Description);

        await unitOfWork.OrgSettingRepository.CreateAsync(setting, cancellationToken).ConfigureAwait(false);
        await unitOfWork.CompleteAsync(cancellationToken).ConfigureAwait(false);

        var response = setting.ToResponse();
        return AppResponses.Success("Tenant setting created successfully.", response);
    }
}
