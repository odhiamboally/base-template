using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json.Serialization;

using BT.Domain.Shared.Entities;

namespace BT.Domain.Features.IAM.Users.Entities;

public class AppUserSession : BaseEntity
{
    public string AppUserId { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public DateTimeOffset LastAccessedAt { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? EndedAt { get; set; }
    public string? EndReason { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsRevoked { get; set; }

    [Required]
    [MaxLength(256)] // SHA-256 hash is 64 hex characters, so 256 is safe.
    public string DeviceFingerprint { get; set; } = string.Empty;

    // Navigation
    [JsonIgnore]
    public virtual AppUser AppUser { get; set; } = null!;
}
