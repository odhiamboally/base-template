using BT.Application.Contracts.Interfaces.Common;
using BT.Domain.Features.Shared.Contracts;
using BT.SharedKernel.Dtos.Common;
using BT.SharedKernel.Features.Shared.OrgSettings.Dtos;
using BT.Application.Features.Shared.OrgSettings.Mappings;
using MediatR;

namespace BT.Application.Features.Shared.OrgSettings.QueryHandlers;

internal sealed class GetOrgSettingsHandler(
    ISharedUnitOfWork unitOfWork,
    IEncryptionService encryptionService)
    : IRequestHandler<GetOrgSettingsQuery, AppResponse<IEnumerable<OrgSettingResponse>>>
{
    public async Task<AppResponse<IEnumerable<OrgSettingResponse>>> Handle(GetOrgSettingsQuery request, CancellationToken cancellationToken)
    {
        var settings = await unitOfWork.OrgSettingRepository.ListAsync(ct: cancellationToken).ConfigureAwait(false);
        
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
