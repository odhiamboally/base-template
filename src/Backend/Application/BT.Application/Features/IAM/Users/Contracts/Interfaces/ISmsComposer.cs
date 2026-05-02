using BT.Domain.Features.IAM.Users.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace BT.Application.Features.IAM.Users.Contracts.Interfaces;

public interface ISmsComposer
{
    Task<string> ComposePasswordResetSmsAsync(RequestPasswordResetEvent evt);
}
