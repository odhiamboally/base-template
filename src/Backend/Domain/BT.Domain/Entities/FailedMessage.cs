using BT.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace BT.Domain.Entities;

public class FailedMessage : BaseEntity
{
    public required string MessageId { get; set; }
    public required string MessageType { get; set; }
    public string? EntityId { get; set; } // Generic - could be ClientId, OrderId, etc.
    public required string Payload { get; set; }
    public required string ErrorMessage { get; set; }
    public string? ErrorStackTrace { get; set; }
    public int RetryCount { get; set; }
    public DateTimeOffset FailedAt { get; set; }
    public FailedMessageStatus Status { get; set; }
    public bool IsResolved { get; set; }
    public DateTimeOffset? ResolvedAt { get; set; }
    public string? ResolutionNotes { get; set; }
}
