using BT.SharedKernel.Dtos.Common;
using BT.SharedKernel.Features.IAM.Users.Dtos;
using BT.UI.Blazor.Features.Shared.Messaging;
using BT.UI.Blazor.Logging;
using BT.UI.Rcl.Features.IAM.Users.Contracts.Interfaces;

namespace BT.UI.Blazor.Features.IAM.Users.Contracts.Implementations;

internal sealed class AuthSession(IAuthService authService, ITokenStorage storage, ILogger<AuthSession> logger) : IAuthSession, IDisposable
{
    private readonly SemaphoreSlim _initializationLock = new(1, 1);

    public bool IsInitialized { get; private set; }

    public bool IsAuthenticated => CurrentUser?.IsAuthenticated != false && CurrentUser is not null;

    public CurrentUserResponse? CurrentUser { get; private set; }

    public string? PendingTwoFactorUserId { get; private set; }

    public string? LastError { get; private set; }

    public bool HasFullAccess => IsInRole("System Administrator");

    public bool MfaEnrollmentRequired => CurrentUser?.MfaEnrollmentRequired == true;

    public async Task InitializeAsync()
    {
        if (IsInitialized)
        {
            return;
        }

        await _initializationLock.WaitAsync().ConfigureAwait(false);
        try
        {
            if (IsInitialized)
            {
                return;
            }

            try
            {
                var (accessToken, _, _) = await storage.GetAsync().ConfigureAwait(false);
                if (!string.IsNullOrWhiteSpace(accessToken))
                {
                    var currentUser = await authService.GetCurrentUserAsync().ConfigureAwait(false);
                    if (currentUser.IsSuccess)
                    {
                        CurrentUser = currentUser.Data;
                        LastError = null;
                    }
                    else
                    {
                        LastError = GetMeaningfulMessage(currentUser.Message, "Your saved session is no longer valid. Please sign in again.");
                        await TryClearStorageAsync().ConfigureAwait(false);
                    }
                }
            }
            catch (Exception ex)
            {
                AuthSessionLogDefinitions.LogSessionInitializationFailed(logger, ex);
                CurrentUser = null;
                PendingTwoFactorUserId = null;
                LastError = "We could not restore your previous session. Please sign in again.";
                await TryClearStorageAsync().ConfigureAwait(false);
            }

            IsInitialized = true;
        }
        finally
        {
            _initializationLock.Release();
        }
    }

    public async Task RefreshAsync()
    {
        await RefreshCurrentUserAsync().ConfigureAwait(false);
        IsInitialized = true;
    }

    public async Task<AppResponse<LoginResponse>> SignInAsync(LoginRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var response = await authService.LoginAsync(request).ConfigureAwait(false);
        if (!response.IsSuccess || response.Data is null)
        {
            LastError = response.Message;
            return response;
        }

        if (response.Data.Requires2FA)
        {
            PendingTwoFactorUserId = response.Data.UserId;
            CurrentUser = null;
            return response;
        }

        PendingTwoFactorUserId = null;
        await RefreshCurrentUserAsync().ConfigureAwait(false);

        if (CurrentUser is null && response.Data.UserInfo is not null)
        {
            var roles = response.Data.UserInfo.Roles ?? [];

            CurrentUser = new CurrentUserResponse(
                response.Data.UserInfo.Id,
                response.Data.UserInfo.EmployeeId ?? Guid.Empty,
                response.Data.UserInfo.CustomerId ?? Guid.Empty,
                response.Data.UserInfo.IdNumber ?? string.Empty,
                response.Data.UserInfo.Username,
                response.Data.UserInfo.Email,
                response.Data.UserInfo.FirstName,
                response.Data.UserInfo.LastName,
                response.Data.UserInfo.PhoneNumber ?? string.Empty,
                true,
                response.Data.UserInfo.TwoFactorEnabled,
                response.Data.UserInfo.Gender,
                true,
                response.Data.UserInfo.LastLoginAt,
                [.. roles],
                SessionId: response.Data.SessionId,
                MfaEnrollmentRequired: response.Data.MfaEnrollmentRequired,
                ProfilePictureUrl: response.Data.UserInfo.ProfilePictureUrl);

            LastError = null;
        }

        return response;
    }

    public async Task<AppResponse<VerifyOtpResponse>> CompleteTwoFactorAsync(VerifyOtpRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var response = await authService.VerifyTotpAsync(request).ConfigureAwait(false);
        if (!response.IsSuccess || response.Data is null)
        {
            LastError = response.Message;
            return response;
        }

        await storage.SaveAsync(response.Data.Token, response.Data.RefreshToken, response.Data.SessionId).ConfigureAwait(false);
        PendingTwoFactorUserId = null;
        await RefreshCurrentUserAsync().ConfigureAwait(false);

        return response;
    }

    public async Task<AppResponse<bool>> SignOutAsync()
    {
        var response = await authService.LogoutAsync().ConfigureAwait(false);
        CurrentUser = null;
        PendingTwoFactorUserId = null;
        LastError = null;
        IsInitialized = true;
        return response;
    }

    public bool HasPermission(string permission)
    {
        if (string.IsNullOrWhiteSpace(permission))
        {
            return true;
        }

        return HasFullAccess
            || CurrentUser?.Permissions?.Contains(permission, StringComparer.OrdinalIgnoreCase) == true;
    }

    public bool HasAnyPermission(params string[] permissions)
    {
        return permissions.Length == 0 || permissions.Any(HasPermission);
    }

    public bool IsInRole(string role)
    {
        return !string.IsNullOrWhiteSpace(role)
            && CurrentUser?.Roles?.Contains(role, StringComparer.OrdinalIgnoreCase) == true;
    }

    private async Task RefreshCurrentUserAsync()
    {
        var currentUser = await authService.GetCurrentUserAsync().ConfigureAwait(false);
        CurrentUser = currentUser.IsSuccess ? currentUser.Data : null;
        LastError = currentUser.IsSuccess ? null : GetMeaningfulMessage(currentUser.Message, "The API did not return a current user profile.");
    }

    private async Task TryClearStorageAsync()
    {
        try
        {
            await storage.ClearAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            AuthSessionLogDefinitions.LogSessionStorageClearFailed(logger, ex);
        }
    }

    private static string GetMeaningfulMessage(string? message, string fallback)
        => UserMessageSanitizer.Normalize(message, fallback);

    public void Dispose()
    {
        _initializationLock.Dispose();
    }
}
