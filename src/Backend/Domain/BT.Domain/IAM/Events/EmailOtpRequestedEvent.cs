using BT.Domain.Banking.Enums;
using BT.Domain.HR.Enums;
using BT.Domain.IAM.Enums;
using BT.Domain.Shared.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace BT.Domain.IAM.Events;

public sealed record EmailOtpRequestedEvent(
    string UserId,
    string Email,
    string FirstName,
    string Code,
    string Purpose,
    DateTimeOffset ExpiresAt
) : INotification;
