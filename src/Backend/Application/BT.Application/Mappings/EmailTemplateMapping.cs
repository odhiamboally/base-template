using BT.Domain.Shared.Entities;
using BT.SharedKernel.Dtos.Common;
using BT.SharedKernel.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace BT.Application.Mappings;

public static class EmailTemplateMapping
{
    public static EmailTemplateResponse ToEmailTemplateResponse(this EmailTemplate entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new EmailTemplateResponse
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            Name = entity.Name,
            Subject = entity.Subject,
            Body = entity.Body
        };
    }

    public static EmailTemplateType ToEnum(this EmailTemplate entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        return entity.Name.ToTemplateEnum();
    }

    public static EmailTemplateType ToTemplateEnum(this string templateName)
    {
        ArgumentNullException.ThrowIfNull(templateName);

        return templateName switch
        {
            nameof(EmailTemplateType.AppUserCreated) => EmailTemplateType.AppUserCreated,
            nameof(EmailTemplateType.TenantWelcome) => EmailTemplateType.TenantWelcome,
            nameof(EmailTemplateType.TenantPasswordReset) => EmailTemplateType.TenantPasswordReset,
            nameof(EmailTemplateType.TenantNotification) => EmailTemplateType.TenantNotification,
            nameof(EmailTemplateType.ClientCreated) => EmailTemplateType.ClientCreated,
            nameof(EmailTemplateType.ClientApproved) => EmailTemplateType.ClientApproved,
            nameof(EmailTemplateType.ClientWelcome) => EmailTemplateType.ClientWelcome,
            nameof(EmailTemplateType.EmployeeCreated) => EmailTemplateType.EmployeeCreated,
            nameof(EmailTemplateType.EmployeApproved) => EmailTemplateType.EmployeApproved,
            nameof(EmailTemplateType.EmployeeWelcome) => EmailTemplateType.EmployeeWelcome,
            nameof(EmailTemplateType.PasswordResetRequest) => EmailTemplateType.PasswordResetRequest,
            nameof(EmailTemplateType.PasswordResetCode) => EmailTemplateType.PasswordResetCode,
            nameof(EmailTemplateType.PasswordResetSuccess) => EmailTemplateType.PasswordResetSuccess,
            nameof(EmailTemplateType.SecuritySettingsChanged) => EmailTemplateType.SecuritySettingsChanged,

            _ => throw new ArgumentException($"Unknown email template name: {templateName}")
        };
    }

    public static string ToTemplateName(this EmailTemplateType enumValue)
    {
        return enumValue switch
        {
            EmailTemplateType.AppUserCreated => nameof(EmailTemplateType.AppUserCreated),
            EmailTemplateType.TenantWelcome => nameof(EmailTemplateType.TenantWelcome),
            EmailTemplateType.TenantPasswordReset => nameof(EmailTemplateType.TenantPasswordReset),
            EmailTemplateType.TenantNotification => nameof(EmailTemplateType.TenantNotification),
            EmailTemplateType.ClientCreated => nameof(EmailTemplateType.ClientCreated),
            EmailTemplateType.ClientApproved => nameof(EmailTemplateType.ClientApproved),
            EmailTemplateType.ClientWelcome => nameof(EmailTemplateType.ClientWelcome),
            EmailTemplateType.EmployeeCreated => nameof(EmailTemplateType.EmployeeCreated),
            EmailTemplateType.EmployeApproved => nameof(EmailTemplateType.EmployeApproved),
            EmailTemplateType.EmployeeWelcome => nameof(EmailTemplateType.EmployeeWelcome),
            EmailTemplateType.PasswordResetRequest => nameof(EmailTemplateType.PasswordResetRequest),
            EmailTemplateType.PasswordResetCode => nameof(EmailTemplateType.PasswordResetCode),
            EmailTemplateType.PasswordResetSuccess => nameof(EmailTemplateType.PasswordResetSuccess),
            EmailTemplateType.SecuritySettingsChanged => nameof(EmailTemplateType.SecuritySettingsChanged),

            _ => throw new ArgumentException($"Undefined enum value: {enumValue}")
        };
    }
}
