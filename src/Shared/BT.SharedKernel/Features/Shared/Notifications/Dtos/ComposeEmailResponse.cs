using System;
using System.Collections.Generic;
using System.Text;

namespace BT.SharedKernel.Features.Shared.Notifications.Dtos;

public record ComposeEmailResponse(
    string TemplateName,
    string RecipientName,
    string RecipientEmail,
    string Subject,
    string Body
);
