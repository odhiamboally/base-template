using Microsoft.Extensions.Logging;

namespace BT.Api.Logging;

internal static partial class PaymentLogDefinitions
{
    [LoggerMessage(
        EventId = 4000,
        Level = LogLevel.Error,
        Message = "Error processing M-Pesa C2B confirmation: {Error}")]
    public static partial void LogMpesaC2bConfirmationError(ILogger logger, string error);
}
