using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using System.Text.Json.Serialization;

using BT.Domain.Shared.Entities;

namespace BT.Domain.Features.IAM.Users.Entities;

public class RefreshToken : BaseEntity
{

    public string? AppUserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; set; }
    public string? CreatedByIp { get; set; }
    public DateTimeOffset? UsedAt { get; set; }
    public DateTimeOffset? RevokedAt { get; set; }
    public string? RevokedByIp { get; set; }
    public string? RevokedReason { get; set; }
    public string? ReplacedByToken { get; set; }
    public string? TokenFamily { get; set; } = Guid.CreateVersion7().ToString();



    // Methods

    [NotMapped]
    public bool IsActive => !IsRevoked && !IsExpired;

    [NotMapped]
    public bool IsUsed => UsedAt.HasValue;

    [NotMapped]
    public bool IsExpired => DateTimeOffset.UtcNow >= ExpiresAt;

    [NotMapped]
    public bool IsRevoked => RevokedAt.HasValue;


    // Navigation

    [JsonIgnore]
    public virtual AppUser AppUser { get; set; } = null!;
}
