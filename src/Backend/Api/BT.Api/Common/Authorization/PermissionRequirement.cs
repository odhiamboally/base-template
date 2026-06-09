using Microsoft.AspNetCore.Authorization;

namespace BT.Api.Common.Authorization;

internal sealed class PermissionRequirement(string permission) : IAuthorizationRequirement
{
    public string Permission { get; } = string.IsNullOrWhiteSpace(permission)
        ? throw new ArgumentException("Permission is required.", nameof(permission))
        : permission.Trim();
}
