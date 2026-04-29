using BT.Application.Contracts.Interfaces.Common;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace BT.Infrastructure.Logging;

internal static partial class SecurityLogDefinitions
{
    [LoggerMessage(EventId = 3000, Level = LogLevel.Warning, Message = "Security Event: {EventType} for User {UserId}. Details: {Details}")]
    public static partial void LogSecurityEvent(ILogger logger, string eventType, string userId, string details);
}
