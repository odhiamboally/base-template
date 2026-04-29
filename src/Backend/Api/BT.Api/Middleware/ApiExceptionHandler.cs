using BT.Application.Exceptions;
using BT.Domain.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Net;
using DataAnnotationsValidationException = System.ComponentModel.DataAnnotations.ValidationException;

namespace BT.Api.Middleware;

internal sealed class ApiExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var status = (int)HttpStatusCode.InternalServerError;
        var title = "Internal Server Error";
        var detail = "An internal server error occurred. Please contact support if the problem persists.";

        var problemDetails = new ProblemDetails
        {
            Instance = httpContext.Request.Path,
        };

        switch (exception)
        {
            case ServiceUnavailableException:
                status = (int)HttpStatusCode.ServiceUnavailable;
                title = "Service Unavailable";
                detail = exception.Message;
                break;

            case ResourceNotFoundException:
            case KeyNotFoundException:
                status = (int)HttpStatusCode.NotFound;
                title = "Resource Not Found";
                detail = exception.Message;
                break;

            case DataAnnotationsValidationException validationException:
                status = (int)HttpStatusCode.UnprocessableEntity;
                title = "Validation Error";
                detail = validationException.ValidationResult?.ErrorMessage ?? exception.Message;

                var errors = new Dictionary<string, string[]>();
                foreach (var memberName in validationException.ValidationResult?.MemberNames ?? [])
                {
                    errors[memberName] = [validationException.ValidationResult?.ErrorMessage ?? "Validation error"];
                }

                problemDetails.Extensions["errors"] = errors;
                break;

            case FluentValidation.ValidationException fluentEx:
                status = (int)HttpStatusCode.BadRequest;
                title = "Validation Error";
                detail = "One or more validation errors occurred.";

                var validationErrors = fluentEx.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray());

                problemDetails.Extensions["errors"] = validationErrors;
                break;

            case CreatingDuplicateException:
                status = StatusCodes.Status409Conflict;
                title = "Conflict";
                detail = string.IsNullOrWhiteSpace(exception.Message)
                    ? "A resource with the same identifier already exists."
                    : exception.Message;
                break;

            case DomainException:
            case ArgumentException:
            case FormatException:
                status = (int)HttpStatusCode.BadRequest;
                title = "Bad Request";
                detail = exception.Message;
                break;

            case CustomException customException:
                status = (int)customException.StatusCode;
                title = customException.StatusCode.ToString();
                detail = customException.Message;

                if (customException.ErrorMessages is { Count: > 0 })
                {
                    problemDetails.Extensions["errors"] = customException.ErrorMessages;
                }
                break;

            case HttpRequestException:
                status = (int)HttpStatusCode.ServiceUnavailable;
                title = "Service Unavailable";
                detail = exception.Message;
                break;

            default:
                status = (int)HttpStatusCode.InternalServerError;
                title = "Internal Server Error";
                detail = "An internal server error occurred. Please contact support if the problem persists.";
                break;

        }

        problemDetails.Status = status;
        problemDetails.Title = title;
        problemDetails.Detail = detail;
        problemDetails.Type = $"https://httpstatuses.com/{status}";
        problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;

        httpContext.Response.StatusCode = problemDetails.Status.Value;

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken: cancellationToken).ConfigureAwait(false);

        return true;
    }


}

