using BT.SharedKernel.Features.Shared.TenantSettings.Dtos;
using BT.SharedKernel.Dtos.Common;

namespace BT.UI.Blazor.Features.Shared.TenantSettings.Contracts;

internal interface ITenantSettingsService
{
    Task<AppResponse<IEnumerable<TenantSettingResponse>>> GetTenantSettingsAsync(CancellationToken ct = default);
    Task<AppResponse<TenantSettingResponse>> GetTenantSettingByKeyAsync(string key, CancellationToken ct = default);
    Task<AppResponse<TenantSettingResponse>> CreateTenantSettingAsync(CreateTenantSettingRequest request, CancellationToken ct = default);
    Task<AppResponse<TenantSettingResponse>> UpdateTenantSettingAsync(UpdateTenantSettingRequest request, CancellationToken ct = default);
    Task<AppResponse<bool>> DeleteTenantSettingAsync(Guid id, CancellationToken ct = default);
}
