using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace BT.Persistence.Logging;

internal static partial class PersistenceLogDefinitions
{
    [LoggerMessage(EventId = 4000, Level = LogLevel.Information, Message = "Entity {EntityTypeName} with ID {EntityId} created by {User}")]
    public static partial void LogEntityCreated(ILogger logger, string entityTypeName, Guid entityId, string user);

    [LoggerMessage(EventId = 4001, Level = LogLevel.Information, Message = "Entity {EntityTypeName} with ID {EntityId} updated by {User}")]
    public static partial void LogEntityUpdated(ILogger logger, string entityTypeName, Guid entityId, string user);

    [LoggerMessage(EventId = 4002, Level = LogLevel.Information, Message = "Entity {EntityTypeName} with ID {EntityId} was soft-deleted by {User}")]
    public static partial void LogSoftDelete(ILogger logger, string entityTypeName, Guid entityId, string user);

    [LoggerMessage(EventId = 4100, Level = LogLevel.Error, Message = "Concurrency conflict on {EntityTypeName} (ID: {EntityId})")]
    public static partial void LogConcurrencyConflict(ILogger logger, string entityTypeName, string entityId);

    [LoggerMessage(EventId = 4101, Level = LogLevel.Error, Message = "Database operation failed for {EntityType}")]
    public static partial void LogDatabaseError(ILogger logger, string entityType, Exception ex);

    [LoggerMessage(EventId = 4200, Level = LogLevel.Debug, Message = "Executing SQL: {SqlCommand}")]
    public static partial void LogSqlExecution(ILogger logger, string sqlCommand);

    [LoggerMessage(EventId = 4201, Level = LogLevel.Warning, Message = "Concurrency conflict detected while executing transaction. Rolling back and clearing change tracker.")]
    public static partial void LogTransactionConcurrencyRollback(ILogger logger, Exception ex);

    [LoggerMessage(EventId = 4202, Level = LogLevel.Error, Message = "Error in transaction. Rolling back...")]
    public static partial void LogTransactionErrorRollback(ILogger logger, Exception ex);

    [LoggerMessage(EventId = 4203, Level = LogLevel.Debug, Message = "Successfully saved and published {EventCount} domain events")]
    public static partial void LogEventsPublished(ILogger logger, int eventCount);

    [LoggerMessage(EventId = 4204, Level = LogLevel.Error, Message = "Transaction failed - rolling back")]
    public static partial void LogCompleteWithEventsRollback(ILogger logger, Exception ex);
}
