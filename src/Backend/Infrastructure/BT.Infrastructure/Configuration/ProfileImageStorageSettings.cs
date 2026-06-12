using System.ComponentModel.DataAnnotations;

namespace BT.Infrastructure.Configuration;

public sealed class ProfileImageStorageSettings
{
    public const string SectionName = "ProfileImageStorage";

    [Required]
    public string Provider { get; init; } = "Local";

    [Range(1, 10_485_760)]
    public long MaxBytes { get; init; } = 2_097_152;

    [Required]
    public string LocalRootPath { get; init; } = "uploads/profile-images";

    [Required]
    public string PublicBasePath { get; init; } = "/uploads/profile-images";

    public string[] AllowedContentTypes { get; init; } =
    [
        "image/jpeg",
        "image/png",
        "image/webp"
    ];
}
