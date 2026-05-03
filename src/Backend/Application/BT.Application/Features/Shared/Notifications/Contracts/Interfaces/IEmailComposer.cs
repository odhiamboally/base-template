using BT.SharedKernel.Features.Shared.Notifications.Dtos;
using BT.Application.Features.Banking.Customers.IntegrationEvents;
using BT.Application.Features.HR.Employees.IntegrationEvents;
using BT.Application.Features.IAM.Users.IntegrationEvents;
using BT.Domain.Features.HR.Employees.Events;
using BT.Domain.Features.IAM.Users.Events;
using BT.Domain.Shared.Contracts.Common;
using BT.Domain.Shared.Entities;
using BT.SharedKernel.Dtos.Common;
using BT.SharedKernel.Features.Shared.EmailTemplates.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace BT.Application.Features.Shared.Notifications.Contracts.Interfaces;

public interface IEmailComposer<TEvent> where TEvent : IIntegrationEvent
{
    Task<AppResponse<ComposeEmailResponse>> ComposeAsync(TEvent evt, CancellationToken ct);
    Task<EmailTemplate?> ResolveTemplateAsync(EmailTemplateType emailTemplateType, CancellationToken cancellationToken);

}
