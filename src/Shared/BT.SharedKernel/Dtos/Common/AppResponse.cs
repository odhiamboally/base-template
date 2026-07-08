using System;
using System.Text.Json.Serialization;

namespace BT.SharedKernel.Dtos.Common;


public record AppResponse<T>
{
    public bool IsSuccess { get; init; }
    public string? Message { get; init; }
    public string? SessionId { get; init; }
    public T? Data { get; init; }

    // Failures are converted to ProblemDetails by BaseController.
    [JsonIgnore]
    public AppError? Error { get; }

    public AppResponse() { }

    public AppResponse(T? data)
    {
        IsSuccess = true;
        Message = "Operation Successful";
        Data = data;
    }

    [JsonConstructor]
    internal AppResponse(bool isSuccess, string? message, T? data)
    {
        IsSuccess = isSuccess;
        Message = message ?? "Operation Successful";
        Data = data;
    }

    [JsonPropertyName("successful")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonInclude]
    internal bool? Successful
    {
        get => null;
        init
        {
            if (value.HasValue)
            {
                IsSuccess = value.Value;
            }
        }
    }

    internal AppResponse(AppError error)
    {
        ArgumentNullException.ThrowIfNull(error);

        IsSuccess = false;
        Error = error;
    }

    public AppResponse(T? data, string message)
    {
        IsSuccess = true;
        Data = data;
        Message = message;
    }
}
