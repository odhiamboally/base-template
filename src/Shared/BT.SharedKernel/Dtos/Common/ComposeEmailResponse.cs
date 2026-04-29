using System;
using System.Collections.Generic;
using System.Text;

namespace BT.SharedKernel.Dtos.Common;

public record ComposeEmailResponse(
    string TemplateName,
    string RecipientName,
    string RecipientEmail,
    string Subject,
    string Body
);
