using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace BT.Domain.Features.Shared.Outbox.Enums;

public enum OutboxMessageType
{
    None,

    // --- Domain Events ---
    UserCreated,
    UserUpdated,
    UserDeleted,

    // --- Notification Events ---
    EmailRequested,
    SmsRequested,
    PushNotificationRequested
}

