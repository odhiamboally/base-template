using System;
using System.Collections.Generic;
using System.Text;

using BT.Domain.Shared.Entities;

namespace BT.Domain.Features.IAM.Users.Entities;

public class AppUserTotpSecret : BaseEntity
{
    public string AppUserId { get; set; } = string.Empty;
    public string EncryptedSecret { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTimeOffset? LastUsedAt { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }

    public virtual AppUser AppUser { get; set; } = null!;
}
