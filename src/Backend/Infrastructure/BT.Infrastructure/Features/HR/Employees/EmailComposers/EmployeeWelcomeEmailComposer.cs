using BT.SharedKernel.Features.Shared.Notifications.Dtos;
using BT.Application.Contracts.Interfaces.Common;
using BT.Application.Features.IAM.Users.Contracts.Interfaces;
using BT.Application.Features.Shared.Notifications.Contracts.Interfaces;
using BT.SharedKernel.Extensions;
using BT.Application.Features.Banking.Customers.IntegrationEvents;
using BT.Application.Features.HR.Employees.IntegrationEvents;
using BT.Application.Features.IAM.Users.IntegrationEvents;
using BT.Application.Utilities;
using BT.Domain.Features.Banking.Customers.Enums;
using BT.Domain.Shared.Contracts;
using BT.Domain.Shared.Entities;
using BT.Infrastructure.Logging;
using BT.Infrastructure.Messaging.Consumers;
using BT.Infrastructure.Utilities;
using BT.SharedKernel.Dtos.Common;
using BT.SharedKernel.Features.Shared.EmailTemplates.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace BT.Infrastructure.Features.HR.Employees.EmailComposers;


internal sealed class EmployeeWelcomeEmailComposer(
    ICacheService _cache,
    ISharedUnitOfWork _sharedUnitOfWork,
    ILogger<EmployeeWelcomeEmailComposer> _logger) : EmailComposer<EmployeeCreatedIntegrationEvent>(_cache, _sharedUnitOfWork, _logger)
{
    public override async Task<AppResponse<ComposeEmailResponse>> ComposeAsync(EmployeeCreatedIntegrationEvent evt, CancellationToken ct)
    {
        var templateType = EmailTemplateType.StandardWelcome;

        try
        {
            var template = await ResolveTemplateAsync(templateType, ct).ConfigureAwait(false);

            if (template == null)
                return AppResponse.Failure<ComposeEmailResponse>($"Template {templateType} not found");

            var tokens = new Dictionary<string, string>
            {
                ["EmployeeId"] = evt.EmployeeId.ToString(),
                ["EmployeeNumber"] = evt.Number,
                ["EmployeeName"] = evt.Name,
                ["Id"] = evt.EmployeeId.ToString(),
                ["Number"] = evt.Number,
                ["Name"] = evt.Name,
                ["Email"] = evt.Email,
                ["Date"] = evt.OccurredAt.ToString("f", CultureInfo.InvariantCulture)
            };

            return AppResponse.Success("Email composed", ComposeFromTemplate(
                template,
                evt.Name,
                evt.Email,
                tokens));
        }
        catch (Exception ex)
        {
            ServiceLogDefinitions.LogEmailComposeError(_logger, templateType.ToString(), ex);
            throw;
        }
    }

}
