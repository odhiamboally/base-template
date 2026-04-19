using BT.Application.Contracts.Interfaces.Common;
using BT.Application.IntegrationEvents;
using BT.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace BT.Application.Features.Auth.EventHandlers;

internal sealed class AppUserCreatedEventHandler(IIntegrationEventPublisher integrationEventPublisher, ILogger<AppUserCreatedEventHandler> logger)
    : INotificationHandler<AppUserCreatedEvent>
{
    public async Task Handle(AppUserCreatedEvent evt, CancellationToken cancellationToken)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(evt);

            var integrationEvent = new AppUserCreatedIntegrationEvent(
                evt.UserId,
                evt.TenantId,
                evt.EmployeeId,
                evt.UserName,
                evt.FullName,
                evt.Email);

            await integrationEventPublisher.PublishAsync(integrationEvent, cancellationToken).ConfigureAwait(false);

            logger.LogInformation("AppUserCreatedIntegrationEvent published for {UserId}", evt.UserId);
                
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to publish integration event for AppUser {UserId}", evt?.UserId);
            throw;
        }
    }
}
