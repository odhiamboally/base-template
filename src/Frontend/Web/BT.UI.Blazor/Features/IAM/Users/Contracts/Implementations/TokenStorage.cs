using BT.UI.Blazor.Features.IAM.Users.Contracts.Interfaces;
using BT.UI.Blazor.Features.IAM.Users.Implementations;
using BT.UI.Rcl.Features.IAM.Users.Contracts.Interfaces;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace BT.UI.Blazor.Features.IAM.Users.Contracts.Implementations;

internal sealed class TokenStorage(ProtectedLocalStorage storage, IServerTokenStore? serverStore) : ITokenStorage
{
    private const string AccessKey = "auth.access_token";
    private const string RefreshKey = "auth.refresh_token";
    private const string SessionKey = "auth.session_id";

    public async Task<bool> ClearAsync()
    {
        await storage.DeleteAsync(AccessKey).ConfigureAwait(false);
        await storage.DeleteAsync(RefreshKey).ConfigureAwait(false);
        await storage.DeleteAsync(SessionKey).ConfigureAwait(false);
        if (serverStore is not null)
        {
            await serverStore.ClearAsync().ConfigureAwait(false);
        }
        return true;

    }
    public async Task<(string? AccessToken, string? RefreshToken, string? SessionId)> GetAsync()
    {
        try
        {
            var access = await storage.GetAsync<string>(AccessKey).ConfigureAwait(false);
            var refresh = await storage.GetAsync<string>(RefreshKey).ConfigureAwait(false);
            var session = await storage.GetAsync<string>(SessionKey).ConfigureAwait(false);
            var result = (
                access.Success ? access.Value : null, 
                refresh.Success ? refresh.Value : null,
                session.Success ? session.Value : null
                
                );                  
                    
            // Mirror to server store if available so background tasks can read tokens when JS is unavailable.
            if (serverStore is not null)
            {
                await serverStore.SaveAsync(result.Item1, result.Item2, result.Item3).ConfigureAwait(false);
            }

            return result;
        }
        catch (Exception ex) when (ex is OperationCanceledException or Microsoft.JSInterop.JSDisconnectedException or InvalidOperationException)
        {
            // JS interop / ProtectedLocalStorage call was canceled or unavailable.
            // Fall back to server-side token store if available.
            if (serverStore is not null)
            {
                return await serverStore.GetAsync().ConfigureAwait(false);
            }

            return (null, null, null);
        }

    }

    public async Task<bool> SaveAsync(string? accessToken, string? refreshToken, string? sessionId)
    {
        try
        {
            await storage.SetAsync(AccessKey, accessToken ?? "").ConfigureAwait(false);
            await storage.SetAsync(RefreshKey, refreshToken ?? "").ConfigureAwait(false);
            await storage.SetAsync(SessionKey, sessionId ?? "").ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is OperationCanceledException or Microsoft.JSInterop.JSDisconnectedException or InvalidOperationException)
        {
            // Ignore; will save to server store below if available.
        }

        if (serverStore is not null)
        {
            await serverStore.SaveAsync(accessToken, refreshToken, sessionId).ConfigureAwait(false);
        }

        return true;
    }
}
