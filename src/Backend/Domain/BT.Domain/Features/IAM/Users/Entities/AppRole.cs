using BT.Domain.Shared.Contracts.Common;
using Microsoft.AspNetCore.Identity;

namespace BT.Domain.Features.IAM.Users.Entities;

public sealed class AppRole : IdentityRole, ISoftDeletable
{
    public Guid? DepartmentId { get; set; }

    public bool IsDeleted { get; set; }

    public DateTimeOffset? DeletedAt { get; set; }

    public string? DeletedBy { get; set; }

    public void MarkAsDeleted(string deletedBy)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(deletedBy);

        IsDeleted = true;
        DeletedAt = DateTimeOffset.UtcNow;
        DeletedBy = deletedBy;
    }
}
