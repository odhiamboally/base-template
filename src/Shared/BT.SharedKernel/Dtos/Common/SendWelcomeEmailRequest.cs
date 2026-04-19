using System;
using System.Collections.Generic;
using System.Text;

namespace BT.SharedKernel.Dtos.Common;

public record SendWelcomeEmailRequest(Guid ClientId, string ClientNumber, string ClientName, string ClientEmail, string ClientType);