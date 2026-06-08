using BT.SharedKernel.Dtos.Common;

namespace BT.UI.Blazor.Features.Shared.BackendApi.Contracts.Interfaces;

internal interface IBackendApiClient
{
    Task<AppResponse<T>> SendAsync<T>(
        HttpMethod method,
        string endpoint,
        object? request = null,
        bool requiresAuthentication = true,
        string? unavailableMessage = null,
        string? timeoutMessage = null,
        CancellationToken cancellationToken = default);
}
