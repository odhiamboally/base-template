using System;
using System.Linq;

namespace BT.UI.Blazor.Features.Shared.Messaging;

internal static class UserMessageSanitizer
{
    public static bool IsDevelopment { get; set; }

    private static readonly string[] SensitiveFragments =
    [
        "System.",
        "Exception",
        "StackTrace",
        " at ",
        "Microsoft.",
        "SqlException",
        "DbUpdateException",
        "RequestFailedException",
        "CryptographicException",
        "AuthenticationException",
        "InvalidOperationException",
        "NullReferenceException",
        "ArgumentNullException",
        "AuthorizationPermissionMismatch",
        "key ring",
        "keyring",
        "dataprotection",
        "unprotect",
        "protect",
        "decrypt",
        "encrypt",
        "aka.ms",
        "license",
        "licensing",
        "mediatr",
        "masstransit",
        "fluentvalidation",
        "stack trace",
        "stacktrace",
        "hresult",
        "error code",
        "invalid operation",
        "database",
        "connection",
        "socket",
        "network"
    ];

    public static string Normalize(string? message, string fallback)
    {
        if (string.IsNullOrWhiteSpace(message)
            || string.Equals(message, "Operation Successful", StringComparison.OrdinalIgnoreCase))
        {
            return fallback;
        }

        if (LooksTechnical(message))
        {
            return IsDevelopment
                ? $"[DEV WARNING: Technical Error] {message.Trim()}"
                : fallback;
        }

        return message.Trim();
    }

    public static string? NormalizeNullable(string? message, string fallback)
        => string.IsNullOrWhiteSpace(message) ? null : Normalize(message, fallback);

    private static bool LooksTechnical(string message)
        => SensitiveFragments.Any(fragment => message.Contains(fragment, StringComparison.OrdinalIgnoreCase))
           || ContainsGuidPattern(message)
           || message.Contains("https://", StringComparison.OrdinalIgnoreCase);

    private static bool ContainsGuidPattern(string message)
    {
        if (string.IsNullOrWhiteSpace(message)) return false;

        int hyphensCount = 0;
        for (int i = 0; i < message.Length; i++)
        {
            if (message[i] == '-')
            {
                hyphensCount++;
            }
        }
        return hyphensCount >= 4;
    }
}
