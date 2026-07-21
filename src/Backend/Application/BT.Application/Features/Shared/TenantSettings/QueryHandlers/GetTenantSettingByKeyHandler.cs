using BT.Application.Contracts.Interfaces.Common;
using BT.Domain.Features.Shared.Contracts;
using BT.SharedKernel.Dtos.Common;
using BT.SharedKernel.Features.Shared.TenantSettings.Dtos;
using BT.Application.Features.Shared.TenantSettings.Mappings;
using MediatR;
using System.Net;

namespace BT.Application.Features.Shared.TenantSettings.QueryHandlers;

internal sealed class GetTenantSettingByKeyHandler(
    ISharedUnitOfWork unitOfWork,
    IEncryptionService encryptionService)
    : IRequestHandler<GetTenantSettingByKeyQuery, AppResponse<TenantSettingResponse>>
{
    public async Task<AppResponse<TenantSettingResponse>> Handle(GetTenantSettingByKeyQuery request, CancellationToken cancellationToken)
    {
        var setting = await unitOfWork.TenantSettingRepository.FirstOrDefaultAsync(x => x.Key == request.Key, cancellationToken).ConfigureAwait(false);
        
        if (setting == null)
        {
            return AppResponses.Failure<TenantSettingResponse>("Tenant setting not found.");
        }

        var response = setting.ToResponse();
        
        if (response.Value != "***")
        {
            try
            {
                response = response with { Value = encryptionService.Decrypt(response.Value) };
            }
            catch
            {
                // Fallback to original value on decryption failure
            }
        }

        return AppResponses.Success("Tenant setting retrieved successfully.", response);
    }
}
