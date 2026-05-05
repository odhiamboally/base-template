using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using System.Text.Json.Serialization;

using BT.Domain.Shared.Entities;

namespace BT.Domain.Features.IAM.Users.Entities;

public class RefreshToken : BaseEntity
{

    public string AppUserId { get; private set; } = string.Empty;
    public string Token { get; private set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; private set; }
    public string? CreatedByIp { get; private set; }
    public DateTimeOffset? UsedAt { get; private set; }
    public DateTimeOffset? RevokedAt { get; private set; }
    public string? RevokedByIp { get; private set; }
    public string? RevokedReason { get; private set; }
    public string? ReplacedByToken { get; private set; }
    public string? TokenFamily { get; private set; } = Guid.CreateVersion7().ToString();



    // Methods

    [NotMapped]
    public bool IsActive => !IsRevoked && !IsExpired;

    [NotMapped]
    public bool IsUsed => UsedAt.HasValue;

    [NotMapped]
    public bool IsExpired => DateTimeOffset.UtcNow >= ExpiresAt;

    [NotMapped]
    public bool IsRevoked => RevokedAt.HasValue;

    private RefreshToken() { }

    public static RefreshToken Create(
        string appUserId,
        string token,
        DateTimeOffset expiresAt,
        string createdBy,
        string? createdByIp = null,
        string? tokenFamily = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(appUserId);
        ArgumentException.ThrowIfNullOrWhiteSpace(token);
        ArgumentException.ThrowIfNullOrWhiteSpace(createdBy);

        return new RefreshToken
        {
            Id = Guid.CreateVersion7(),
            AppUserId = appUserId,
            Token = token,
            ExpiresAt = expiresAt,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = createdBy,
            CreatedByIp = createdByIp,
            TokenFamily = string.IsNullOrWhiteSpace(tokenFamily)
                ? Guid.CreateVersion7().ToString()
                : tokenFamily
        };
    }

    public void MarkAsUsed()
    {
        UsedAt = DateTimeOffset.UtcNow;
        SetUpdatedInfo(AppUserId);
    }

    public void Revoke(string reason, string? revokedByIp = null, string? replacedByToken = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);

        RevokedAt = DateTimeOffset.UtcNow;
        RevokedReason = reason;
        RevokedByIp = revokedByIp;
        ReplacedByToken = replacedByToken;
        SetUpdatedInfo(AppUserId);
    }

    // Navigation

    [JsonIgnore]
    public virtual AppUser AppUser { get; set; } = null!;
}
