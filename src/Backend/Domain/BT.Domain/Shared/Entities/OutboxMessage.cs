using BT.Domain.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace BT.Domain.Shared.Entities;

public class OutboxMessage : BaseEntity
{
    public OutboxMessageType MessageType { get; set; }          // Event/message type
    public string? Payload { get; set; }       // Serialized message content
    public DateTimeOffset OccurredAt { get; set; }  // When the event occurred
    public DateTimeOffset? ProcessedOn { get; set; } // When message was published
    public DateTimeOffset? NextAttemptUtc { get; set; } = DateTime.UtcNow;
    public int AttemptCount { get; set; }
    public OutboxMessageStatus Status { get; set; }
    public string? Error { get; set; }        // Any processing error
}
