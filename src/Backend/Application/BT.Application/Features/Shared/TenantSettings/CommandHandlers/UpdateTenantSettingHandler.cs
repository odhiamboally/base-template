using BT.Application.Contracts.Interfaces.Common;
using BT.Domain.Features.Shared.Contracts;
using BT.SharedKernel.Dtos.Common;
using BT.SharedKernel.Features.Shared.TenantSettings.Dtos;
using BT.Application.Features.Shared.TenantSettings.Mappings;
using MediatR;
using System.Net;

namespace BT.Application.Features.Shared.TenantSettings.CommandHandlers;

internal sealed class UpdateTenantSettingHandler(
    ISharedUnitOfWork unitOfWork,
    IEncryptionService encryptionService)
    : IRequestHandler<UpdateTenantSettingCommand, AppResponse<TenantSettingResponse>>
{
    public async Task<AppResponse<TenantSettingResponse>> Handle(UpdateTenantSettingCommand request, CancellationToken cancellationToken)
    {
        var setting = await unitOfWork.TenantSettingRepository.FindByIdAsync(request.Request.Id, cancellationToken).ConfigureAwait(false);
        
        if (setting == null)
        {
            return AppResponses.Failure<TenantSettingResponse>("Tenant setting not found.");
        }

        // Check if the key was changed and if it conflicts with an existing one
        if (setting.Key != request.Request.Key)
        {
            var existingWithKey = await unitOfWork.TenantSettingRepository.FirstOrDefaultAsync(x => x.Key == request.Request.Key && x.Id != request.Request.Id, cancellationToken).ConfigureAwait(false);
            if (existingWithKey != null)
            {
                return AppResponses.Failure<TenantSettingResponse>($"A setting with key '{request.Request.Key}' already exists.");
            }
            setting.Key = request.Request.Key;
        }

        // Only update the value if it's not the obscured mask "***"
        if (request.Request.Value != "***")
        {
            setting.Value = TenantSettingMapping.IsSensitiveKey(request.Request.Key)
                ? encryptionService.Encrypt(request.Request.Value)
                : request.Request.Value;
        }

        setting.Description = request.Request.Description;


        await unitOfWork.TenantSettingRepository.UpdateAsync(setting, cancellationToken).ConfigureAwait(false);
        await unitOfWork.CompleteAsync(cancellationToken).ConfigureAwait(false);

        var response = setting.ToResponse();
        return AppResponses.Success("Tenant setting updated successfully.", response);
    }
}
