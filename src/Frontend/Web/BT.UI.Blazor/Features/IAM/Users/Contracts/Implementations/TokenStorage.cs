using BT.UI.Rcl.Features.IAM.Users.Contracts.Interfaces;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace BT.UI.Blazor.Features.IAM.Users.Contracts.Implementations;

internal sealed class TokenStorage(ProtectedLocalStorage storage) : ITokenStorage
{
    private const string AccessKey = "auth.access_token";
    private const string RefreshKey = "auth.refresh_token";
    private const string SessionKey = "auth.session_id";

    public async Task<bool> ClearAsync()
    {
        await storage.DeleteAsync(AccessKey).ConfigureAwait(false);
        await storage.DeleteAsync(RefreshKey).ConfigureAwait(false);
        await storage.DeleteAsync(SessionKey).ConfigureAwait(false);
        return true;

    }
    public async Task<(string? AccessToken, string? RefreshToken, string? SessionId)> GetAsync()
    {
        var access = await storage.GetAsync<string>(AccessKey).ConfigureAwait(false);
        var refresh = await storage.GetAsync<string>(RefreshKey).ConfigureAwait(false);
        var session = await storage.GetAsync<string>(SessionKey).ConfigureAwait(false);
        return (access.Success ? access.Value : null,
                refresh.Success ? refresh.Value : null,
                session.Success ? session.Value : null);

    }

    public async Task<bool> SaveAsync(string? accessToken, string? refreshToken, string? sessionId)
    {
        await storage.SetAsync(AccessKey, accessToken ?? "").ConfigureAwait(false);
        await storage.SetAsync(RefreshKey, refreshToken ?? "").ConfigureAwait(false);
        await storage.SetAsync(SessionKey, sessionId ?? "").ConfigureAwait(false);
        return true;
    }
}
