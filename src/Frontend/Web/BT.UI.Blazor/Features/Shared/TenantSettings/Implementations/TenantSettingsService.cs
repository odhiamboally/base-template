using BT.SharedKernel.Dtos.Common;
using BT.SharedKernel.Features.Shared.TenantSettings.Dtos;
using BT.UI.Blazor.Configuration;
using BT.UI.Blazor.Features.Shared.BackendApi;
using BT.UI.Blazor.Features.Shared.BackendApi.Contracts.Interfaces;
using BT.UI.Blazor.Features.Shared.TenantSettings.Contracts;
using Microsoft.Extensions.Options;

namespace BT.UI.Blazor.Features.Shared.TenantSettings.Implementations;

internal sealed class TenantSettingsService(IBackendApiClient apiClient, IOptions<BackendApiSettings> apiSettings) : ITenantSettingsService
{
    private readonly BackendApiSettings _apiSettings = apiSettings.Value;

    public Task<AppResponse<IEnumerable<TenantSettingResponse>>> GetTenantSettingsAsync(CancellationToken ct = default)
    {
        return apiClient.SendAsync<IEnumerable<TenantSettingResponse>>(
            HttpMethod.Get,
            EndpointFormatter.Format(_apiSettings.Endpoints.Shared.TenantSettings.Root, _apiSettings.Version),
            unavailableMessage: "The tenant settings service is unavailable. Please try again.",
            timeoutMessage: "The request timed out. Please try again.",
            cancellationToken: ct);
    }

    public Task<AppResponse<TenantSettingResponse>> GetTenantSettingByKeyAsync(string key, CancellationToken ct = default)
    {
        return apiClient.SendAsync<TenantSettingResponse>(
            HttpMethod.Get,
            EndpointFormatter.Format(_apiSettings.Endpoints.Shared.TenantSettings.Detail, _apiSettings.Version, new Dictionary<string, string> { { "key", key } }),
            unavailableMessage: "The tenant settings service is unavailable. Please try again.",
            timeoutMessage: "The request timed out. Please try again.",
            cancellationToken: ct);
    }

    public Task<AppResponse<TenantSettingResponse>> CreateTenantSettingAsync(CreateTenantSettingRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        return apiClient.SendAsync<TenantSettingResponse>(
            HttpMethod.Post,
            EndpointFormatter.Format(_apiSettings.Endpoints.Shared.TenantSettings.Root, _apiSettings.Version),
            request,
            unavailableMessage: "The tenant settings service is unavailable. Please try again.",
            timeoutMessage: "The request timed out. Please try again.",
            cancellationToken: ct);
    }

    public Task<AppResponse<TenantSettingResponse>> UpdateTenantSettingAsync(UpdateTenantSettingRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        return apiClient.SendAsync<TenantSettingResponse>(
            HttpMethod.Put,
            EndpointFormatter.Format(_apiSettings.Endpoints.Shared.TenantSettings.Root, _apiSettings.Version),
            request,
            unavailableMessage: "The tenant settings service is unavailable. Please try again.",
            timeoutMessage: "The request timed out. Please try again.",
            cancellationToken: ct);
    }

    public Task<AppResponse<bool>> DeleteTenantSettingAsync(Guid id, CancellationToken ct = default)
    {
        return apiClient.SendAsync<bool>(
            HttpMethod.Delete,
            EndpointFormatter.Format(_apiSettings.Endpoints.Shared.TenantSettings.Root, _apiSettings.Version) + $"/{id}",
            unavailableMessage: "The tenant settings service is unavailable. Please try again.",
            timeoutMessage: "The request timed out. Please try again.",
            cancellationToken: ct);
    }
}
