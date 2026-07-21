using BT.Application.Contracts.Interfaces.Common;
using BT.Domain.Features.Shared.Contracts;
using BT.Domain.Features.Shared.TenantSettings.Entities;
using BT.SharedKernel.Dtos.Common;
using BT.SharedKernel.Features.Shared.TenantSettings.Dtos;
using BT.Application.Features.Shared.TenantSettings.Mappings;
using MediatR;
using System.Net;

namespace BT.Application.Features.Shared.TenantSettings.CommandHandlers;

internal sealed class CreateTenantSettingHandler(
    ISharedUnitOfWork unitOfWork,
    IEncryptionService encryptionService)
    : IRequestHandler<CreateTenantSettingCommand, AppResponse<TenantSettingResponse>>
{
    public async Task<AppResponse<TenantSettingResponse>> Handle(CreateTenantSettingCommand request, CancellationToken cancellationToken)
    {
        var existingSetting = await unitOfWork.TenantSettingRepository.FirstOrDefaultAsync(x => x.Key == request.Request.Key, cancellationToken).ConfigureAwait(false);
        
        if (existingSetting != null)
        {
            return AppResponses.Failure<TenantSettingResponse>($"A setting with key '{request.Request.Key}' already exists.");
        }

        var setting = new TenantSetting(
            request.Request.Key,
            TenantSettingMapping.IsSensitiveKey(request.Request.Key)
                ? encryptionService.Encrypt(request.Request.Value)
                : request.Request.Value,
            request.UserId,
            request.Request.Description);

        await unitOfWork.TenantSettingRepository.CreateAsync(setting, cancellationToken).ConfigureAwait(false);
        await unitOfWork.CompleteAsync(cancellationToken).ConfigureAwait(false);

        var response = setting.ToResponse();
        return AppResponses.Success("Tenant setting created successfully.", response);
    }
}
