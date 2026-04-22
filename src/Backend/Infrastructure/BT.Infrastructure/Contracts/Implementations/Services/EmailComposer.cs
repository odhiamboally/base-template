using BT.Application.Contracts.Interfaces.Common;
using BT.Application.Contracts.Interfaces.Services;
using BT.Application.Extensions;
using BT.Domain.Contracts.Interfaces.Common;
using BT.Domain.Events;
using BT.Infrastructure.Utilities;
using BT.SharedKernel.Dtos.Common;
using BT.SharedKernel.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using BT.Application.IntegrationEvents;
using BT.Application.Utilities;
using BT.Infrastructure.Logging;
using BT.Domain.Entities;

namespace BT.Infrastructure.Contracts.Implementations.Services;


internal sealed class EmailComposer(
    ICacheService _cache, 
    ISharedUnitOfWork _sharedUnitOfWork, 
    ILogger<EmailComposer> _logger) : IEmailComposer
{
    public bool CanHandle(Type eventType, EmailTemplateType template) =>
        _templateToEventMap.TryGetValue(template, out var mapped) && mapped == eventType;

    public async Task<AppResponse<ComposeEmailResponse>> ComposeClientCreatedAsync(CustomerCreatedIntegrationEvent evt, EmailTemplateType emailTemplate)
    {
        try
        {
            var template = await ResolveTemplateAsync(emailTemplate).ConfigureAwait(false);
            if (template is null)
                return AppResponse.Failure<ComposeEmailResponse>(
                    $"Email template '{emailTemplate.ToDisplayString()}' not found");

            var props = new Dictionary<string, string>
            {
                ["ClientId"] = evt.ClientId.ToString(),
                ["ClientNumber"] = evt.ClientNumber,
                ["ClientName"] = evt.ClientName,
                ["ClientType"] = evt.ClientType,
                ["Date"] = evt.OccurredAt.ToString("f", CultureInfo.InvariantCulture)
            };

            var body = EmailTemplateRenderer.Render(template.Body, props);

            return AppResponse.Success("Email composed successfully", new ComposeEmailResponse(
                template.Name, 
                evt.ClientName, 
                evt.Email, 
                template.Subject, 
                body));
            
        }
        catch (Exception ex)
        {
            ServiceLogDefinitions.LogEmailComposeError(_logger, emailTemplate.ToString(), ex);
            throw;
        }
    }

    public async Task<AppResponse<ComposeEmailResponse>> ComposeClientCreatedAsync(SendWelcomeEmailRequest req, EmailTemplateType emailTemplate)
    {
        try
        {
            var template = await ResolveTemplateAsync(emailTemplate).ConfigureAwait(false);
            if (template is null)
                return AppResponse.Failure<ComposeEmailResponse>(
                    $"Email template '{emailTemplate.ToDisplayString()}' not found");

            var props = new Dictionary<string, string>
            {
                ["ClientId"] = req.ClientId.ToString(),
                ["ClientNumber"] = req.ClientNumber,
                ["ClientName"] = req.ClientName,
                ["ClientType"] = req.ClientType,
                ["Date"] = DateTimeOffset.UtcNow.ToString("f", CultureInfo.InvariantCulture)
            };

            var body = EmailTemplateRenderer.Render(template.Body, props);

            return AppResponse.Success("Email composed successfully", new ComposeEmailResponse(
                template.Name,
                req.ClientName,
                req.ClientEmail,
                template.Subject,
                body));
        }
        catch (Exception ex)
        {
            ServiceLogDefinitions.LogEmailComposeError(_logger, emailTemplate.ToString(), ex);
            throw;
        }
    }

    public async Task<AppResponse<ComposeEmailResponse>> ComposeEmployeeCreatedAsync(EmployeeCreatedEvent evt, EmailTemplateType emailTemplate) 
    {
        try
        {
            var templateKey = CacheKeys.EmailTemplate(emailTemplate.ToDisplayString());

            var cachedTemplate = await _cache
                .GetAsync<EmailTemplate>(templateKey)
                .ConfigureAwait(false);

            var template = cachedTemplate ?? await _sharedUnitOfWork.EmailTemplateRepository
                .FindByCondition(t => t.Name == emailTemplate.ToDisplayString())
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);

            if (template == null)
            {
                return AppResponse.Failure<ComposeEmailResponse>($"Email template '{emailTemplate.ToDisplayString()}' not found");
            }

            var props = new Dictionary<string, string>
            {
                ["EmployeeId"] = evt.EmployeeId.ToString(),
                ["EmployeeNumber"] = evt.EmployeeNumber,
                ["EmployeeName"] = evt.EmployeeName,
                ["Email"] = evt.Email,
                ["Date"] = evt.OccurredAt.ToString("f", CultureInfo.InvariantCulture)
            };

            var body = EmailTemplateRenderer.Render(template.Body, props);

            var response = new ComposeEmailResponse(
                template.Name,
                evt.EmployeeName,
                evt.Email,
                template.Subject,
                body);

            await _cache.SetAsync(templateKey, template, TimeSpan.FromHours(1)).ConfigureAwait(false);

            return AppResponse.Success("Email composed successfully", response);
        }
        catch (Exception ex)
        {
            ServiceLogDefinitions.LogEmailComposeError(_logger, emailTemplate.ToString(), ex);
            throw;
        }
    }

    private static readonly Dictionary<EmailTemplateType, Type> _templateToEventMap = new()
    {
        [EmailTemplateType.ClientCreated] = typeof(CustomerCreatedEvent),
        [EmailTemplateType.EmployeeCreated] = typeof(EmployeeCreatedEvent),
        [EmailTemplateType.AppUserCreated] = typeof(AppUserCreatedEvent),
    };

    private async Task<EmailTemplate?> ResolveTemplateAsync(EmailTemplateType emailTemplate)
    {
        var key = CacheKeys.EmailTemplate(emailTemplate.ToDisplayString());
        var cached = await _cache.GetAsync<EmailTemplate>(key).ConfigureAwait(false);
        if (cached is not null) return cached;

        var template = await _sharedUnitOfWork.EmailTemplateRepository
            .FindByCondition(t => t.Name == emailTemplate.ToDisplayString())
            .FirstOrDefaultAsync()
            .ConfigureAwait(false);

        if (template is not null)
            await _cache.SetAsync(key, template, TimeSpan.FromHours(1)).ConfigureAwait(false);

        return template;
    }

}