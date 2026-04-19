using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace BT.Application.Utilities;

internal static partial class LogDefinitions
{
    [LoggerMessage(EventId = 2300, Level = LogLevel.Error, Message = "MediatR Pipeline caught exception for {RequestName}")]
    public static partial void LogPipelineException(ILogger logger, string requestName, Exception ex);

    [LoggerMessage(EventId = 2301, Level = LogLevel.Error, Message = "An unexpected error occurred: {ErrorMessage}")]
    public static partial void LogUnexpectedError(ILogger logger, string errorMessage, Exception exception);

    [LoggerMessage(EventId = 2302, Level = LogLevel.Information, Message = "Customer created: {CustomerNumber} — {CompanyName}")]
    public static partial void LogCustomerCreated(ILogger logger, string customerNumber, string companyName);

    [LoggerMessage(EventId = 2303, Level = LogLevel.Warning, Message = "Domain validation failed creating customer")]
    public static partial void LogCustomerCreateValidationFailed(ILogger logger, Exception ex);

    [LoggerMessage(EventId = 2304, Level = LogLevel.Error, Message = "Error creating customer")]
    public static partial void LogCustomerCreateFailed(ILogger logger, Exception ex);

    [LoggerMessage(EventId = 2305, Level = LogLevel.Information, Message = "Customer updated: {CustomerNumber}")]
    public static partial void LogCustomerUpdated(ILogger logger, string customerNumber);

    [LoggerMessage(EventId = 2306, Level = LogLevel.Error, Message = "Error updating customer {CustomerId}")]
    public static partial void LogCustomerUpdateFailed(ILogger logger, Guid customerId, Exception ex);

    [LoggerMessage(EventId = 2307, Level = LogLevel.Error, Message = "Error deleting customer {CustomerId}")]
    public static partial void LogCustomerDeleteFailed(ILogger logger, Guid customerId, Exception ex);

    [LoggerMessage(EventId = 2308, Level = LogLevel.Information, Message = "Successfully published CustomerCreatedIntegrationEvent for customer {CustomerId}")]
    public static partial void LogCustomerCreatedIntegrationPublished(ILogger logger, Guid customerId);

    [LoggerMessage(EventId = 2309, Level = LogLevel.Error, Message = "Error occurred while handling CustomerCreatedDomainEvent for MemberEmail: {MemberEmail}")]
    public static partial void LogCustomerCreatedDomainEventHandlerError(ILogger logger, string memberEmail, Exception ex);

    [LoggerMessage(EventId = 2310, Level = LogLevel.Error, Message = "Error consuming CustomerCreatedIntegrationEvent for CustomerId: {CustomerId}")]
    public static partial void LogCustomerCreatedIntegrationConsumeError(ILogger logger, Guid customerId, Exception ex);

    [LoggerMessage(EventId = 2311, Level = LogLevel.Error, Message = "Email template '{TemplateName}' not found in database")]
    public static partial void LogEmailTemplateNotFound(ILogger logger, string templateName);

    [LoggerMessage(EventId = 2312, Level = LogLevel.Error, Message = "Email template mismatch: expected '{Expected}', found '{Actual}'")]
    public static partial void LogEmailTemplateMismatch(ILogger logger, string expected, string actual);

    [LoggerMessage(EventId = 2313, Level = LogLevel.Critical, Message = "Permanent failure after {Attempt} attempts - moving to dead letter")]
    public static partial void LogPermanentConsumerFailure(ILogger logger, int attempt, Exception ex);

    [LoggerMessage(EventId = 2314, Level = LogLevel.Warning, Message = "Temporary failure (attempt {Attempt}) - will retry")]
    public static partial void LogTemporaryConsumerFailure(ILogger logger, int attempt, Exception ex);

    [LoggerMessage(EventId = 2315, Level = LogLevel.Error, Message = "Error fetching customer {CustomerId}")]
    public static partial void LogGetCustomerByIdFailed(ILogger logger, Guid customerId, Exception ex);

    [LoggerMessage(EventId = 2316, Level = LogLevel.Error, Message = "Error fetching customer list")]
    public static partial void LogCustomerListFetchFailed(ILogger logger, Exception ex);

    [LoggerMessage(EventId = 2317, Level = LogLevel.Error, Message = "Error fetching dashboard summary")]
    public static partial void LogDashboardSummaryFetchFailed(ILogger logger, Exception ex);

    [LoggerMessage(EventId = 2318, Level = LogLevel.Error, Message = "Error fetching staff members")]
    public static partial void LogStaffMembersFetchFailed(ILogger logger, Exception ex);

}
