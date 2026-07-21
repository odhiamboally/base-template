using BT.Application.Contracts.Interfaces.Common;
using BT.Domain.Features.Shared.Contracts;
using BT.SharedKernel.Dtos.Common;
using BT.SharedKernel.Features.Shared.TenantSettings.Dtos;
using BT.Application.Features.Shared.TenantSettings.Mappings;
using MediatR;

namespace BT.Application.Features.Shared.TenantSettings.QueryHandlers;

internal sealed class GetTenantSettingsHandler(
    ISharedUnitOfWork unitOfWork,
    IEncryptionService encryptionService)
    : IRequestHandler<GetTenantSettingsQuery, AppResponse<IEnumerable<TenantSettingResponse>>>
{
    public async Task<AppResponse<IEnumerable<TenantSettingResponse>>> Handle(GetTenantSettingsQuery request, CancellationToken cancellationToken)
    {
        var settings = await unitOfWork.TenantSettingRepository.ListAsync(ct: cancellationToken).ConfigureAwait(false);
        
        var responses = settings.Select(s =>
        {
            var mapped = s.ToResponse();
            // If mapping didn't obscure it (i.e. not "***"), we must decrypt it so it's readable
            if (mapped.Value != "***")
            {
                try
                {
                    mapped = mapped with { Value = encryptionService.Decrypt(mapped.Value) };
                }
                catch
                {
                    // Ignore decryption error, fallback to original encrypted or raw value
                }
            }
            return mapped;
        });

        return AppResponses.Success("Tenant settings retrieved successfully.", responses);
    }
}
