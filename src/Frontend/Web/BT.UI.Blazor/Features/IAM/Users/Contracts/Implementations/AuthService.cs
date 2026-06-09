using System.Net;
using BT.SharedKernel.Dtos.Common;
using BT.SharedKernel.Features.IAM.Users.Dtos;
using BT.UI.Blazor.Configuration;
using BT.UI.Blazor.Features.Shared.BackendApi;
using BT.UI.Blazor.Features.Shared.BackendApi.Contracts.Interfaces;
using BT.UI.Rcl.Features.IAM.Users.Contracts.Interfaces;
using Microsoft.Extensions.Options;

namespace BT.UI.Blazor.Features.IAM.Users.Contracts.Implementations;

internal sealed class AuthService(IBackendApiClient apiClient, ITokenStorage storage, IOptions<BackendApiSettings> apiSettings) : IAuthService
{
    private readonly BackendApiSettings _apiSettings = apiSettings.Value;

    public async Task<AppResponse<LoginResponse>> LoginAsync(LoginRequest loginRequest)
    {
        ArgumentNullException.ThrowIfNull(loginRequest);

        var response = await apiClient.SendAsync<LoginResponse>(
            HttpMethod.Post,
            Format(_apiSettings.Endpoints.Iam.Auth.Login),
            loginRequest,
            requiresAuthentication: false,
            unavailableMessage: "The identity service is unavailable. Please try again.",
            timeoutMessage: "The identity service timed out. Please try again.").ConfigureAwait(false);

        if (response.Successful && response.Data is { IsAuthenticated: true, Requires2FA: false } login)
        {
            await storage.SaveAsync(login.Token, login.RefreshToken, login.SessionId).ConfigureAwait(false);
        }

        return response;
    }

    public async Task<AppResponse<RefreshTokenResponse>> RefreshTokenAsync(RefreshTokenRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var response = await apiClient.SendAsync<RefreshTokenResponse>(
            HttpMethod.Post,
            Format(_apiSettings.Endpoints.Iam.Auth.RefreshToken),
            request,
            requiresAuthentication: false,
            unavailableMessage: "The identity service is unavailable. Please try again.",
            timeoutMessage: "The identity service timed out. Please try again.").ConfigureAwait(false);

        if (response.Successful && response.Data is not null)
        {
            await storage.SaveAsync(response.Data.Token, response.Data.RefreshToken, response.Data.SessionId).ConfigureAwait(false);
        }

        return response;
    }

    public Task<AppResponse<CurrentUserResponse>> GetCurrentUserAsync()
        => SendWithRefreshAsync<CurrentUserResponse>(HttpMethod.Get, Format(_apiSettings.Endpoints.Iam.Auth.CurrentUser));

    public Task<AppResponse<TwoFactorSetupInfo>> InitiateTotpSetupAsync()
        => SendWithRefreshAsync<TwoFactorSetupInfo>(HttpMethod.Post, Format(_apiSettings.Endpoints.Iam.Users.InitiateTotpSetup));

    public Task<AppResponse<VerifyOtpResponse>> VerifyTotpAsync(VerifyOtpRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        return apiClient.SendAsync<VerifyOtpResponse>(
            HttpMethod.Post,
            Format(_apiSettings.Endpoints.Iam.Users.VerifyTotp),
            request,
            requiresAuthentication: false,
            unavailableMessage: "The identity service is unavailable. Please try again.",
            timeoutMessage: "The identity service timed out. Please try again.");
    }

    public Task<AppResponse<OtpStatusResponse>> GetTotpStatusAsync(string userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        return SendWithRefreshAsync<OtpStatusResponse>(
            HttpMethod.Get,
            Format(_apiSettings.Endpoints.Iam.Users.TotpStatus, ("userId", userId)));
    }

    public async Task<AppResponse<bool>> LogoutAsync()
    {
        try
        {
            _ = await SendWithRefreshAsync<bool>(HttpMethod.Post, Format(_apiSettings.Endpoints.Iam.Auth.Logout))
                .ConfigureAwait(false);
        }
        finally
        {
            await storage.ClearAsync().ConfigureAwait(false);
        }

        return AppResponse.Success("Signed out", true);
    }

    private async Task<AppResponse<T>> SendWithRefreshAsync<T>(HttpMethod method, string endpoint)
    {
        var response = await SendAuthenticatedAsync<T>(method, endpoint).ConfigureAwait(false);
        if (response.ErrorCode != HttpStatusCode.Unauthorized.ToString())
        {
            return response;
        }

        var (accessToken, refreshToken, _) = await storage.GetAsync().ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(accessToken) || string.IsNullOrWhiteSpace(refreshToken))
        {
            return response;
        }

        var refreshResponse = await RefreshTokenAsync(new RefreshTokenRequest(accessToken, refreshToken)).ConfigureAwait(false);
        if (!refreshResponse.Successful)
        {
            await storage.ClearAsync().ConfigureAwait(false);
            return AppResponse.Failure<T>(refreshResponse.Message ?? "Your session has expired. Please sign in again.");
        }

        return await SendAuthenticatedAsync<T>(method, endpoint).ConfigureAwait(false);
    }

    private Task<AppResponse<T>> SendAuthenticatedAsync<T>(HttpMethod method, string endpoint)
        => apiClient.SendAsync<T>(
            method,
            endpoint,
            requiresAuthentication: true,
            unavailableMessage: "The identity service is unavailable. Please try again.",
            timeoutMessage: "The identity service timed out. Please try again.");

    private string Format(string endpoint, params (string Key, string Value)[] parameters)
        => EndpointFormatter.Format(endpoint, _apiSettings.Version, parameters.ToDictionary(static item => item.Key, static item => item.Value));
}
