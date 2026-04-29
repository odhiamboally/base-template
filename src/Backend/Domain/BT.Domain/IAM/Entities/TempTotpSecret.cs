using BT.Domain.Contracts.Interfaces.Common;
using System;
using System.Collections.Generic;
using System.Text;

using BT.Domain.Shared.Entities;

namespace BT.Domain.IAM.Entities;

public class TempTotpSecret : BaseEntity, ISoftDeletable
{
    public string UserId { get; set; } = string.Empty;
    public string EncryptedSecret { get; set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; set; }

    public virtual AppUser User { get; set; } = null!;
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    public void MarkAsDeleted(string deletedBy)
    {
        ArgumentNullException.ThrowIfNull(deletedBy);
        IsDeleted = true;
        DeletedAt = DateTimeOffset.UtcNow;
        DeletedBy = deletedBy;
    }
}
