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

## 25. Reusable payment feature showcase audit

- User requested a read-only assessment of the existing Stripe/M-Pesa capability and a DB-backed sidebar/page approach for exercising reusable platform features from the UI.
- The initial broad payment/navigation search exceeded the useful output budget; no code changes were made and its search must not be repeated.
- Previous PR packaging is complete and unrelated to this audit. Next: inspect the focused payment file inventory, API endpoints, frontend pages, and DB-backed menu loading/registration flow with narrow queries.
- Focused inventory completed. Backend already contains provider-neutral payment commands/status queries, persisted `PaymentRecord`, Stripe Checkout/webhook support, M-Pesa STK/C2B operations, and provider routing. Frontend contains `IPaymentCheckoutService`, its Blazor implementation, and Stripe success/cancel pages, but no payment initiation/showcase page.
- DB-backed navigation uses `MenuItem` plus seed/config/query infrastructure; the exact route/menu authorization behavior still needs focused inspection before recommending records and hierarchy.
- Next: read the payment controller/orchestrator/UI service and navigation query/seed/NavMenu to establish the implemented flow, missing UI contracts, and correct DB-backed menu design.
- Flow inspection completed. `PaymentsController` exposes permission-protected checkout/status/admin operations and anonymous Stripe/M-Pesa callbacks; `RoutedPaymentGateway` selects Stripe, M-Pesa, NoOp, or unsupported adapters per request; initiation persists `PaymentRecord` before provider I/O. The Blazor checkout service can call checkout/status but has no consuming initiation page.
- Sidebar navigation loads active, permission-filtered menu trees from the database after authentication. Its three-item static fallback is a resilience shell only. `MenuItemSeed` already contains a top-level `Billing` route at `/billing`, but no matching Blazor page exists, so the current menu seed and executable UI are inconsistent.
- Next: inspect payment DTOs/settings, route catalog, permission seeds, and payment persistence shape to decide exactly what belongs in DB, typed code/config, and the reusable showcase UI.
- Persistence/config inspection completed. `PaymentRecord` is tenant-scoped, audited, soft-deletable, and persists amount/currency, provider, generated customer reference, provider reference, and status. Provider secrets/endpoints correctly remain typed configuration, not database data.
- The controller requires `payments.create`, `payments.view`, and `payments.admin`, but no matching payment permission was found in the current persistence/domain seed search. This is a concrete catalog gap that must be addressed with the UI/menu work.
- Next: inspect exact shared DTO fields, permission/reference seeds, menu route validation, and success/cancel page behavior; then produce a discussion-ready implementation recommendation without editing product code.
- The first exact-file DTO read used an incorrect guessed `BT.SharedKernel` path and failed before yielding a usable combined result. No code was changed. A filename-only lookup for the DTOs and separate narrow reads of the already-known success/cancel and catalog files are allowed; do not repeat the guessed path or broad payment scan.
- Filename-only lookup succeeded. The payment DTOs are under `BT.SharedKernel/Features/Shared/Payments/Dtos`; the exact success/cancel pages, permission seed, and `MenuRoute` entity are now located. Next: read only these six files, then close the audit without further repository scanning.
- Final focused read completed. The shared initiation contract supports amount, currency, description, callback URL, optional payer phone, and optional provider; the response supplies provider, payment reference, checkout URL, and status. This is sufficient for one provider-neutral showcase form, with provider-specific UI sections driven by the selected provider.
- Stripe success/cancel pages exist, but cancel links to `/payments/card`, which has no page. Together with the dead seeded `/billing` route, this confirms the frontend showcase/navigation is incomplete rather than absent backend capability.
- `PermissionSeed` definitively lacks the controller-required `payments.view`, `payments.create`, and `payments.admin` entries. `MenuRoute` is DB-backed route metadata, not executable routing; only approved routes implemented in Blazor should be seeded/managed.
- Audit recommendation ready: replace the dead Billing entry with a DB-backed `Features` parent and implemented `Payments` child; add payment permission/catalog seeds and admin assignment; add a provider-capabilities endpoint; implement Stripe Checkout, M-Pesa STK, status lookup, and paged payment history UI; keep credentials/endpoints/provider wire DTOs in typed configuration/code, not the database. No product code or verification was performed for this read-only audit.

## 26. Reusable payment feature showcase implementation

- User approved implementing the complete audited payment showcase as a focused vertical slice and requested a dedicated local and remote branch.
- The first combined Git/plan/document read exceeded the useful output budget and produced no reliable state evidence. No code changed. Continue with narrow sequential reads only; do not repeat the combined command.
- Handoff recovery completed. The implementation scope is fixed: branch isolation, provider capabilities and payment history APIs, permission and DB-backed menu alignment, Stripe/M-Pesa showcase UI including admin C2B tools, focused tests, migrations, and one final verification run.
- Next: inspect only current Git branch/status and GitHub PR state, then create or reuse a focused `codex/` branch without staging the known user-owned `DBContextHelper` edit.
- Git state inspected. The workspace remains on `codex/storage-iam-ui-resilience`; PR #40 for that branch is merged. Two local modifications remain: this handoff and `AzureBlobProfilePictureStorage.cs`.
- The payment work must therefore use a new focused branch based on current `main`, not the merged branch. Before switching, inspect the remaining storage diff to determine whether it is an uncommitted user change that must be preserved.
- The remaining storage diff is a valid, unrelated resilience adjustment that broadens transient-failure translation to `OperationCanceledException` and uses the imported `IOException`. Preserve it as an unstaged user-owned change; do not include it in the payment commit.
- Next: stash both local changes temporarily, update `main`, create and publish a dedicated `codex/payment-feature-showcase` branch, then restore the stash and keep the storage file excluded.
- Branch isolation completed. Updated `main` to merged PR #40, created `codex/payment-feature-showcase`, restored the handoff and unrelated storage edit, and published the branch to `origin` (the push command timed out after remote tracking was established, and the subsequent local upstream check confirmed the remote branch exists).
- `AzureBlobProfilePictureStorage.cs` remains an unstaged user-owned change and must stay out of the payment commit.
- Next: read the phase/configuration documents required by `AGENTS.md`, then inspect only the existing payment and seed patterns needed for implementation.
- The first combined convention-document read was truncated and yielded no reliable guidance. Do not repeat it. Narrow literal searches against each required document are allowed before implementation.
- Next: read only payment-relevant feature-folder, UI-to-backend, and typed-provider configuration excerpts, updating this handoff after each successful read.
- Feature-folder convention read completed. Payments correctly remains a `Shared/Payments` reusable capability across layers; shared transport DTOs stay in `SharedKernel/Features/Shared/Payments`, provider adapters remain feature-owned, persistence artifacts remain under the same Shared feature path, and Shared UoW coordinates writes.
- Next: read only the UI-to-backend flow rules relevant to a DB-backed payment showcase.
- UI-to-backend flow read completed. The showcase must use a typed Blazor HTTP client over the real API boundary, keep shared contracts in `BT.SharedKernel`, retain presentation state in UI, and leave payment rules/orchestration in Application/Domain.
- Next: read only typed provider/configuration rules, then inspect the existing payment slice file inventory.
- The broad keyword scan of `docs/development/configuration-code-conventions.md` overflowed and produced no reliable result. Do not repeat broad reads or broad `Select-String` scans of this file; inspect only headings and small targeted line ranges.
- Next: complete one narrow configuration-convention read, record it, then build a concrete implementation plan from the existing payment files.
- Targeted configuration-convention read completed. Provider/mode strings must be parsed once into typed enums, per-operation payment provider selection must use the existing router/factory, provider-specific wire DTOs stay beside adapters, and unsupported values fail fast. Secrets/endpoints remain typed configuration rather than DB records.
- Next: inspect the exact existing payment controller/contracts/handlers/persistence, permission/menu seeds, and Blazor service/page patterns needed for implementation.
- Exact payment inspection confirmed the existing controller/router/UoW flow, provider-neutral DTOs, and tenant-scoped `PaymentRecord`. Missing backend capabilities are provider metadata, paged local history, and idempotent initiation. Permission and menu seeds still lack the payment permissions and an implemented Features/Payments route.
- The first role-assignment search used two nonexistent guessed IAM subdirectories and failed after returning the valid seed/config inventory. Do not repeat those paths; inspect `AppRoleConfiguration`, IAM seeding services, and reference data directly.
- Next: locate existing role-permission seeding and paging/specification patterns, then finalize the concrete edit set.
- A combined role/paging/specification inspection overflowed and produced no reliable implementation evidence. Do not repeat it. Continue with one exact file or narrowly located interface at a time.
- Next: inspect `AppRoleConfiguration.cs`, then locate the existing role-permission seeder and generic repository/specification interfaces with filename-only queries.
- `AppRoleConfiguration` contains only role table/property/index mapping and no permission assignment seed. The development identity seeder has no direct `PermissionSeed`, role-claim, or permission assignment references.
- Filename lookup located the canonical generic repository and specification contracts under `BT.Domain/Shared/Contracts`; use those exact files for the history design.
- Next: inspect `IRepository.cs` and `Specification.cs`, then locate the permission-role assignment source by exact symbol search in Persistence and Domain only.
- Generic persistence supports composed `ListAsync` projection/filter/order/paging, expression-based counts/existence checks, and cursor specifications. Payment history can therefore use the existing Shared UoW repository without a new concrete repository method.
- Next: locate exact permission assignment entities/configuration and payment UI/API endpoint constants, then begin edits.
- Permission/reference catalogs are EF-seeded from `PermissionSeed` and `IamReferenceDataSeed`; both need Shared/payments entries plus approved `/features` and `/features/payments` routes. The typed Blazor payment service currently exposes only checkout and provider status.
- Next: inspect the exact payment file inventory, menu seed, UoW contract, and endpoint settings once, then apply the backend/domain/catalog edit set.
- The combined UoW/MenuItemSeed/BackendApiSettings read overflowed and produced no reliable implementation evidence. Do not repeat that combined command; read each exact file separately.
- Next: read `ISharedUnitOfWork.cs` only, record it, then continue with `MenuItemSeed.cs` and the payment endpoint settings separately.
- The guessed Application path for `ISharedUnitOfWork.cs` does not exist. The handoff reread succeeded, but no UoW content was obtained. Do not retry that path; use a filename-only lookup.
- Next: locate `ISharedUnitOfWork.cs` by filename, then read only the returned file.
- Located the Shared UoW contract at `src/Backend/Domain/BT.Domain/Features/Shared/Contracts/ISharedUnitOfWork.cs`.
- Next: read that exact contract only.
- Shared UoW inspection completed. `PaymentRecordRepository` is already exposed and the unit supports both normal transactional completion and integration-event completion, so no new persistence abstraction is needed for history or idempotency.
- Next: locate and read `MenuItemSeed.cs` only to preserve its exact IDs/hierarchy conventions.
- Located `MenuItemSeed.cs` at `src/Backend/Persistence/BT.Persistence/Features/IAM/Menus/Seeds/MenuItemSeed.cs`.
- Next: read that exact seed file only.
- Menu seed inspection completed. The dead top-level `Billing` item uses stable ID `...10401`; it can be safely repurposed as the `Features` parent while a new stable child ID is added for `Payments`, preserving deterministic seed behavior.
- Next: locate `BackendApiSettings.cs`, then read only its payment endpoint members.
- Located the typed Blazor API settings at `src/Frontend/Web/BT.UI.Blazor/Configuration/BackendApiSettings.cs`.
- Next: read only the payment-related declarations with narrow context.
- Endpoint settings inspection completed. The Blazor client currently defines only checkout and status routes; capabilities, history, and C2B administration routes must be added to the existing typed `PaymentEndpointSettings` section.
- Exact pre-edit inspection is complete. Next: enumerate only `Features/Shared/Payments` files and apply the backend/domain/catalog implementation in focused batches.
- Payment inventory enumeration completed. The slice already has isolated Stripe/M-Pesa adapters, provider routing, callbacks, C2B commands, status query, persistence, and a typed Blazor service. The implementation will extend these patterns rather than duplicate them.
- Next: read the exact controller, initiation handler, entity/configuration, and Blazor service contracts needed for the first edit batch.
- Core flow inspection completed. `PaymentRecord` is tracked across provider initiation, the controller already has permission-gated C2B endpoints, and the Blazor abstraction is ready to extend. Idempotency requires persisted key/checkout URL; history can project from the existing repository.
- Next: inspect exact Stripe/M-Pesa settings and C2B command shapes, then implement shared DTOs, provider catalog, history, idempotency, and controller endpoints.
- Located typed payment settings in Infrastructure. C2B registration takes no body; sandbox simulation requires amount, phone, and bill reference.
- Next: read the three settings POCOs and the payment DI registration block only.
### Payment showcase continuation note

- The previous combined read of payment settings and DI registration exceeded the tool output limit, so it produced no reliable evidence and must not be treated as completed.
- Read `PaymentSettings`, `StripePaymentSettings`, `MpesaPaymentSettings`, and the payment DI registration separately before editing.
- `PaymentSettings` read completed. The section is `Payments`, with a default provider, production NoOp guard, and nested Stripe/M-Pesa settings. Provider capability reporting can derive from this single typed options object.
- Next: read `StripePaymentSettings.cs` only.
- `StripePaymentSettings` read completed. Stripe readiness is determined by `SecretKey`, checkout endpoint, webhook secret, and success/cancel URLs; the capability response must expose readiness without exposing any of these values.
- Next: read `MpesaPaymentSettings.cs` only.
- `MpesaPaymentSettings` read completed. M-Pesa capability metadata can safely report sandbox/live environment and supported STK/C2B operations while readiness is derived from credentials, shortcode/passkey, and callback base URL. Provider endpoints remain internal typed configuration.
- Next: locate and read only the payment registrations in Infrastructure DI.
- Payment DI inspection completed. The existing registrations bind/validate `PaymentSettings`, register the routed gateway plus all provider adapters/C2B/webhook services, and configure named Stripe/M-Pesa clients. The new provider catalog only needs one scoped registration and should reuse `IOptions<PaymentSettings>`.
- Pre-edit discovery is now complete. Next: implement shared contracts and backend query/catalog/idempotency changes.
- Shared payment contracts implemented: provider capability metadata, paged history response/items, and an optional initiation idempotency key. Each public type is in its own correctly named file.
- Next: extend the payment aggregate and EF mapping for persisted idempotency and checkout continuation data.
- Payment aggregate/EF mapping completed after one path/signature correction: records now persist a normalized optional idempotency key and checkout URL, with a tenant-scoped filtered unique idempotency index. No provider secret or wire payload was added to persistence.
- Next: add provider catalog and payment history application contracts/queries, then expose them through the controller.
- Existing application pattern inspection completed. Payment requests use `AppResponse<T>` MediatR handlers under `CommandHandlers`/`QueryHandlers`; initiation currently persists before provider I/O and status delegates to the routed gateway.
- Next: use the already-approved generic repository projection/paging methods to implement capabilities, history, and idempotent initiation.
- A prior attempt to read the handoff tail returned an unrelated oversized-output error and yielded no usable evidence. This retry succeeded; no implementation work was lost or repeated.
- Next: read only the generic repository and `AppResponse<T>` factory signatures needed by the payment handlers.
- The assumed `IRepository.cs` and `AppResponse.cs` paths did not exist. No signatures were read and no code was changed; locate the authoritative files by symbol before implementing handlers.
- Authoritative signatures confirmed in `Domain/Shared/Contracts/Repositories/IRepository.cs` and `SharedKernel/Dtos/Common/AppResponse*.cs`: database-side projection, count, first-result, paging composition, and established success/failure factories are available.
- Next: implement provider capability catalog, payment history query, and idempotent initiation using the shared Unit of Work and generic repository.
- Payment feature inventory confirmed the existing bounded-context `IPaymentRecordRepository` is exposed through `ISharedUnitOfWork`; no new repository abstraction is needed. A read targeted the wrong filename (`InitiatePayment.cs`); the authoritative file is `InitiatePaymentHandler.cs`.
- Next: read the exact initiation handler, controller, settings, and payment registration contexts, then implement the backend slice.
- Exact backend surfaces confirmed. `PaymentsController` already applies `payments.create`, `payments.view`, and `payments.admin`; initiation persists before provider I/O but does not yet save checkout continuation data or handle idempotency.
- Next: add the capability/history application slice and harden initiation idempotency/continuation persistence.

### 23. Payment showcase backend continuation

- Reconciled the interrupted payment patch against the working tree. It completed successfully: provider capability and payment-history query contracts/handlers, provider catalog registration, controller read endpoints, and initiation idempotency/checkout persistence are present.
- Preserved the unrelated `AzureBlobProfilePictureStorage.cs` modification without inspection or alteration.
- Next: inspect the completed backend payment diff for narrow correctness issues, then update payment permissions/menu seeds and create the corresponding migration.
- Backend payment diff inspection completed. Capability metadata derives from typed settings without exposing secrets; history composes count/order/page queries before materialization; and initiation returns a prior matching record for the same tenant-scoped idempotency key while persisting the checkout continuation.
- Next: inspect exact permission, reference-catalog, and menu seed files to add the DB-backed Features/Payments navigation and payment permissions.
- A combined seed-file read exceeded the tool output limit, so no seed source was treated as inspected or changed. Next: read the permission, menu, and route/reference seed sources separately before making the catalog patch.
- Permission seed inspected separately. It uses deterministic IDs, a platform-default tenant, and a `Create` helper; payment permissions can be added as `Shared/payments` `view`, `create`, and `admin` entries.
- Menu seed inspected separately. The existing top-level Billing seed has a stable ID and can be repurposed as the top-level `Features` parent; the Payments child will use its own stable ID and `payments.view` visibility requirement.
- Route/reference seed location and usage confirmed. Next: read `IamReferenceDataSeed.cs` directly to add the approved `/features` and `/features/payments` routes and any missing reference keys.
- Reference seed inspected separately. It needs the `Shared` context, `payments` resource, `admin` action, `CreditCard` icon, and approved Features/Payments routes to match the proposed menu and permission model.
- IAM catalog seeds updated: Shared/payments permissions, Shared context, payments resource, admin action, credit-card icon, approved Features/Payments routes, and a permission-gated Payments sidebar child were added. The former Billing parent now consistently represents Features.
- The narrow Persistence filename search for role-permission seed files returned no matches. No role assignment source was treated as inspected and no code changed.
- Search by the `System Administrator` role value found no Persistence seed source; assignments appear to be handled through IAM authorization/runtime configuration instead. No assignment code changed.
- Permission constants inspection confirmed dynamic permission policies use the `permission` claim type and explicitly recognize the `System Administrator` role. The authorization handler is the remaining source to inspect before deciding whether any role-permission mapping is necessary.
- Permission authorization inspection completed. The established administrator bypass succeeds every dynamic permission requirement for the `System Administrator` role, while other roles require matching `permission` claims. No role-permission seed change is needed for the payment showcase.
- Migration conventions confirmed: the contexts are `SharedDBContext` and `IamDBContext`, with generated migrations under `Migrations/Shared` and `Migrations/IAM` respectively.
- Design-time factories are provider-specific and use `SharedSqlServerDBContextFactory`/`IamSqlServerDBContextFactory` for the local SQL Server path. The concrete factory setup remains to be read before generating migrations.
- SQL Server design-time factories confirmed. EF migrations must target `SharedSqlServerDBContext` using `SharedConnection` and `IamSqlServerDBContext` using `IamConnection`, not their provider-neutral base contexts.
- Shared SQL Server migration `AddPaymentFeatureShowcase` generated successfully. EF emitted an existing namespace advisory because both provider contexts use the same migration folder; the repository's existing migration layout was preserved.
- IAM SQL Server migration `AddPaymentFeatureCatalog` generated successfully. It produced the same known namespace advisory from the repository's existing multi-context migration layout; no migration generation failure occurred.
- Frontend inventory confirmed an existing provider-neutral `PaymentCheckoutService`, Payment success/cancel pages, and shared `BackendApiClient`; the new showcase can extend these rather than introducing a parallel client pattern.
- Existing UI conventions confirmed: the payment service already routes through `IBackendApiClient`; page components use InteractiveServer, MudBlazor, protected session permissions, compact panel headers, right-aligned actions/count chips, responsive toolbars, and horizontally scrollable dense tables.
- A filename-only frontend search for payment contracts/settings returned no matches because its combined pattern was too restrictive. No source was treated as inspected and no code changed.
- Payment contract/registration symbols are located in the RCL interface, Blazor `Program.cs`, and the existing `Billing.razor` page. No endpoint settings source has yet been inspected.
- The first broad `BackendApiSettings.cs` read exceeded the output limit. It is not treated as a completed settings inspection and no frontend source was changed.
- Payment endpoint settings inspection completed. The existing typed section defines checkout and provider-status routes only; it needs capability/history and C2B administration endpoints added alongside those existing routes.
- Payment service interface inspection completed. It is provider-neutral and currently exposes checkout and status methods only, so capabilities/history/C2B operations should extend this interface rather than introduce another frontend client abstraction.
- Payment service implementation inspection completed. It consistently uses `IBackendApiClient`, `EndpointFormatter`, typed endpoint settings, and sanitized availability/timeout messages. New calls must preserve this pattern.
- Payment registration inspection completed. `IPaymentCheckoutService` is already registered as scoped in the Blazor host, so extending its interface and implementation requires no DI shape change.
- Existing `Billing.razor` inspection completed. It is an older authenticated one-time checkout form with hardcoded provider choices and no capability, readiness, history, permission, or C2B-administration support. Replace its route with a DB-backed Features/Payments showcase rather than retaining a competing Billing surface.
- Next: inspect payment controller request types and route shapes for the C2B operations, then implement the frontend contract and showcase page.

### 24. Payment showcase frontend continuation

- Re-read the handoff before resuming frontend implementation. The backend capability/history/idempotency work, IAM catalog seeds, and both migrations remain complete; the unrelated Azure profile-storage file remains excluded.
- Next: inspect the exact C2B controller route and request shapes with a narrow symbol search, then extend the existing typed payment client and implement the showcase UI.
- Payment controller route inspection completed. The existing API exposes permission-gated `GET capabilities`, `GET history`, `POST checkout`, `POST mobile-money/admin/register-c2b-urls`, and `POST mobile-money/admin/simulate-c2b`; simulation accepts `SimulateMpesaC2BPaymentCommand`.
- `SimulateMpesaC2BPaymentCommand` inspection completed. The API body shape is `Amount`, `PhoneNumber`, and `BillRefNumber`, returning `AppResponse<string>`.
- Razor route inspection completed. The legacy payment flow is rooted at `Components/Pages/Common/Billing.razor` with `/billing`; no `/features` route exists yet.
- Existing payment client and legacy Billing page were re-read. The client uses the expected typed backend boundary; Billing is the only legacy checkout route and will become a compatibility redirect. The initial endpoint-settings path guess was wrong, so no settings file was treated as re-inspected.
- Authoritative endpoint settings and permission-check conventions were located. `PaymentEndpointSettings` is nested in `Configuration/BackendApiSettings.cs`; UI pages use `IAuthSession.HasPermission(...)` for action visibility.
- Payment DTO shapes confirmed: capability metadata contains enabled/configured/readiness flags; history is paged; checkout returns provider/reference/URL/status.
- Next: apply the frontend contract and showcase page in one focused edit, retaining `/billing` as a compatibility redirect.

### 25. Deployment alignment audit

- Current deployment worktree inspected read-only. Branch `ops/azure-sandbox-deployment` is clean and matches `origin/ops/azure-sandbox-deployment` at `8ea9cfd` (`Configure AzureServiceBus instead of disabling Messaging for Production`).
- The branch contains deployment workflows (`deploy-azure.yml`, `provision-azure.yml`, `non-azure-deploy.yml`), Docker publishing, and Bicep modules for App Service, Container Apps, Key Vault, Storage, SQL Server, PostgreSQL, and Service Bus. It also includes the previously interrupted payment showcase and SaaS control-plane work.
- Deployment workflow, Bicep parameter, runtime configuration, and planning-document alignment inspection completed read-only. `deploy-azure.yml` builds/tests, creates migration bundles, supports App Service plus ACA through ACR or GHCR, and applies migrations using short-lived GitHub-runner SQL firewall access that it revokes afterwards. `provision-azure.yml` uses OIDC and provisions App Service or Container Apps with SQL Server or PostgreSQL.
- The target terminology is intentionally split: provisioning uses infrastructure target `container-apps`; deployment uses registry-specific targets `aca-acr` and `aca-ghcr`. This needs a short documentation mapping, not a code change.
- Concrete deployment-stamp blockers identified: App Service Bicep still selects `DOTNETCORE|8.0` although this repo targets .NET 10; API runtime settings enable Azure Service Bus without supplying its required connection string; Data Protection Key Vault mode/key identifier and cache connection settings are not injected; Key Vault grants only `Key Vault Secrets User` while API Data Protection needs crypto access; and Azure Key Vault secrets are not seeded/referenced by Bicep.
- ACA-specific hardening remains: Interactive Server Blazor requires sticky-session support or a deliberate circuit-scale architecture, and ACR should use managed-identity pull access instead of leaving the registry admin user enabled. Storage role assignment exists, but UI storage/key-vault access should be reduced to least privilege because profile assets are API-mediated.
- `PLAN.md` and `PLAN_EXECUTION_STRATEGY.md` describe the intended SaaS deployment-stamp work but have completion checkboxes that lag the existing workflows/Bicep modules. Update them only after the runtime configuration contract is fixed and smoke-tested.
- Next: implement one focused deployment-stamp hardening pass covering .NET runtime, managed-identity roles, non-secret configuration injection, Key Vault/Container Apps secret wiring, ACA Interactive Server affinity, and the associated documentation/plan certification.

### 26. Deployment-stamp hardening

- Re-read the handoff and the BaseTemplate deployment continuity notes before beginning the hardening pass. The existing branch state and completed deployment audit remain authoritative; no completed payment, IAM, or local-platform work will be repeated.
- API bootstrap, PostgreSQL module, and provisioning workflow inputs inspected. Outside Development, the API already loads the configured Key Vault through `DefaultAzureCredential`; no second configuration provider is needed. PostgreSQL exposes the server FQDN and database name needed to compose the runtime connection string.
- The provisioning workflow currently supplies only SQL administrator credentials, deployment target, provider, and prefix. It has no optional Azure Redis input, so the stamp must accept Redis as an optional secure deployment input without making it mandatory.
- The Bicep contracts were inspected. Both hosts duplicate the same non-secret API configuration and omit the Key Vault-secret runtime values; App Service is still hard-coded to `DOTNETCORE|8.0`; and Key Vault/Storage incorrectly grant the UI identity access it does not need.
- ACA currently enables the ACR admin account and has no registry identity or ingress affinity. The Service Bus module exposes a connection string from its namespace; main can inject it into Key Vault without exposing it in image configuration.
- Next: verify the current Azure resource schema/role identifiers from official sources, then apply the focused Bicep and workflow contract change.
- Official Azure documentation verified the required deployment contract: ACA sticky sessions use `ingress.stickySessions.affinity = 'sticky'` and require HTTP ingress/single-revision mode; Key Vault Crypto User is role definition `12338af0-0e69-4776-bea7-57ae8d297424`. The existing Key Vault RBAC model supports this assignment.
- Official Azure RBAC documentation also verified `AcrPull` as role definition `7f951dda-4ed3-4680-a7ca-43fe172d538d`; the Container Apps identities can use it instead of the ACR admin account.
- The interrupted Bicep/workflow patch completed successfully and was reconciled against the working tree: App Service now uses .NET 10; Key Vault receives database, Service Bus, and optional Redis secrets; API-only Key Vault/Storage access is enforced; API receives Key Vault data-protection settings; Container Apps use managed-identity ACR pull and UI sticky sessions; provisioning accepts optional Redis input.
- The single planned Bicep compilation smoke test ran and found a deterministic Bicep error: ACR role-assignment names cannot use runtime-generated system-assigned principal IDs. The test also reported pre-existing conditional-module and Service Bus secret-output warnings.
- ACR role-assignment names now use deterministic Container App names while the assignment principal remains the system-assigned identity. This removes the compile-time BCP120 failure without reintroducing ACR admin access.
- Verification retry rule: when a targeted verification fails and its identified cause is corrected, rerun that same verification once. Stop immediately if it passes; if it fails again, record the new failure and do not repeat it without a further targeted correction. `PLAN.md` and `PLAN_EXECUTION_STRATEGY.md` remain unchanged until a clean smoke test certifies this deployment stamp.
- The Service Bus module and Key Vault call site were inspected after the compilation result. The namespace module currently exports `listKeys(...).primaryConnectionString`, which triggers Azure's secret-output warning. The next focused correction is to resolve that key only inside the Key Vault module and remove the secret-bearing module output; no second compilation will be run in this task.
- The Service Bus secret-output path is now removed. `servicebus.bicep` exports only non-sensitive namespace metadata; `keyvault.bicep` resolves the existing namespace's `RootManageSharedAccessKey` internally when creating `Messaging--AzureServiceBus--ConnectionString`; and `main.bicep` passes only the safe namespace output. The targeted Bicep smoke test may now be rerun once under the verification retry rule.
- Publish status checked: the deployment hardening changes remain uncommitted on `ops/azure-sandbox-deployment`; no push or PR update has been made in this task. The current PR query returned no active PR for this branch.
- The permitted Bicep re-check was invoked once, but the Azure CLI command timed out after 14 seconds before producing a compiler result. Treat deployment-stamp compilation as unverified, not passed; do not run another compilation in this task without an explicit new retry decision.
- The requested publish set was staged on `ops/azure-sandbox-deployment`: provisioning workflow, deployment Bicep modules, and this handoff. No unrelated working-tree files were staged.
- Deployment hardening committed as `60f5a39` (`Harden Azure deployment runtime configuration`). The Bicep smoke remains unverified because the permitted retry timed out before compiler output.
