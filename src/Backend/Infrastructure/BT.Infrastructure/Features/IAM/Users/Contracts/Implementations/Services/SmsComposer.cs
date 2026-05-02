using BT.Application.Features.IAM.Users.Contracts.Interfaces;
using BT.Application.Features.Shared.Notifications.Contracts.Interfaces;
using BT.Domain.Features.IAM.Users.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace BT.Infrastructure.Features.IAM.Users.Contracts.Implementations.Services;

internal sealed class SmsComposer : ISmsComposer
{
    public SmsComposer()
    {

    }

    public async Task<string> ComposePasswordResetSmsAsync(RequestPasswordResetEvent evt)
    {
        var message = $"Dear {evt.FirstName}, your SACCO security code is {evt.ValidationCode}. " +
                      $"Valid for 10 mins. Do not share this code.";

        return await Task.FromResult(message).ConfigureAwait(false);
    }
}
