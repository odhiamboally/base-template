using BT.Application.Contracts.Interfaces.Services;
using BT.Domain.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace BT.Infrastructure.Contracts.Implementations.Services;

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
