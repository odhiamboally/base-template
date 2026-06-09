using System.ComponentModel.DataAnnotations;

namespace BT.Infrastructure.Configuration;

public sealed class DevelopmentSeedSettings
{
    public const string SectionName = "DevelopmentSeed";

    public bool Enabled { get; init; }

    [Required]
    public string AdminUserName { get; init; } = "aamodhiambo@gmail.com";

    [Required]
    [EmailAddress]
    public string AdminEmail { get; init; } = "aamodhiambo@gmail.com";

    [Required]
    public string AdminPassword { get; init; } = "Admin@12345";

    public Guid TenantId { get; init; } = new("0194f700-0000-7000-8000-000000000001");
}
