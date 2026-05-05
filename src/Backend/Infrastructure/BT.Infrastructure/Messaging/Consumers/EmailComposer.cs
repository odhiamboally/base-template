using BT.SharedKernel.Features.Shared.Notifications.Dtos;
using BT.Application.Contracts.Interfaces.Common;
using BT.Application.Features.IAM.Users.Contracts.Interfaces;
using BT.Application.Features.Shared.Notifications.Contracts.Interfaces;
using BT.SharedKernel.Extensions;
using BT.Application.Utilities;
using BT.Domain.Shared.Contracts;
using BT.Domain.Shared.Contracts.Common;
using BT.Domain.Shared.Entities;
using BT.Infrastructure.Logging;
using BT.Infrastructure.Utilities;
using BT.SharedKernel.Dtos.Common;
using BT.SharedKernel.Features.Shared.EmailTemplates.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace BT.Infrastructure.Messaging.Consumers;


public abstract class EmailComposer<TEvent>(
    ICacheService _cache,
    ISharedUnitOfWork _sharedUnitOfWork,
    ILogger<EmailComposer<TEvent>> logger) : IEmailComposer<TEvent> where TEvent : IIntegrationEvent
{
    private static readonly TimeSpan Ttl = TimeSpan.FromHours(1);

    public abstract Task<AppResponse<ComposeEmailResponse>> ComposeAsync(TEvent evt,CancellationToken ct);

    protected static ComposeEmailResponse ComposeFromTemplate(
        EmailTemplate template,
        string recipientName,
        string recipientEmail,
        IReadOnlyDictionary<string, string> tokens)
    {
        ArgumentNullException.ThrowIfNull(template);
        ArgumentException.ThrowIfNullOrWhiteSpace(recipientName);
        ArgumentException.ThrowIfNullOrWhiteSpace(recipientEmail);
        ArgumentNullException.ThrowIfNull(tokens);

        return new ComposeEmailResponse(
            template.Name,
            recipientName,
            recipientEmail,
            EmailTemplateRenderer.Render(template.Subject, tokens),
            EmailTemplateRenderer.Render(template.Body, tokens));
    }
        
    public async Task<EmailTemplate?> ResolveTemplateAsync(EmailTemplateType emailTemplateType, CancellationToken cancellationToken)
    {
        try
        {
            var key = CacheKeys.EmailTemplate(emailTemplateType.ToDisplayString());

            var cached = await _cache.GetAsync<EmailTemplate>(key, cancellationToken).ConfigureAwait(false);
            if (cached is not null) return cached;

            var template = await _sharedUnitOfWork.EmailTemplateRepository
                .FindByCondition(t => t.Name == emailTemplateType.ToDisplayString())
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);

            if (template is not null)
                await _cache.SetAsync(key, template, Ttl, cancellationToken).ConfigureAwait(false);

            return template;
        }
        catch (Exception ex)
        {
            ServiceLogDefinitions.LogEmailComposeError(logger, emailTemplateType.ToDisplayString(), ex);
            throw;
        }

    }

}
