using BT.SharedKernel.Dtos.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BT.Api.Common.Controllers;

public abstract class BaseController : ControllerBase
{
    protected IActionResult HandleResponse<T>(AppResponse<T> response)
    {
        ArgumentNullException.ThrowIfNull(response);

        if (response.Successful && response.Data != null)
        {
            return Ok(response);
        }

        // Use Problem Details format for failures
        var problemDetails = new ProblemDetails
        {
            Instance = HttpContext.Request.Path,
        };

        if (!response.Successful)
        {
            var (statusCode, title) = DetermineErrorType(response.ErrorCode, response.Message ?? string.Empty);

            problemDetails.Status = statusCode;
            problemDetails.Title = title;
            problemDetails.Detail = response.Message;

            // Add additional context if available
            if (response.Errors != null && response.Errors.Any())
            {
                // Since Errors is a List<string>, group all errors under "general"
                problemDetails.Extensions["errors"] = new Dictionary<string, string[]>
                {
                    { "general", response.Errors.ToArray() }
                };
            }

            return StatusCode(statusCode, problemDetails);
        }

        // Handle case where successful but data is null
        problemDetails.Status = StatusCodes.Status404NotFound;
        problemDetails.Title = "Resource Not Found";
        problemDetails.Detail = "The requested resource was not found.";

        return NotFound(problemDetails);
    }

    private static (int statusCode, string title) DetermineErrorType(string? errorCode, string message)
    {
        var normalizedErrorCode = errorCode?.Trim().ToUpperInvariant();
        if (!string.IsNullOrWhiteSpace(normalizedErrorCode))
        {
            return normalizedErrorCode switch
            {
                "VALIDATION_ERROR" => (StatusCodes.Status400BadRequest, "Validation Error"),
                "NOT_FOUND" => (StatusCodes.Status404NotFound, "Resource Not Found"),
                "FORBIDDEN" or "ACCESS_DENIED" => (StatusCodes.Status403Forbidden, "Access Denied"),
                "CONFLICT" => (StatusCodes.Status409Conflict, "Conflict"),
                "BUSINESS_RULE" => (StatusCodes.Status422UnprocessableEntity, "Business Rule Violation"),
                "SERVICE_UNAVAILABLE" => (StatusCodes.Status503ServiceUnavailable, "Service Unavailable"),
                _ => (StatusCodes.Status500InternalServerError, "Internal Server Error")
            };
        }

        // Compatibility fallback for older handlers that still return only messages.
        var lowerMessage = message?.ToLowerInvariant() ?? "";

        if (lowerMessage.Contains("validation") || lowerMessage.Contains("invalid"))
            return (StatusCodes.Status400BadRequest, "Validation Error");

        if (lowerMessage.Contains("not found") || lowerMessage.Contains("does not exist"))
            return (StatusCodes.Status404NotFound, "Resource Not Found");

        if (lowerMessage.Contains("unauthorized") || lowerMessage.Contains("permission"))
            return (StatusCodes.Status403Forbidden, "Access Denied");

        if (lowerMessage.Contains("duplicate") || lowerMessage.Contains("already exists"))
            return (StatusCodes.Status409Conflict, "Conflict");

        if (lowerMessage.Contains("service unavailable") || lowerMessage.Contains("connection") ||
            lowerMessage.Contains("timeout") || lowerMessage.Contains("external service"))
            return (StatusCodes.Status503ServiceUnavailable, "Service Unavailable");

        if (lowerMessage.Contains("business rule") || lowerMessage.Contains("cannot") ||
            lowerMessage.Contains("insufficient") || lowerMessage.Contains("exceed"))
            return (StatusCodes.Status422UnprocessableEntity, "Business Rule Violation");

        return (StatusCodes.Status500InternalServerError, "Internal Server Error");
    }

}

