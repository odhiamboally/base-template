using BT.Domain.IAM.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace BT.Application.Contracts.Interfaces.Services;

public interface ISmsComposer
{
    Task<string> ComposePasswordResetSmsAsync(RequestPasswordResetEvent evt);
}
