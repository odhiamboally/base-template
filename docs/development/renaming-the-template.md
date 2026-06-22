# Renaming The Template

Rename a cloned repository before its first application run or deployment:

```powershell
cd E:\Repos\BaseTemplate
.\scripts\rename-template.ps1 -NewName Bumure -NamespacePrefix Bumure
```

Use `-WhatIf` first to preview changes. The script updates product-name variants in text files, optionally changes the `BT` namespace prefix, and renames matching files and folders. It skips Git metadata, generated build output, logs, dependencies, and `ops/local/.env`.

After the script runs:

1. Review URLs, email sender values, tenant seed data, Azure resource names, and deployment variables.
2. Restore and build the solution.
3. Configure local user-secrets or deployed environment variables.
4. Start the application only after the new identity is final.

## Data Protection Boundary

`DataProtection:ApplicationName` participates in cryptographic purpose chains. The rename script changes it safely only when used before protected data exists. Once cookies, TOTP secrets, trusted-device tokens, or other protected payloads have been issued, do not rename it casually. A later change requires a deliberate compatibility migration or security-state reset.

## Azure Resources

The script updates repository configuration tokens; it does not rename live Azure resources. Supply actual Key Vault, Storage, App Service, SQL, and Service Bus names through deployment variables and environment configuration.
