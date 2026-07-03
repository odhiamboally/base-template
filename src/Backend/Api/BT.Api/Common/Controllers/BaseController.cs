using BT.SharedKernel.Dtos.Common;
using BT.SharedKernel.Features.Shared.Common.Enums;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using OpenTelemetry.Trace;

using System.Diagnostics;

namespace BT.Api.Common.Controllers;

public abstract class BaseController : ControllerBase
{
    //protected IActionResult HandleResponse<T>(AppResponse<T> response)
    //{
    //    ArgumentNullException.ThrowIfNull(response);

    //    if (response.IsSuccess && response.Data != null)
    //    {
    //        return Ok(response);
    //    }

    //    // Use Problem Details format for failures
    //    var problemDetails = new ProblemDetails
    //    {
    //        Instance = HttpContext.Request.Path,
    //    };

    //    if (!response.IsSuccess)
    //    {
    //        var (statusCode, title) = DetermineErrorType(response.ErrorCode, response.Message ?? string.Empty);

    //        problemDetails.Status = statusCode;
    //        problemDetails.Title = title;
    //        problemDetails.Detail = response.Message;

    //        // Add additional context if available
    //        if (response.Errors != null && response.Errors.Any())
    //        {
    //            // Since Errors is a List<string>, group all errors under "general"
    //            problemDetails.Extensions["errors"] = new Dictionary<string, string[]>
    //            {
    //                { "general", response.Errors.ToArray() }
    //            };
    //        }

    //        return StatusCode(statusCode, problemDetails);
    //    }

    //    // Handle case where successful but data is null
    //    problemDetails.Status = StatusCodes.Status404NotFound;
    //    problemDetails.Title = "Resource Not Found";
    //    problemDetails.Detail = "The requested resource was not found.";

    //    return NotFound(problemDetails);
    //}

    protected IActionResult HandleResponse<T>(AppResponse<T> response, Func<AppResponse<T>, IActionResult>? onSuccess = null)
    {
        ArgumentNullException.ThrowIfNull(response);

        if (response.IsSuccess)
        {
            // Preserve your current success-envelope convention.
            // Change to Ok(response.Data) only if that is a deliberate API-wide decision.
            return onSuccess?.Invoke(response) ?? Ok(response);
        }

        return ToProblem(response.Error ?? AppError.Unexpected());
    }

    private ObjectResult ToProblem(AppError error)
    {
        var (statusCode, title) = GetHttpMetadata(error.Type);

        if (error.Type == ErrorType.Validation)
        {
            var errors = error.ValidationErrors is null
                ? new Dictionary<string, string[]>()
                : new Dictionary<string, string[]>(error.ValidationErrors);

            var validationProblem = new ValidationProblemDetails(errors)
            {
                Status = statusCode,
                Title = title,
                Detail = error.Message,
                Type = ToProblemType(error.Code),
                Instance = HttpContext.Request.Path.Value
            };

            AddDiagnostics(validationProblem, error);

            return StatusCode(statusCode, validationProblem);
        }

        if (error.Type == ErrorType.Validation)
        {
            var validationErrors = error.ValidationErrors is null
                ? new Dictionary<string, string[]>()
                : new Dictionary<string, string[]>(error.ValidationErrors);

            var validationProblemDetails = new ValidationProblemDetails(validationErrors)
            {
                Status = statusCode,
                Title = title,
                Detail = error.Message,
                Instance = HttpContext.Request.Path
            };

            AddDiagnostics(validationProblemDetails, error);

            return StatusCode(statusCode, validationProblemDetails);
        }

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            // Never accidentally expose internal exception details.
            Detail = error.Type == ErrorType.Unexpected
                ? "An unexpected error occurred."
                : error.Message,
            Type = ToProblemType(error.Code),
            Instance = HttpContext.Request.Path.Value
        };

        AddDiagnostics(problemDetails, error);

        return StatusCode(statusCode, problemDetails);
    }

    private static (int StatusCode, string Title) GetHttpMetadata(ErrorType type) =>
        type switch
        {
            ErrorType.Validation =>
                (StatusCodes.Status400BadRequest, "Validation failed"),

            ErrorType.NotFound =>
                (StatusCodes.Status404NotFound, "Resource not found"),

            ErrorType.Unauthorized =>
                (StatusCodes.Status401Unauthorized, "Authentication required"),

            ErrorType.Forbidden =>
                (StatusCodes.Status403Forbidden, "Access denied"),

            ErrorType.Conflict =>
                (StatusCodes.Status409Conflict, "Conflict"),

            ErrorType.BusinessRule =>
                (StatusCodes.Status422UnprocessableEntity, "Business rule violation"),

            ErrorType.DependencyUnavailable =>
                (StatusCodes.Status503ServiceUnavailable, "Dependency unavailable"),

            ErrorType.Unexpected =>
                (StatusCodes.Status500InternalServerError, "Internal server error"),

            _ =>
                (StatusCodes.Status500InternalServerError, "Internal server error")
        };

    private static string ToProblemType(string code) =>
        $"urn:bt:problem:{code.ToLowerInvariant().Replace('_', '-')}";

    private void AddDiagnostics(ProblemDetails problem, AppError error)
    {
        problem.Extensions["code"] = error.Code;
        problem.Extensions["traceId"] = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
    }


}
