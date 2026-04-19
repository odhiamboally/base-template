using BT.Domain.Contracts.Interfaces.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace BT.Domain.Entities;

public class AppUserProfile : BaseEntity, ISoftDeletable
{
    public string? AppUserId { get; set; }
    public string? TelephoneNo { get; set; }
    public string? MobileNo { get; set; }
    public string? Email { get; set; }
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    public void MarkAsDeleted(string deletedBy)
    {
        ArgumentNullException.ThrowIfNull(deletedBy);
        DeletedBy = deletedBy;
        DeletedAt = DateTimeOffset.UtcNow;
        IsDeleted = true;
    }
}
