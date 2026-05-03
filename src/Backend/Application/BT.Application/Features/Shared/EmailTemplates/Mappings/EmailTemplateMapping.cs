using BT.SharedKernel.Extensions;
using BT.Domain.Shared.Entities;
using BT.SharedKernel.Dtos.Common;
using BT.SharedKernel.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace BT.Application.Features.Shared.EmailTemplates.Mappings;

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
        return entity.Name.ToEnum<EmailTemplateType>();
    }

    public static EmailTemplateType ToTemplateEnum(this string templateName)
    {
        ArgumentNullException.ThrowIfNull(templateName);

        return templateName.ToEnum<EmailTemplateType>();
    }

    public static string ToTemplateName(this EmailTemplateType enumValue)
    {
        return enumValue.ToDisplayString();
    }
}
