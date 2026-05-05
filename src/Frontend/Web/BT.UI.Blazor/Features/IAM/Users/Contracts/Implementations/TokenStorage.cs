using BT.Domain.Features.IAM.Users.Entities;
using BT.UI.Rcl.Features.IAM.Users.Contracts.Interfaces;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace BT.UI.Blazor.Features.IAM.Users.Contracts.Implementations;

internal sealed class TokenStorage(ProtectedLocalStorage storage) : ITokenStorage
{
    const string AccessKey = "auth.access_token";
    const string RefreshKey = "auth.refresh_token";

    public async Task<bool> ClearAsync()
    {
        await storage.DeleteAsync(AccessKey).ConfigureAwait(false);
        await storage.DeleteAsync(RefreshKey).ConfigureAwait(false);
        return true;

    }
    public async Task<(string?, string?)> GetAsync()
    {
        var access = await storage.GetAsync<string>(AccessKey).ConfigureAwait(false);
        var refresh = await storage.GetAsync<string>(RefreshKey).ConfigureAwait(false);
        return (access.Success ? access.Value : null,
                refresh.Success ? refresh.Value : null);

    }

    public async Task<bool> SaveAsync(string? accessToken, string? refreshToken)
    {
        await storage.SetAsync(AccessKey, accessToken ?? "").ConfigureAwait(false);
        await storage.SetAsync(RefreshKey, refreshToken ?? "").ConfigureAwait(false);
        return true;
    }
}
