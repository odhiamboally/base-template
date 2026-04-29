using System;
using System.Collections.Generic;
using System.Text;

namespace BT.SharedKernel.Dtos.Common;

public record SendEmailResponse(
    string MessageId,
    DateTimeOffset SentAt,
    string Recipient,
    string Subject
);
