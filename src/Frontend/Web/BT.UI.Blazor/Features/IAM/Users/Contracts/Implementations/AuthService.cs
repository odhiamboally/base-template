using System.Net;
using BT.SharedKernel.Dtos.Common;
using BT.SharedKernel.Features.Shared.Common.Enums;
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

        if (response.IsSuccess && response.Data is { IsAuthenticated: true, Requires2FA: false } login)
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

        if (response.IsSuccess && response.Data is not null)
        {
            await storage.SaveAsync(response.Data.Token, response.Data.RefreshToken, response.Data.SessionId).ConfigureAwait(false);
        }

        return response;
    }

    public Task<AppResponse<CurrentUserResponse>> GetCurrentUserAsync()
        => SendWithRefreshAsync<CurrentUserResponse>(HttpMethod.Get, Format(_apiSettings.Endpoints.Iam.Auth.CurrentUser));

    public Task<AppResponse<ProfilePictureResponse>> UpdateProfilePictureAsync(byte[] content, string fileName, string contentType)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        ArgumentException.ThrowIfNullOrWhiteSpace(contentType);

        return SendMultipartWithRefreshAsync<ProfilePictureResponse>(
            Format(_apiSettings.Endpoints.Iam.Auth.UpdateProfilePicture),
            () =>
            {
                var multipart = new MultipartFormDataContent();
                var fileContent = new ByteArrayContent(content);
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
                multipart.Add(fileContent, "file", fileName);
                return multipart;
            });
    }

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

    public Task<AppResponse<bool>> DisableTotpAsync()
        => SendWithRefreshAsync<bool>(
            HttpMethod.Post,
            Format(_apiSettings.Endpoints.Iam.Users.DisableTotp));

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

        return AppResponses.Success("Signed out", true);
    }

    private async Task<AppResponse<T>> SendWithRefreshAsync<T>(HttpMethod method, string endpoint)
    {
        var response = await SendAuthenticatedAsync<T>(method, endpoint).ConfigureAwait(false);
        if (response.Error?.Type != ErrorType.Unauthorized)
        {
            return response;
        }

        var (accessToken, refreshToken, _) = await storage.GetAsync().ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(accessToken) || string.IsNullOrWhiteSpace(refreshToken))
        {
            return response;
        }

        var refreshResponse = await RefreshTokenAsync(new RefreshTokenRequest(accessToken, refreshToken)).ConfigureAwait(false);
        if (!refreshResponse.IsSuccess)
        {
            await storage.ClearAsync().ConfigureAwait(false);
            return AppResponses.Failure<T>(refreshResponse.Message ?? "Your session has expired. Please sign in again.");
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

    private async Task<AppResponse<T>> SendMultipartWithRefreshAsync<T>(
        string endpoint,
        Func<MultipartFormDataContent> contentFactory)
    {
        using var content = contentFactory();
        var response = await apiClient.SendMultipartAsync<T>(
                endpoint,
                content,
                requiresAuthentication: true,
                unavailableMessage: "The identity service is unavailable. Please try again.",
                timeoutMessage: "The identity service timed out. Please try again.")
            .ConfigureAwait(false);

        if (response.Error?.Type != ErrorType.Unauthorized)
        {
            return response;
        }

        var (accessToken, refreshToken, _) = await storage.GetAsync().ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(accessToken) || string.IsNullOrWhiteSpace(refreshToken))
        {
            return response;
        }

        var refreshResponse = await RefreshTokenAsync(new RefreshTokenRequest(accessToken, refreshToken)).ConfigureAwait(false);
        if (!refreshResponse.IsSuccess)
        {
            await storage.ClearAsync().ConfigureAwait(false);
            return AppResponses.Failure<T>(refreshResponse.Message ?? "Your session has expired. Please sign in again.");
        }

        using var retryContent = contentFactory();
        return await apiClient.SendMultipartAsync<T>(
                endpoint,
                retryContent,
                requiresAuthentication: true,
                unavailableMessage: "The identity service is unavailable. Please try again.",
                timeoutMessage: "The identity service timed out. Please try again.")
            .ConfigureAwait(false);
    }

    private string Format(string endpoint, params (string Key, string Value)[] parameters)
        => EndpointFormatter.Format(endpoint, _apiSettings.Version, parameters.ToDictionary(static item => item.Key, static item => item.Value));
}
