using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json.Serialization;

using BT.Domain.Shared.Entities;

namespace BT.Domain.Features.IAM.Users.Entities;

public class AppUserDevice : BaseEntity
{
    [MaxLength(450)]
    public string AppUserId { get; set; } = string.Empty;
    public string DeviceFingerprint { get; set; } = string.Empty;
    public string DeviceName { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTimeOffset? LastUsedAt { get; set; }
    public bool IsTrusted { get; set; }
    public DateTimeOffset? TrustedUntil { get; set; }


    [JsonIgnore]
    public virtual AppUser AppUser { get; set; } = null!;
}
