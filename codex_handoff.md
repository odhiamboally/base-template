# Codex Handoff

## Task

Address PR security review findings without redoing completed integration work.

## Execution Rules

- Read this file before every sub-step.
- Update it immediately after each completed sub-step.
- Do not repeat completed analysis or verification.
- Run verification purposefully and stop after the required checks pass.
- If verification exposes stale assets, missing restores, transient tooling failures, or fixable code errors, perform the necessary restore or correction and retry.
- Do not repeat an unchanged failing command without first addressing its diagnosed cause.
- A task is not complete until the affected solution builds cleanly and the required checks pass.

## Completed

### 3. TOTP and bearer-token hardening

- Changed TOTP secret decryption failures to fail closed with a logged generic availability error.
- Removed all automatic 2FA disabling, secret deactivation/deletion, security-stamp mutation, and sign-out behavior from the cryptographic failure path.
- Removed the profile-picture `access_token` query-string fallback; authenticated file requests continue to use the Authorization header.

### 4. Password-reset transition-token backend

- Successful email OTP verification now issues an ASP.NET Core Identity password-reset transition token.
- Password reset requires that token for both email-OTP and email-link recovery modes.
- Removed the reusable boolean password-reset authorization marker from shared cache.
- Added the typed OTP verification response DTO and token validation guardrails.

### 1. Initial state and targeted discovery

- Branch: `codex/iam-ui-recovery-integration` tracking its matching origin branch.
- Worktree was clean at task start.
- Located the TOTP cryptographic failure path in `VerifyTotpCode.cs`; it currently disables 2FA and deletes/deactivates secrets, so the high-severity review finding is valid.
- Located `access_token` query handling in Infrastructure authentication configuration; it requires contextual inspection because SignalR WebSocket/SSE authentication commonly uses this mechanism.
- Located password-reset OTP verification; it currently writes a five-minute boolean cache marker and therefore lacks a single-use transition credential.
- No code changes have been made yet.

### 2. Security flow analysis

- TOTP decryption failures must fail closed without changing `TwoFactorEnabled`, stored secrets, security stamps, or sign-in state.
- The query-string bearer-token hook is scoped to the profile-picture endpoint, not SignalR. The current UI already fetches private image bytes with an authorization header, so the stale query-token hook can be removed.
- OTP verification currently creates a reusable five-minute boolean authorization marker. Replace it with an ASP.NET Identity password-reset token issued only after successful OTP verification.
- Keep the OTP transition token in a circuit-scoped Blazor state service and submit it in the reset request body; do not place it in a navigation URL.
- Email-link recovery continues to use the expected single-use Identity token from the emailed link.

## In Progress

- Prepare the completed security fixes for a focused commit and push them to the existing PR branch.

### 5. Password-reset transition-token UI

- Updated the UI auth contract and implementation to receive the typed OTP verification response.
- Added a circuit-scoped, consume-once password recovery state for the email and transition token.
- OTP recovery no longer places the email or transition token in the navigation URL.
- Reset password consumes the scoped state while preserving the existing email-link query flow.

## Pending

- None for this task.

### 6. Single verification result

- Ran exactly once: `dotnet build BaseTemplate.slnx --no-restore -p:UseSharedCompilation=false`.
- Result: failed before a reliable code-level verdict.
- Primary failure: the MAUI `project.assets.json` has no `net10.0` target because the solution was built with `--no-restore` against stale/incomplete assets.
- Secondary cascade: the generated `BT.UI.Rcl` reference assembly could not be read as managed metadata, producing broad downstream namespace/type errors in Blazor.
- No retry was made under the previous rule; that rule has now been replaced so necessary restores and evidence-driven retries are allowed.

### 7. Restore attempt

- `dotnet restore BaseTemplate.slnx` could not start because the managed sandbox denied MSBuild access to `C:\Users\PC\AppData\Local\Temp`.
- This is an execution-environment permission failure, not a NuGet, workload, or source-code failure.
- The next action is the same restore with elevated execution so MSBuild can use its normal temporary directory.

### 8. Solution restore

- Restored `BaseTemplate.slnx` successfully outside the managed sandbox.
- The MAUI project assets were refreshed; the remaining twelve projects were already current.

### 9. Post-restore build

- The restored solution build removed the prior RCL/Blazor cascade.
- API, Blazor, SharedKernel, Domain, Application, Infrastructure, Persistence, unit tests, integration tests, and architecture tests all compiled.
- The only remaining build error is `NETSDK1005` for `BT.UI.Maui`: the solution requests plain `net10.0`, but the MAUI assets do not contain that target.

### 10. MAUI target-framework diagnosis

- `Directory.Build.props` assigns `TargetFramework=net10.0` repository-wide.
- `BT.UI.Maui.csproj` separately declares Android, iOS, Mac Catalyst, and Windows through `TargetFrameworks`.
- Both properties remain populated after project evaluation, so MSBuild adds an invalid plain `net10.0` MAUI target.
- The focused correction is to clear the inherited single-target property in the MAUI project while preserving all four intended platform targets.

### 11. MAUI target-framework correction

- Added an empty project-local `TargetFramework` override to `BT.UI.Maui.csproj`.
- The MAUI project continues to target Android, iOS, Mac Catalyst, and Windows through `TargetFrameworks`; no repository-wide target convention was changed.

### 12. Corrected solution restore

- `dotnet restore BaseTemplate.slnx` completed successfully after the MAUI correction.
- MAUI assets were regenerated for the intended platform targets; the other twelve projects were already current.

### 13. Final verification

- Ran `dotnet build BaseTemplate.slnx --no-restore -p:UseSharedCompilation=false` after correcting and restoring the MAUI target graph.
- Result: succeeded with zero errors across API, Blazor, SharedKernel, Domain, Application, Infrastructure, Persistence, all three test projects, and MAUI Android/iOS/Mac Catalyst/Windows.
- One non-blocking existing analyzer warning remains in the MAUI Windows app suggesting that `App` can be internal (`CA1515`); no optimization sweep was started after the successful build.
- The task stop condition was reached.

### 14. Commit scope review

- Confirmed branch `codex/iam-ui-recovery-integration` contains only the three PR security fixes, their UI/shared contracts, the MAUI build-graph correction, and this handoff file.
- GitHub CLI could not read its roaming configuration inside the managed sandbox; retry the PR lookup outside the sandbox before publishing.
- Confirmed the branch is the head of open PR #38, `feat: complete IAM recovery and deployment integration`; these fixes belong in that same PR.

### 15. Required API build

- `dotnet build src\Backend\Api\BT.Api\BT.Api.csproj --no-restore -p:UseSharedCompilation=false` succeeded with zero warnings and zero errors.

### 16. Required architecture tests

- `dotnet test tests\BT.Tests.Architecture\BT.Tests.Architecture.csproj --no-restore -p:UseSharedCompilation=false` passed all 60 tests with zero failures and zero skips.

### 17. Pre-commit integrity check

- `git diff --check` passed with no whitespace errors.
- Final worktree scope remains limited to the intended security, recovery UI, MAUI build-graph, and handoff changes.

### 18. Publishing checkpoint

- All intended files were staged on `codex/iam-ui-recovery-integration`.
- The staged scope contains the three security review corrections, supporting shared/UI contracts, the MAUI target-graph correction, and this handoff record.
- Completed build and architecture verification must not be rerun before publishing to existing PR #38.
