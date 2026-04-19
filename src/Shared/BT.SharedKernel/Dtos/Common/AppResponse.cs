using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Text.Json.Serialization;

namespace BT.SharedKernel.Dtos.Common;


public record AppResponse<T>
{
    public bool Successful { get; init; }
    public string? Message { get; init; }
    public string? SessionId { get; init; }
    public T? Data { get; init; }
    public Collection<string> Errors { get; init; } = [];
    public Dictionary<string, string> ValidationErrors { get; init; } = [];
    public string? ErrorCode { get; init; }
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;
    public string? TraceId { get; init; }

    [JsonIgnore]
    public Exception? Exception { get; init; }

    public AppResponse()
    {
    }

    [JsonConstructor]
    internal AppResponse(bool successful, string? message, T? data, Exception? exception)
    {
        Successful = successful;
        Message = message ?? "Operation Successful";
        Data = data;
        Exception = exception;
    }

   
}

public static class AppResponse
{
    public static AppResponse<T> Success<T>(T data)
    {
        return new AppResponse<T>
        {
            Successful = true,
            Data = data
        };
    }

    public static AppResponse<T> Success<T>(string message, T data)
    {
        return new AppResponse<T>
        {
            Successful = true,
            Message = message,
            Data = data
        };
    }

    public static AppResponse<T> Success<T>(string message, T data, Exception? exception = null) 
        => new(true, message, data, exception);

    public static AppResponse<T> Failure<T>(string message, T? data = default, Exception? exception = null) 
        => new(false, message, data, exception);

    public static AppResponse<T> ValidationFailure<T>(Dictionary<string, List<string>> validationErrors)
    {
        ArgumentNullException.ThrowIfNull(validationErrors, nameof(validationErrors));

        var totalErrorCount = 0;
        foreach (var entry in validationErrors)
        {
            totalErrorCount += entry.Value.Count;
        }

        var allErrors = new List<string>(totalErrorCount);
        var flattenedValidationErrors = new Dictionary<string, string>(validationErrors.Count);

        foreach (var (key, value) in validationErrors)
        {
            foreach (var error in value)
            {
                allErrors.Add($"{key}: {error}");
            }

            flattenedValidationErrors[key] = string.Join(", ", value);
        }

        return new AppResponse<T>
        {
            Successful = false,
            Message = "One or more validation failures have occurred.",
            Errors = new Collection<string>(allErrors),
            ValidationErrors = flattenedValidationErrors,
            ErrorCode = "VALIDATION_ERROR"
        };
    }
}


