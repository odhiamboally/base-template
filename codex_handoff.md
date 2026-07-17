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

### 19. Post-merge stash audit

- Synced local `main` to merged PR #38; current worktree was clean before this handoff update.
- Kept `stash@{0}` intact and did not apply or drop it.
- Compared stash-only UI additions with current `main`: the stash navigation is an older hardcoded menu, its profile image URL exposes `access_token` in the query string, and current session/profile implementations are newer. None of these stash UI changes should be restored.
- Continue by checking only stash-specific local-platform and authentication additions; do not repeat the completed UI comparison.
- Local-platform comparison completed: current `main` already has the stash's SQL credential validation and Azurite selection, plus newer randomized Azurite credentials, app-compose wiring, and fuller documentation. Restore nothing from the stash in this area.
- Continue only with the remaining backend/security stash additions; do not repeat UI or local-platform comparisons.
- Backend/security comparison completed: stash-only additions are the rejected query-string bearer token fallback, automatic MFA downgrade on decryption failure, and reusable cache-marker OTP reset flow. Current `main` has the safer replacements. Restore none of them.
- Final audit step: enumerate stash tracked/untracked paths once to ensure no file category was omitted; do not re-open already classified diffs.
- Full tracked/untracked stash inventory completed. Added-file categories are SaaS/deployment docs and assets, local app compose, OTLP configuration, password recovery, profile/UI support, and tests; these categories are present in merged `main`.
- Final existence-only check remains for stash-added paths. Do not compare implementations again.
- Final existence check passed: all 23 files added by the stash exist on current `main`.
- Audit conclusion: the stash contains no required template capability absent from `main`. Do not pop/apply it; it is safe to drop only after explicit user approval. No build/test was run because this was a read-only code-history audit.
- User approved deletion; dropped the audited stash `32e8ea944a2f869ce2c414448c36f9751087554c`. It was not applied to the worktree.

### 20. Azurite profile-picture timeout diagnosis

- New failure: after TOTP setup, profile-picture retrieval exhausts Azure SDK retries against `127.0.0.1:10000`; this makes the request slow and surfaces an aggregate connection exception.
- Confirmed local documentation intentionally selects Azurite for Visual Studio development; the error currently indicates the configured emulator is not listening.
- Existing unrelated user change detected in `src/Backend/Persistence/BT.Persistence/Common/DBContextHelper.cs`; do not modify or revert it.
- Next: inspect active Docker services, selected user-secret keys, storage registration, and profile-picture query/error flow. Do not repeat documentation review.
- Docker compose inspection completed: the local platform project currently has no running services. Azurite is therefore not listening on `127.0.0.1:10000`, which directly explains the retry exhaustion.
- Next: inspect selected profile-storage configuration and the profile-picture storage/query failure boundary once; do not repeat Docker diagnosis.
- The first root-level `dotnet user-secrets list --project ...` attempt could not load the API project. Retry from the API project directory is allowed and required; do not treat the selected keys as inspected yet.
- The API-directory retry failed with the same MSBuild project-load exit. Stop retrying user-secrets in this diagnosis; earlier runtime evidence already proves Azurite selection, and configuration can be inspected from code without exposing secret values.
- Next: inspect the profile storage implementation, query handler, settings, and DI registration together.
- Configuration/code search completed: JSON defaults are `Local`, production is `AzureBlob`, and DI intentionally maps both `Azurite` and `AzureBlob` to `AzureBlobProfilePictureStorage`. Runtime override to Azurite is therefore external configuration, consistent with the failed endpoint.
- `GetCurrentUserProfilePicture` currently calls storage directly with no availability translation, and the blob client uses Azure SDK default retry behavior. Inspect the exact implementation and existing logging definitions next; do not repeat broad searches.
- Exact code inspection completed. `OpenReadAsync` performs two blob calls (`ExistsAsync` then `DownloadStreamingAsync`) and uses the Azure SDK default six-attempt retry policy. Non-404 storage failures are not translated, so a stopped emulator yields the observed slow `AggregateException`.
- Existing logging only covers profile-picture updates, not reads. Next: inspect the profile controller response mapping once, then implement a single-call download, bounded retry policy, and sanitized/logged availability result.
- Controller inspection completed: profile content is an independent authenticated GET and failures go through `HandleResponse`; TOTP verification itself is not calling blob storage. The perceived TOTP delay occurs during post-verification UI/profile refresh.
- Next: inspect `AppResponses`/`HandleResponse` failure-code support once, then edit storage, query logging, and response translation.
- Response mapping inspection completed: `AppError.DependencyUnavailable(...)` maps cleanly to sanitized HTTP 503 through `BaseController`. This is the correct result for a configured but unreachable profile-image provider.
- Planned edit is fixed: use one blob download call with 404 handling, bound optional-image retry latency, translate provider transport failures, log the read outage through `LoggerMessage`, and return dependency-unavailable without affecting IAM/TOTP state.
- Event ID check completed: infrastructure service event `3492` is unused and reserved for the new profile-picture read availability log.
- Next: apply the three-file resilience edit once.
- Three-file resilience edit applied: blob reads now use one download request, 404 returns missing, retry latency is bounded to two retries, transport failures translate to `HttpRequestException`, and the query logs event 3492 before returning sanitized dependency-unavailable.
- Next: perform a compile-readiness inspection only (no build yet), then start the local platform and run the single allowed verification command.
- Compile-readiness diff inspection completed. Replace the framework `StatusCodes` reference in storage with provider status code `404` so the infrastructure adapter does not acquire an unnecessary ASP.NET HTTP dependency.
- Next: apply that one-line correction, start local infrastructure, then run exactly one backend build verification.
- One-line adapter correction applied; profile storage now handles Azure's numeric 404 without referencing ASP.NET HTTP constants.
- Next: start the documented local platform once so Azurite is available, then run the one allowed backend build.
- Local platform startup completed. Azurite is listening on ports `10000-10002`; RabbitMQ, Redis, and Mailpit are also running and healthy.
- Next: run the single allowed backend build verification. If it succeeds, stop without further checks or optimization.
- Backend build verification ran once and reached the changed Infrastructure code. It failed only on two diagnosed imports in `GetCurrentUserProfilePicture`: the logger definitions namespace is incorrect and `ILogger<T>` is not imported.
- The storage adapter changes compiled. Next: inspect the existing logger namespace/global usings once, correct only those imports, then retry the backend build under the allowed evidence-driven retry rule.
- Import inspection completed: `ServiceLogDefinitions` is in `BT.Infrastructure.Logging`, and Infrastructure has no global `Microsoft.Extensions.Logging` import.
- Next: replace the incorrect logger namespace import and add the explicit logging import in the query handler, then retry the backend build.
- Query-handler imports corrected to the existing `BT.Infrastructure.Logging` namespace with an explicit `Microsoft.Extensions.Logging` import.
- Next: retry the backend build once; stop immediately if it succeeds.
- Final backend build retry succeeded with zero warnings and zero errors. The task stop condition is reached; do not run additional checks or optimization sweeps.

### 21. Post-build Azurite 404 runtime check

- Confirmed Azurite is the `azurite` service inside the `cg-basetemplate` Compose project and is currently running on host ports `10000-10002`; it does not need a separate process when the Compose group is up.
- The reported debugger stack still calls `BlobBaseClient.ExistsAsync`, but current source and the successful build use a single `DownloadStreamingAsync` call with explicit 404 handling. Visual Studio is therefore debugging an older API process/assembly.
- A 404 from Azurite means the persisted profile-picture URI references a container/blob that is absent from the emulator. Current code treats this as a missing optional profile picture rather than an IAM/TOTP failure.
- No further code change or verification is required. Stop the old API debugging session and start it again so the newly built assembly is loaded.

### 22. Listing toolbar alignment standard

- New task: standardize listing toolbars so search/filter controls are left-aligned and shown-count badges plus create actions are right-aligned.
- Preserve existing unrelated changes, including `DBContextHelper.cs` and the completed profile-storage resilience work.
- The first targeted lookup combined the valid web project with guessed `Rcl`/`Shared` paths that do not exist, so it returned no usable implementation inventory. A narrow retry against the actual `src/Frontend` tree is allowed; do not repeat the invalid paths.
- Narrow frontend lookup completed. IAM roles, permissions, users, and user devices are separate Razor components under `Components/Pages/Admin`; the same shown-count pattern also exists in customers, employees, departments, settings, menus, and access catalog.
- IAM markup inspection completed. All four tabs use `.list-toolbar`, but place their create/count group before the search controls; users and permissions also include filter buttons with the search controls.
- `.list-toolbar` is defined centrally in `wwwroot/css/app.css`. The first combined CSS/inventory command failed because PowerShell stripped quoting from the `rg` regex; no inventory result was obtained. A literal-search retry is allowed.
- Central CSS and literal inventory completed. Nine admin listing components use `.list-toolbar`; its current three-column grid assumes child order instead of expressing search versus actions semantically, and collapses to one column below 800px.
- Non-IAM markup inspection completed. Customers, employees, departments, settings, and access catalog already put search/filter controls left while their page header or panel heading places create/count actions right.
- The defect is isolated to the four IAM tabs. Planned edit: add semantic search/action wrappers and a two-zone toolbar modifier to those tabs, keep filter buttons with search, place count before create in the right action group, and document the standard in `AGENTS.md`.
- Focused edit applied to roles, permissions, users, and user devices. Search/filter controls now occupy the left semantic group; shown-count and permitted create actions occupy the right semantic group. Shared CSS handles the two-zone responsive layout, and `AGENTS.md` records the convention.
- Next: perform one compile-readiness diff inspection only, correct only concrete issues found, then run the single frontend build verification.
- The broad diff inspection output was truncated and is not usable as verification. One narrow structural retry limited to the edited toolbar blocks and CSS is allowed; do not repeat the full diff.
- Narrow compile-readiness inspection completed successfully. All four IAM tabs have valid left search/filter and right count/create groups, and the shared responsive CSS supports the structure. No corrective edit was needed.
- Next: run the frontend build exactly once. Stop immediately if it passes; only an evidence-driven correction/retry is allowed if it fails.
- Verification complete: `dotnet build src\Frontend\Web\BT.UI.Blazor\BT.UI.Blazor.csproj --no-restore -p:UseSharedCompilation=false` passed with 0 warnings and 0 errors. Stop; do not run further checks or optimization sweeps for this task.

## 23. IAM listing hierarchy correction

- New correction from visual review: the previous two-zone single-row IAM toolbar does not match the Customer standard.
- Required hierarchy: create action in a top-right heading area, shown-count badge at the panel/list heading top-right, and search/filter controls on a separate full-width row below.
- Next: inspect the Customer page and IAM host composition narrowly, then implement the same visual hierarchy without changing unrelated listing behavior.
- The first combined Customer/IAM inspection was truncated and produced no usable markup evidence. A narrow exact-file lookup and read is allowed; do not repeat the broad command.
- Exact pages located: `AdminCustomers.razor` is the visual reference and `AdminIam.razor` is the IAM host; IAM tab bodies remain in their four existing child components.
- Next: read only the relevant opening markup from the reference, host, and child components to derive the smallest hierarchy change.
- Markup comparison completed. Customer uses three levels: hero-level create action, `panel-heading` with the count badge, then a full-width `list-toolbar` for search/filter controls. IAM child tabs currently compress all three concerns into one toolbar.
- Smallest safe correction: each IAM child gets a right-aligned action row (when it has a create action), a descriptive `panel-heading` with the count badge, and the existing search/filter controls in the standard `list-toolbar`; no data-loading or action behavior changes.
- Next: inspect only the existing shared CSS for `panel-heading`, `list-toolbar`, and obsolete IAM toolbar modifiers before editing.
- CSS inspection completed. `panel-heading` already provides the required split heading/badge alignment, and the base `list-toolbar` already matches the Customer search/filter row. Only a small reusable right-aligned page-action row is needed; the prior IAM-only toolbar modifiers can be removed.
- Next: edit the four IAM tab components, shared CSS, and the explicit frontend convention in `AGENTS.md`; preserve all data and action logic.
- Hierarchy correction applied to Roles, Permissions, Users, and User Devices. Create actions are now separate right-aligned rows, count badges are in descriptive list headings, and search/filter controls use the Customer-style full-width toolbar below.
- Removed the superseded IAM-only single-row toolbar CSS and updated `AGENTS.md` to make the Customer hierarchy the explicit listing-page standard.
- Next: perform one narrow compile-readiness inspection of only the edited opening blocks and CSS, then run the single frontend build verification.
- Narrow structural inspection found one concrete visual mismatch before verification: create actions were separated but still rendered inside the IAM panel, while the Customer standard places them in the page hero above the panel.
- Corrective plan: render the active tab's permitted create action from `AdminIam.razor` and delegate to a small public create-dialog entry point on the owning child component; remove the now-unneeded in-panel action row/CSS. Badge and search markup remains unchanged.
- Next: apply this focused correction, inspect only the new host-to-child wiring, then run the single frontend build.
- Handoff reread succeeded after the earlier oversized-output interruption. The only pending UI correction is moving active IAM create actions into the page hero while retaining each child tab's existing dialog implementation.
- Focused hierarchy correction completed: `AdminIam.razor` now owns the active tab's permission-gated create action in the page hero, while Roles, Permissions, and Users expose small public entry points that reuse their existing dialogs. The in-panel action rows and obsolete CSS were removed; list badges and search rows remain Customer-aligned.
- Next: inspect only the new host-to-child references and absence of the removed CSS, then run the one permitted frontend build.
- Narrow compile-readiness inspection passed: the hero actions are permission-gated by active tab, all three child references expose their create-dialog entry point, and `list-page-actions` no longer exists in the Blazor project.
- Next: run the frontend build exactly once. If it succeeds, stop without further inspection or optimization.
- Verification complete: the single `BT.UI.Blazor` build succeeded with 0 warnings and 0 errors. Stop condition reached; no additional checks or optimization were performed.

## 24. Session timeout dialog visual correction

- User screenshot confirmed the timeout warning remained overly wide and cramped; it was not included in the completed IAM listing hierarchy change.
- Located the implementation in `Components/Security/SessionLifecycle.razor`.
- Next: inspect only this component and its associated CSS, then apply a compact centered MudBlazor layout and run one frontend build.
- The first combined component/CSS inspection failed with no diagnostic output. A narrow retry of the files separately is required; no code changes were made.
- Narrow retry located the component-scoped stylesheet at `Components/Security/SessionLifecycle.razor.css`; `app.css` correctly contains no timeout-dialog styles. The component already uses MudBlazor primitives, so the correction can remain CSS/layout-focused.
- Next: read the scoped stylesheet, then adjust only the dialog markup/styles for a compact centered presentation.
- Root cause identified: component-scoped CSS targets classes rendered by MudBlazor child components without `::deep`, so the intended card width, typography, and button styling do not reach the generated DOM. The native full-screen host styling does apply, explaining the centered overlay but wide, cramped card.
- Next: add a native constrained dialog wrapper and use scoped `::deep` selectors for MudBlazor-rendered elements; preserve all session behavior.
- Timeout dialog correction applied. A native 27-rem responsive wrapper now constrains the centered card, MudBlazor-rendered elements use scoped `::deep` selectors, the copy is shorter, the visual hierarchy is clearer, and mobile remains centered. Session timing and action methods were not changed.
- Next: perform one narrow compile-readiness inspection of this component pair, then run exactly one frontend build.
- The first narrow inspection command failed because PowerShell interpreted the regex action-method alternation as a command. No code issue was identified; retry with literal searches only.
- Literal compile-readiness inspection passed: the native width-constraining wrapper is present, MudBlazor selectors cross the isolation boundary, and both existing action handlers remain wired.
- Next: run the frontend build exactly once. If successful, stop immediately.
- Frontend verification completed once. The tool truncated the verbose build output, but the build emitted a fresh `BT.UI.Blazor.dll` at 05:47:29 and no attached build remained to resume. No second build was launched. Stop condition reached.
- Commit/PR packaging started. The first combined Git/GitHub inspection was blocked because the restricted process could not read the GitHub CLI user configuration. No repository state was changed; retry the local Git inspection normally and the GitHub lookup with approved external access.
- GitHub lookup completed with approved access: `main` has no open pull request. Create a new `codex/` branch for this package. The combined command stopped on the expected no-PR exit before returning local diff details, so inspect the user-owned `DBContextHelper` diff separately before staging.
- Commit scope confirmed. Include the profile-image storage resilience/logging changes, IAM listing hierarchy, session timeout dialog, `AGENTS.md`, and this handoff. Exclude the user-owned one-word `DBContextHelper` comment edit. `app.css` has no content diff and requires no staging.
- Created and switched to `codex/storage-iam-ui-resilience` for this focused PR.
- Staged the intended profile-storage resilience, IAM listing hierarchy, timeout dialog, `AGENTS.md`, and handoff files. The user-owned `DBContextHelper` edit remains unstaged.
- Staging-only scope check passed: 12 intended files are staged. `DBContextHelper.cs` and the no-content-diff `app.css` remain unstaged and will not enter the commit. Existing successful backend/frontend verification is authoritative; do not rerun it.
- Created commit `450c90f` (`fix: harden profile storage and polish IAM UI`) on `codex/storage-iam-ui-resilience` and pushed it to `origin`.
- Opened PR #39, `Harden profile storage and polish IAM UI`: https://github.com/odhiamboally/base-template/pull/39. Final packaging step: commit this handoff update and push it to the same PR; do not rerun verification.
