# Configuration Code Conventions

This document defines how application configuration values should be consumed in code.

## Typed Mode And Provider Selection

Configuration values may remain strings in JSON, user-secrets, environment variables, and Key Vault because that is how .NET configuration works. Once those values enter code, parse mode/provider/transport values once into a typed enum and branch on the enum.

Do this:

```csharp
var mode = GetSomeMode(settings);

switch (mode)
{
    case SomeMode.Local:
        ConfigureLocal();
        break;

    case SomeMode.Azure:
        ConfigureAzure(settings);
        break;

    case SomeMode.Invalid:
    default:
        throw new InvalidOperationException(
            $"SomeSection:Mode '{settings.Mode}' is not supported. Supported values: Local, Azure.");
}
```

Avoid this:

```csharp
if (settings.Mode == "Azure" || settings.Mode == "Cloud")
{
    // ...
}
else
{
    // silently falls back to local
}
```

## Rules

- Parse string configuration modes in one helper such as `GetKeyEncryptionMode` or `GetMessagingTransport`.
- Use a small enum for each bounded configuration choice.
- Keep each configuration branch in a one-purpose method.
- Fail fast on unsupported values with a message that names the section key and supported values.
- Do not silently fall back to a default when a non-empty value is invalid.
- Defaults are allowed only when the value is missing or blank and the default is documented.
- Keep auto-resolution explicit. For example, `Auto` can prefer Key Vault, then certificate, then no extra encryption, but this priority must live in a named method.

## Current Examples

- `DataProtection:KeyEncryptionMode` is parsed into `KeyEncryptionMode`.
- `Messaging:Transport` is parsed into `MessagingTransport`.
- `ProfileImageStorage:Provider` is parsed into `ProfileImageStorageProvider`.
- `AuthProvider:Provider` is parsed into `AuthProvider`.

When adding a new configurable provider, transport, mode, or strategy, follow the same pattern and update this document if the convention changes.
