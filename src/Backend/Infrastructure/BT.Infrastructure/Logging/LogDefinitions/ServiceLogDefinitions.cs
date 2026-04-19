using Microsoft.Extensions.Logging;
using System;

namespace BT.Infrastructure.Logging;

internal static partial class ServiceLogDefinitions
{
    [LoggerMessage(EventId = 3400, Level = LogLevel.Error, Message = "Error composing email for template {EmailTemplate}")]
    public static partial void LogEmailComposeError(ILogger logger, string emailTemplate, Exception ex);

    [LoggerMessage(EventId = 3401, Level = LogLevel.Error, Message = "Error checking concurrent sessions for user: {UserId}")]
    public static partial void LogSessionConcurrentCheckError(ILogger logger, string userId, Exception ex);

    [LoggerMessage(EventId = 3402, Level = LogLevel.Error, Message = "Failed to end old sessions for user {UserId}: {Message}")]
    public static partial void LogFailedToEndOldSessions(ILogger logger, string userId, string message);

    [LoggerMessage(EventId = 3405, Level = LogLevel.Error, Message = "Error creating session for user: {UserId}")]
    public static partial void LogSessionCreateError(ILogger logger, string userId, Exception ex);

    [LoggerMessage(EventId = 3410, Level = LogLevel.Error, Message = "Error ending session: {SessionId}")]
    public static partial void LogSessionEndError(ILogger logger, string sessionId, Exception ex);

    [LoggerMessage(EventId = 3411, Level = LogLevel.Error, Message = "Error ending all sessions for user: {UserId}")]
    public static partial void LogEndAllSessionsError(ILogger logger, string userId, Exception ex);

    [LoggerMessage(EventId = 3412, Level = LogLevel.Error, Message = "Error getting active sessions for user: {UserId}")]
    public static partial void LogGetActiveSessionsError(ILogger logger, string userId, Exception ex);

    [LoggerMessage(EventId = 3414, Level = LogLevel.Error, Message = "Error during session cleanup")]
    public static partial void LogSessionCleanupError(ILogger logger, Exception ex);

    [LoggerMessage(EventId = 3415, Level = LogLevel.Error, Message = "Error validating session: {SessionId}")]
    public static partial void LogSessionValidationError(ILogger logger, string sessionId, Exception ex);

    [LoggerMessage(EventId = 3416, Level = LogLevel.Error, Message = "Error initiating TOTP setup for user {UserId}")]
    public static partial void LogTotpSetupInitiationError(ILogger logger, string userId, Exception ex);

    [LoggerMessage(EventId = 3417, Level = LogLevel.Error, Message = "Error finalizing TOTP setup for user {UserId}")]
    public static partial void LogTotpSetupFinalizationError(ILogger logger, string userId, Exception ex);

    [LoggerMessage(EventId = 3418, Level = LogLevel.Error, Message = "Error verifying TOTP code for user {UserId}")]
    public static partial void LogTotpVerificationError(ILogger logger, string userId, Exception ex);

    [LoggerMessage(EventId = 3419, Level = LogLevel.Error, Message = "Error verifying TOTP code")]
    public static partial void LogTotpCodeVerificationError(ILogger logger, Exception ex);

    [LoggerMessage(EventId = 3420, Level = LogLevel.Error, Message = "Error verifying TOTP code with plain text secret")]
    public static partial void LogTotpPlainTextCodeVerificationError(ILogger logger, Exception ex);
}
