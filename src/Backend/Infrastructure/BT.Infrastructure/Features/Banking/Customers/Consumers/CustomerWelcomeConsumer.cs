using BT.Application.Features.IAM.Users.Contracts.Interfaces;
using BT.Application.Features.Shared.Notifications.Contracts.Interfaces;
using BT.Application.IntegrationEvents;
using BT.Domain.Shared.Contracts;
using BT.Infrastructure.Messaging.Consumers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace BT.Infrastructure.Features.Banking.Customers.Consumers;


internal sealed class CustomerWelcomeConsumer(
    IEmailComposer<CustomerCreatedIntegrationEvent> composer,
    ISharedUnitOfWork shared,
    ILogger<IntegrationEventEmailConsumer<CustomerCreatedIntegrationEvent>> logger)
    : IntegrationEventEmailConsumer<CustomerCreatedIntegrationEvent>(composer, shared, logger)
{ 


}
