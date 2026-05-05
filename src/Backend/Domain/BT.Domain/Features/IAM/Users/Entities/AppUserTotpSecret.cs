using System;
using System.Collections.Generic;
using System.Text;

using BT.Domain.Shared.Entities;

namespace BT.Domain.Features.IAM.Users.Entities;

public class AppUserTotpSecret : BaseEntity
{
    public string AppUserId { get; private set; } = string.Empty;
    public string EncryptedSecret { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;
    public DateTimeOffset? LastUsedAt { get; private set; }
    public DateTimeOffset? ExpiresAt { get; private set; }

    private AppUserTotpSecret() { }

    public static AppUserTotpSecret Create(
        string appUserId,
        string encryptedSecret,
        string createdBy,
        DateTimeOffset? expiresAt = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(appUserId);
        ArgumentException.ThrowIfNullOrWhiteSpace(encryptedSecret);
        ArgumentException.ThrowIfNullOrWhiteSpace(createdBy);

        return new AppUserTotpSecret
        {
            Id = Guid.CreateVersion7(),
            AppUserId = appUserId,
            EncryptedSecret = encryptedSecret,
            IsActive = true,
            ExpiresAt = expiresAt,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = createdBy
        };
    }

    public void MarkAsUsed()
    {
        LastUsedAt = DateTimeOffset.UtcNow;
        SetUpdatedInfo(AppUserId);
    }

    public void Deactivate()
    {
        IsActive = false;
        SetUpdatedInfo(AppUserId);
    }

    public virtual AppUser AppUser { get; set; } = null!;
}
