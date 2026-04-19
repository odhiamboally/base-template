using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace BT.Infrastructure.Logging;

internal static partial class MessageBusLogDefinitions
{
    [LoggerMessage(EventId = 3304, Level = LogLevel.Error, Message = "Failed to process {MessageType} {MessageId} after {ElapsedMs}ms. Retry attempt: {RetryCount}, Redelivered: {IsRedelivered}. Error: {ErrorMessage}")]
    public static partial void LogConsumeFailure(ILogger logger, string messageType, string messageId, long elapsedMs, int retryCount, bool isRedelivered, string errorMessage, Exception ex);

    [LoggerMessage(EventId = 3305, Level = LogLevel.Error, Message = "Failed consuming ClientCreatedIntegrationEvent for client {ClientId}")]
    public static partial void LogClientCreatedIntegrationConsumeError(ILogger logger, Guid clientId, Exception ex);

    [LoggerMessage(EventId = 3306, Level = LogLevel.Warning, Message = "Email template not found: {TemplateName}")]
    public static partial void LogEmailTemplateNotFound(ILogger logger, string templateName);

    [LoggerMessage(EventId = 3307, Level = LogLevel.Warning, Message = "Email template mismatch. Expected {ExpectedTemplateName}, Actual {ActualTemplateName}")]
    public static partial void LogEmailTemplateMismatch(ILogger logger, string expectedTemplateName, string actualTemplateName);

    [LoggerMessage(EventId = 3308, Level = LogLevel.Error, Message = "Permanent consumer failure after {RetryCount} attempts")]
    public static partial void LogPermanentConsumerFailure(ILogger logger, int retryCount, Exception ex);

    [LoggerMessage(EventId = 3309, Level = LogLevel.Warning, Message = "Temporary consumer failure on attempt {RetryCount}")]
    public static partial void LogTemporaryConsumerFailure(ILogger logger, int retryCount, Exception ex);
}
