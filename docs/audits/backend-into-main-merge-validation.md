# Backend → Main Merge & Stabilization — Validation & Handoff Record

Date: 2026-05-29
Integration branch: `integration/backend-merge` (created from `main`)
Merged source: `backend` at commit `df14bbc` (`Merge pull request #45 from RuntimeTerrorsss/task6-tests`)
Validation environment: Windows 11, .NET SDK 10.0.201, `maui-windows` workload, EF Core tooling 8.0.11, `(localdb)\MSSQLLocalDB`.

## 1. Outcome Summary

- `backend` was inspected, built, and exercised at runtime **before** being used as the merge baseline.
- Production projects build clean on `backend`; only `BoardGames.Tests` fails (pre-existing, out of scope — see §8).
- The API was booted and smoke-tested: it starts, exposes the canonical route groups, and the canonical identity / cookie-auth / admin-authorization flows work at runtime.
- `backend` was merged into `main` on `integration/backend-merge`. The merge was **textually clean (zero conflicts)**.
- After the merge, the production code (`.Api`, `.Shared`, `.Data`, `.Desktop`, `.Web`) is **byte-identical to the validated `backend`**; the only delta versus `backend` is two task-4 test files preserved from `main`.
- All production projects build on the merged branch; project-reference architecture is correct.
- Remaining gaps (broken test project, seed/identity data, Desktop GUI runtime) are documented as blockers, not worked around.

## 2. Branch Topology & Merge Mechanics

- Merge base of `main` and `backend`: `faef9ce` (`Merge PR #42 Task6-RequestProxyService`).
- `backend` was **17 commits ahead** of the merge base; `main` had only **2 commits** beyond it (`526ec79` "implemented the tests for task 4" + its merge `8eb746e`). `main`'s only divergence from the base is in two test files.
- `backend` had already merged `main`'s Web/Shared history (via `3e21db9 Merge branch 'main' into task10-integration-wip`), so `backend` already contained `main`'s pre-base Web/Shared work.
- **`backend` changed no files under `BoardGames.Web/`** relative to `main` — the Web project is identical on both branches. The only Web-affecting risk was whether Web still compiles against `backend`'s modified `Shared` contracts (it does — see §7).
- Note: `WEB 7` (`1d22c1c`, "Integrate Remaining Features…") lives on the unmerged branch `origin/CotaAndrei/task`. It is in **neither** `main` nor `backend` and was **not** part of this merge.

`git merge-tree` predicted a clean merge; the actual merge used the `ort` strategy with no conflicts.

## 3. Phase 1 — Documentation Reviewed

Audit docs (`docs/audits/`): `desktop-api-final-workflow`, `desktop-api-contract-audit`, `task-1-architecture-boundary-identity-contract`, `task-1-boundary-violation-inventory`, `task-2-api-duplicate-cleanup-decisions`, `task-4-games-filter-search-backend-apis`, `task-7-api-runtime-wiring-analysis`.
Task docs (`docs/tasks/`): Auth/Account/Admin, Games/Filter/Search, Request/Rental lifecycle (+task-5), Chat/Notifications/Payments (+task-6), API duplicate cleanup, and the three `Desktop_*` integration docs (Tasks 8–10).

Canonical expectations extracted (used to judge the merge):

- **Dependency direction:** `Desktop/Web → Shared (DTOs/API clients) → HTTP → Api → Services → Repositories → AppDbContext`. `Shared` must **not** reference `Data`. `Api → Shared + Data`. `NotificationServer/ServerCommunication` are accepted temporary local-notification support.
- **Identity / auth:** cookie authentication; public identity is `Guid AccountId`; legacy `int PamUserId` is an internal bridge for chat/rental/payment; login returns an `AccountProfileDTO`; `[Authorize(Roles="Admin")]` must be enforced server-side.
- **Canonical route groups:** `api/auth`, `api/accounts`, `api/admin`, `api/games` (+`/search`), `api/requests`, `api/rentals`, `api/Conversation`, `api/notifications`, `api/Payments` (+`/user/{accountId}/history`), and the single reduced `api/users/except/{excludeAccountId}`.
- **Quarantined / must-not-revive:** V1 duplicate controllers/services (`GamesController`/`RentalsController`/`UsersController` V1, `*Service`/`*Service2` duplicates), `feed/tonight`/`feed/remaining`, `api/users/login|register`, legacy `BookingBoardGames.*`/`BoardRentAndProperty.*` namespaces, `Shared/ProxyRepositories/*`, deleted `GameDTO`, deprecated Desktop My/Others' Requests & Rentals screens.

(The audit docs describe the **pre-Task-7** state, including a then-broken `Shared` build and unwired DI. `backend` post-dates those fixes — see §4/§5.)

## 4. Phase 2 — Pre-Merge Build Validation (`backend` @ df14bbc)

Worktree: `E:/bg-backend-validate` (detached at `df14bbc`).
Command: `dotnet build BoardGames.sln -c Debug` (full log: `build-backend-premerge.log`).

| Project | Result |
|---|---|
| BoardGames.Shared | **Built** (no `Data` reference; the audit's 23 Shared errors are resolved on `backend`) |
| BoardGames.Data | **Built** |
| BoardGames.Api | **Built** |
| BoardGames.Desktop | **Built** (`net8.0-windows10.0.19041.0`, win-x64) |
| BoardGames.Web | **Built** |
| NotificationServer / ServerCommunication | **Built** |
| BoardGames.Tests | **FAILED — 362 errors** (pre-existing, out of scope; see §8) |

All 362 build errors are confined to `BoardGames.Tests`. No production project has any error.

## 5. Phase 2 — Pre-Merge Runtime Validation (`backend` @ df14bbc)

Setup: `dotnet ef database update --project BoardGames.Data --startup-project BoardGames.Api` applied `InitialCreate` + `UnifiedMergedSchema` to `MergedBoardGamesDb`. API started via `dotnet run --launch-profile http` on `http://localhost:5018` (started cleanly; only a non-fatal "wwwroot not found" static-files warning).

| Check | Expected | Actual |
|---|---|---|
| API process startup | host starts, DI graph resolves | **OK** — "Now listening on http://localhost:5018" |
| `GET /swagger/v1/swagger.json` | 200, canonical route groups | **200** — all canonical groups present; **no legacy/duplicate routes leaked** |
| Conversation route casing (audit open question) | confirm at runtime | route is `**/api/Conversation/...**` (capitalized, singular) |
| `GET /api/games` (anonymous) | 200, read path works | **200** `[]` (empty DB; controller→service→repo→EF path verified) |
| `GET /api/admin/accounts` (no auth) | 401 | **401** |
| `POST /api/auth/register` (fresh account) | 200 | **200** `{"data":true}` |
| `POST /api/auth/login` | 200 + auth cookie | **200**, returns `AccountProfileDTO` (Guid id, role "Standard User") + `.AspNetCore.Cookies` issued |
| `GET /api/admin/accounts` (authed standard user) | 403 (role enforced) | **403** |
| `GET /api/accounts/{id}` (authed) | 200 | **200**, profile returned |

This was exercised with a **newly registered account through the real API** — no hardcoded/fake user. It proves the canonical identity, cookie auth, account retrieval, and server-side admin role authorization work at runtime.

## 6. Phase 3 — Verified Integration Baseline

`backend`'s `.Api`/`.Desktop`/`.Shared`/`.Data` production code (buildable + runtime-validated above) is the canonical baseline. `main` contributes only the task-4 test additions. No documented completed workflow on `backend` contradicted the audit/task requirements, so `backend` production behavior is preserved wholesale.

## 7. Phase 4 — Merge & Conflict Resolution

- `git merge --no-ff df14bbc` into `integration/backend-merge`: **clean, zero conflicts** (`ort` strategy).
- Diff of merged `HEAD` vs `df14bbc` over `BoardGames.Api`/`.Shared`/`.Data`/`.Desktop`/`.Web`: **empty** — production code is byte-identical to the validated baseline.
- Total delta vs `backend`: only `BoardGames.Tests/Api/Controllers/GamesControllerTests.cs` and `BoardGames.Tests/Api/Services/GameServiceTests.cs` (main's task-4 tests, preserved; `GameServiceTests.cs` auto-merged without conflict).
- **No direct Git conflicts** and **no semantic conflicts requiring contract reselection**: because `backend` already contained `main`'s Web/Shared work, there were no competing contract shapes to reconcile. `Shared` contract changes on `backend` (new `FilterType`/`PaymentMethod`/`ReadReceiptDTO` DTOs; `IGameService`/`IPaymentService`/`IConversationService`/`ConversationService`/`GameService`/`ServiceCollectionExtensions` updates) are the single canonical set, and Web compiles against them.

### Architecture boundary verification (merged tree)

| Rule | Result |
|---|---|
| `Web → Shared` only; no direct `AppDbContext`/`Data` access in Web | **OK** (Web references Shared only; no Data/DbContext usage found) |
| `Desktop → Shared` (+ temporary `ServerCommunication`); no `Data`/`Api` | **OK** |
| `Shared` must not reference `Data` | **OK** (no Data project reference) |
| `Api → Shared + Data` | **OK** |

## 8. Post-Merge Build Verification

Command: `dotnet build BoardGames.sln -c Debug` on `integration/backend-merge` (full log: `build-merged.log`).

- Production DLLs emitted: `BoardGames.Api`, `BoardGames.Data`, `BoardGames.Shared`, `BoardGames.Web`, `BoardGames.Desktop` (`net8.0-windows10.0.19041.0`/win-x64), `NotificationServer`, `ServerCommunication`.
- **Zero errors in any production project.**
- `BoardGames.Tests`: still 362 errors (181 distinct) — **identical to `backend`; not introduced or worsened by the merge.**

### Post-merge workflow verification

Because the merged production binaries are byte-identical to the validated `backend` (§7), the Phase-2 runtime smoke (§5) applies unchanged to the merged result. No production code path differs, so there is no integration regression to re-derive. Desktop continues to target `http://localhost:5018/` (`App.config` `ApiBaseUrl`) and consumes the same Shared API clients against the same verified API.

## 9. Documented Blockers (out of scope — not worked around)

1. **`BoardGames.Tests` does not compile (362 errors, pre-existing on `backend`).** Root cause: the test project was not updated to the refactored/quarantined production surface. Examples: references to removed/quarantined `IReceiptService`, `ServicePayment`, `SearchAndFilterService`, legacy `ConversationService`, `UserService`, deleted `GameDTO`; deprecated Desktop screens `CreateRequestViewModel`/`CreateRentalViewModel`/`RequestsFromOthersViewModel`; removed namespaces `BoardGames.Api.Legacy`, `BoardGames.Shared.ProxyRepositories`; ambiguous `ReadReceiptDTO` (exists in both `Shared.DTO` and `Data.Models`), ambiguous `ServiceResult`, and NUnit-vs-Xunit `Theory`. This is broad test repair, explicitly outside this task; it does not affect the production build or runtime.
2. **Seed data is incompatible with the unified schema.** `BoardGames.Data/SeedMockData.sql` targets the legacy schema (`SET IDENTITY_INSERT users` with int ids; placeholder `password_hash` values like `'hash1'`). The canonical identity uses `Guid` `User.Id` with PBKDF2 hashes and a separate roles model. Consequence: **logging in as a seeded user (alice01…) is not possible**, and full multi-actor data-driven flows cannot be demonstrated from the seed. Producing unified-schema seed data is externally owned (database seed/setup).
3. **Full end-to-end data flows not runtime-verified.** The endpoints exist and are wired (Swagger + empty-200 reads confirm the paths), but exercising games-list-with-data → rental request → owner notification → approve → rental → payment history requires canonical seed data (blocker #2). Endpoint presence and the auth/identity backbone are verified; the data-driven choreography is not.
4. **Desktop GUI runtime not driven.** `BoardGames.Desktop` is a WinUI app that cannot be driven headlessly in this environment. It builds, targets the verified local API, and uses the same Shared API clients; its UI workflows were not click-tested.
5. **Web page-level runtime not driven.** `BoardGames.Web` builds against the merged Shared contracts (compile-level contract alignment confirmed) and contains no direct DB access, but page workflows depend on the same seed/identity data (blocker #2) and were not click-tested.

## 10. Scope Boundaries Respected

- No production code rewritten; `backend` `.Api`/`.Desktop` behavior preserved as-is.
- No tests added or repaired; no broad warning cleanup; no unrelated routing or seed/DB redesign.
- No invented routes/DTOs; no revived deprecated controllers/services; no hardcoded users or fake success states.
- No forbidden project references added; architecture direction left correct.

## 11. Landing Status

The merge and all verification live on the local branch `integration/backend-merge`. Integrating this into the shared `main` (fast-forward/merge + push, or PR) is an outward-facing step left to the team/lead's chosen process.
