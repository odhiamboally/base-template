using BT.Domain.Features.Shared.TenantSettings.Entities;
using BT.SharedKernel.Features.Shared.TenantSettings.Dtos;
using System.Diagnostics.CodeAnalysis;

namespace BT.Application.Features.Shared.TenantSettings.Mappings;

public static class TenantSettingMapping
{
    public static TenantSettingResponse ToResponse(this TenantSetting entity)
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));

        return new TenantSettingResponse(
            entity.Id,
            entity.Key,
            IsSensitiveKey(entity.Key) ? "***" : entity.Value,
            entity.Description,
            entity.CreatedAt.UtcDateTime,
            entity.CreatedBy,
            entity.UpdatedAt?.UtcDateTime,
            entity.UpdatedBy
        );
    }

    public static bool IsSensitiveKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key)) return false;
        var lowerKey = key.ToLowerInvariant();
        return lowerKey.Contains("secret") || 
               lowerKey.Contains("password") || 
               lowerKey.Contains("token") || 
               lowerKey.Contains("key");
    }
}
