using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using BT.SharedKernel.Dtos.Common;
using BT.UI.Blazor.Features.Shared.BackendApi.Contracts.Interfaces;
using BT.UI.Blazor.Features.Shared.Messaging;
using BT.UI.Blazor.Logging;
using BT.UI.Rcl.Features.IAM.Users.Contracts.Interfaces;

namespace BT.UI.Blazor.Features.Shared.BackendApi.Contracts.Implementations;

internal sealed class BackendApiClient(HttpClient httpClient, ITokenStorage storage, ILogger<BackendApiClient> logger) : IBackendApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<AppResponse<T>> SendAsync<T>(
        HttpMethod method,
        string endpoint,
        object? request = null,
        bool requiresAuthentication = true,
        string? unavailableMessage = null,
        string? timeoutMessage = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(method);
        ArgumentException.ThrowIfNullOrWhiteSpace(endpoint);

        using var message = CreateRequest(method, endpoint, request);

        if (requiresAuthentication)
        {
            var (accessToken, _, sessionId) = await storage.GetAsync().ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                return AppResponse.Failure<T>("Please sign in to continue.");
            }

            message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            if (!string.IsNullOrWhiteSpace(sessionId))
            {
                message.Headers.TryAddWithoutValidation("X-Session-Id", sessionId);
            }
        }

        try
        {
            using var response = await httpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
            return await ReadAppResponseAsync<T>(response, endpoint).ConfigureAwait(false);
        }
        catch (HttpRequestException ex)
        {
            BackendApiLogDefinitions.LogRequestFailed(logger, method.Method, endpoint, ex);
            return AppResponse.Failure<T>(unavailableMessage ?? "The backend service is unavailable. Please try again.");
        }
        catch (TaskCanceledException ex)
        {
            BackendApiLogDefinitions.LogRequestTimedOut(logger, method.Method, endpoint, ex);
            return AppResponse.Failure<T>(timeoutMessage ?? "The backend service timed out. Please try again.");
        }
    }

    public async Task<AppResponse<T>> SendMultipartAsync<T>(
        string endpoint,
        MultipartFormDataContent content,
        bool requiresAuthentication = true,
        string? unavailableMessage = null,
        string? timeoutMessage = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(endpoint);
        ArgumentNullException.ThrowIfNull(content);

        using var message = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = content
        };

        if (requiresAuthentication)
        {
            var (accessToken, _, sessionId) = await storage.GetAsync().ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                return AppResponse.Failure<T>("Please sign in to continue.");
            }

            message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            if (!string.IsNullOrWhiteSpace(sessionId))
            {
                message.Headers.TryAddWithoutValidation("X-Session-Id", sessionId);
            }
        }

        try
        {
            using var response = await httpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
            return await ReadAppResponseAsync<T>(response, endpoint).ConfigureAwait(false);
        }
        catch (HttpRequestException ex)
        {
            BackendApiLogDefinitions.LogRequestFailed(logger, HttpMethod.Post.Method, endpoint, ex);
            return AppResponse.Failure<T>(unavailableMessage ?? "The backend service is unavailable. Please try again.");
        }
        catch (TaskCanceledException ex)
        {
            BackendApiLogDefinitions.LogRequestTimedOut(logger, HttpMethod.Post.Method, endpoint, ex);
            return AppResponse.Failure<T>(timeoutMessage ?? "The backend service timed out. Please try again.");
        }
    }

    private static HttpRequestMessage CreateRequest(HttpMethod method, string endpoint, object? request)
        => new(method, endpoint)
        {
            Content = request is null ? null : JsonContent.Create(request, options: JsonOptions)
        };

    private async Task<AppResponse<T>> ReadAppResponseAsync<T>(HttpResponseMessage response, string endpoint)
    {
        var appResponse = await TryReadAppResponseAsync<T>(response, endpoint).ConfigureAwait(false);
        if (appResponse is not null)
        {
            return appResponse with
            {
                Message = UserMessageSanitizer.NormalizeNullable(
                    appResponse.Message,
                    response.IsSuccessStatusCode
                        ? "Operation completed."
                        : "The request could not be completed. Please try again or contact support if the problem persists."),
                ErrorCode = appResponse.ErrorCode ?? (response.IsSuccessStatusCode ? null : response.StatusCode.ToString())
            };
        }

        return AppResponse.Failure<T>(
            response.IsSuccessStatusCode
                ? "The backend service returned an empty response."
                : UserMessageSanitizer.Normalize(
                    await ReadErrorMessageAsync(response).ConfigureAwait(false),
                    "The request could not be completed. Please try again or contact support if the problem persists."))
            with
            {
                ErrorCode = response.StatusCode.ToString()
            };
    }

    private async Task<AppResponse<T>?> TryReadAppResponseAsync<T>(HttpResponseMessage response, string endpoint)
    {
        try
        {
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(content))
            {
                return null;
            }

            using var document = JsonDocument.Parse(content);
            if (!document.RootElement.TryGetProperty("successful", out _))
            {
                return null;
            }

            return JsonSerializer.Deserialize<AppResponse<T>>(content, JsonOptions);
        }
        catch (JsonException ex)
        {
            BackendApiLogDefinitions.LogUnreadableResponse(logger, response.StatusCode.ToString(), endpoint, ex);
            return null;
        }
    }

    private static async Task<string> ReadErrorMessageAsync(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(content))
        {
            return "The backend service rejected the request.";
        }

        try
        {
            using var document = JsonDocument.Parse(content);
            return TryGetString(document.RootElement, "message")
                ?? TryGetString(document.RootElement, "detail")
                ?? TryGetString(document.RootElement, "title")
                ?? TryGetString(document.RootElement, "error")
                ?? TryGetSessionInvalidMessage(document.RootElement)
                ?? TryGetValidationErrors(document.RootElement)
                ?? "The backend service rejected the request.";
        }
        catch (JsonException)
        {
            return content.Length <= 200
                ? UserMessageSanitizer.Normalize(content, "The backend service rejected the request.")
                : "The backend service rejected the request.";
        }
    }

    private static string? TryGetString(JsonElement element, string propertyName)
        => element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : null;

    private static string? TryGetSessionInvalidMessage(JsonElement element)
    {
        var code = TryGetString(element, "code");
        if (!string.Equals(code, "SESSION_INVALID", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var reason = TryGetString(element, "reason");
        return string.IsNullOrWhiteSpace(reason)
            ? "Your session has expired. Please sign in again."
            : $"Your session has expired: {reason}. Please sign in again.";
    }

    private static string? TryGetValidationErrors(JsonElement element)
    {
        if (!element.TryGetProperty("errors", out var errors))
        {
            return null;
        }

        if (errors.ValueKind == JsonValueKind.Array)
        {
            var messages = errors
                .EnumerateArray()
                .Where(static error => error.ValueKind == JsonValueKind.String)
                .Select(static error => error.GetString())
                .Where(static error => !string.IsNullOrWhiteSpace(error));

            return string.Join("; ", messages);
        }

        if (errors.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        var keyedMessages = errors
            .EnumerateObject()
            .SelectMany(static property => property.Value.EnumerateArray())
            .Where(static error => error.ValueKind == JsonValueKind.String)
            .Select(static error => error.GetString())
            .Where(static error => !string.IsNullOrWhiteSpace(error));

        return string.Join("; ", keyedMessages);
    }
}
