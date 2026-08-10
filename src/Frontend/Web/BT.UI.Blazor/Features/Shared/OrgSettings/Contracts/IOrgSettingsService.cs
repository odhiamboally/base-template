using BT.SharedKernel.Features.Shared.OrgSettings.Dtos;
using BT.SharedKernel.Dtos.Common;

namespace BT.UI.Blazor.Features.Shared.OrgSettings.Contracts;

internal interface IOrgSettingsService
{
    Task<AppResponse<IEnumerable<OrgSettingResponse>>> GetOrgSettingsAsync(CancellationToken ct = default);
    Task<AppResponse<OrgSettingResponse>> GetOrgSettingByKeyAsync(string key, CancellationToken ct = default);
    Task<AppResponse<OrgSettingResponse>> CreateOrgSettingAsync(CreateOrgSettingRequest request, CancellationToken ct = default);
    Task<AppResponse<OrgSettingResponse>> UpdateOrgSettingAsync(UpdateOrgSettingRequest request, CancellationToken ct = default);
    Task<AppResponse<bool>> DeleteOrgSettingAsync(Guid id, CancellationToken ct = default);
}
