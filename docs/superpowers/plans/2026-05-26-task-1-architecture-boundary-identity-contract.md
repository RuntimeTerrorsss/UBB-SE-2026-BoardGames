# Task 1 Architecture Boundary And Identity Contract Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Produce the accepted Task 1 contract for project boundaries, Shared/Data ownership, forbidden dependencies, and the unified user/session identity that every later `.Api` and `.Desktop` task must follow.

**Architecture:** Task 1 is a setup gate, not a feature implementation lane. The output should be a written contract plus an evidence inventory; it should not attempt to merge duplicate controllers, rewrite Desktop navigation, or make the whole solution build. The target direction is `Desktop -> Shared DTOs/API clients -> HTTP API -> API services -> Data repositories -> AppDbContext`.

**Tech Stack:** WinUI 3 Desktop, ASP.NET Core API, .NET 8 class libraries, EF Core Data project, Shared DTO/API-client project, Markdown audit docs.

---

## Scope Decision

Task 1 should be implemented as an architecture gate.

It should create the contract that Tasks 2-10 follow. It should not remove all forbidden references immediately, because the current codebase still has old Desktop screens and old Shared proxy repositories that depend on `.Api` and `.Data`. Removing those references before Task 2, Task 7, and Task 8 would create a large unrelated compile-failure surface.

Task 1 should therefore produce:

- one accepted dependency rule;
- one accepted Shared/Data ownership rule;
- one accepted user identity/session rule;
- one inventory of current violations;
- a clear note saying which violations are temporary and which later tasks must remove them.

No application code should be changed in Task 1 unless the lead explicitly decides to turn this gate into an enforcement refactor. The safer implementation is docs-first.

## Findings From The Codebase

Current project references show the main boundary problem:

```text
BoardGames.Desktop -> BoardGames.Data
BoardGames.Desktop -> BoardGames.Shared
BoardGames.Desktop -> BoardGames.Api
BoardGames.Desktop -> ServerCommunication
BoardGames.Shared  -> BoardGames.Data
BoardGames.Api     -> BoardGames.Data
BoardGames.Api     -> BoardGames.Shared
BoardGames.Web     -> BoardGames.Shared
```

The target final direction is:

```text
BoardGames.Desktop -> BoardGames.Shared
BoardGames.Web     -> BoardGames.Shared
BoardGames.Api     -> BoardGames.Shared + BoardGames.Data
BoardGames.Data    -> no BoardGames.Shared dependency
BoardGames.Shared  -> no BoardGames.Data dependency
```

Important evidence:

- `BoardGamesApp/BoardGames.Desktop/BoardGames.Desktop.csproj` references `.Data`, `.Shared`, `.Api`, and `ServerCommunication`.
- `BoardGamesApp/BoardGames.Shared/BoardGames.Shared.csproj` references `.Data`.
- `BoardGamesApp/BoardGames.Shared/ProxyRepositories` contains repository-shaped HTTP proxies that implement Data repository interfaces.
- `BoardGamesApp/BoardGames.Shared/DTO/MessageDTO.cs` imports `BoardGames.Data.Enums`.
- `BoardGamesApp/BoardGames.Desktop/App.xaml.cs` constructs `AppDbContext`, repository proxies, and API service classes directly.
- `BoardGamesApp/BoardGames.Desktop/App.xaml2.cs` is closer to the final client model because it uses API-client registration, but it references old/missing namespaces such as `BoardRentAndProperty.ApiClient`, `BoardRentAndProperty.Contracts.DataTransferObjects`, and `BoardRentAndProperty.Utilities`.
- `BoardGamesApp/BoardGames.Data/Enums/SessionContext.cs` contains an old integer-user singleton session. It should not be the final Desktop session owner.
- `BoardGamesApp/BoardGames.Shared/DTO/AccountProfileDTO.cs` contains the newer account profile shape with `Guid Id`, role, email, avatar, suspended/locked state, and profile fields.
- `BoardGamesApp/BoardGames.Data/Models/User.cs` contains both `Guid Id` and `int PamUserId`, so the final identity contract needs both a public account id and a legacy/internal integer id.
- API controllers such as old `GamesController`, `RentalsController`, `UsersController`, `ConversationController`, and `PaymentsController` inject repositories directly. This is not fixed by Task 1, but Task 1 must mark it as forbidden final architecture for Task 2 and API lanes.

## File Structure

Task 1 should create these docs:

- Create: `docs/audits/task-1-architecture-boundary-identity-contract.md`
  - Owns the final rule: dependency direction, project ownership, ID/session contract, forbidden final dependencies, and handoff rules for Tasks 2-10.

- Create: `docs/audits/task-1-boundary-violation-inventory.md`
  - Owns the evidence list from the current codebase: project references, Shared/Data violations, Desktop direct API/Data usage, API repository-in-controller usage, and namespace drift.

- Modify: `docs/audits/desktop-api-10-task-assignment-plan.md`
  - Add a short note under Task 1 pointing to the two new Task 1 outputs.

Task 1 should not modify these production files yet:

- `BoardGamesApp/BoardGames.Desktop/BoardGames.Desktop.csproj`
- `BoardGamesApp/BoardGames.Shared/BoardGames.Shared.csproj`
- `BoardGamesApp/BoardGames.Desktop/App.xaml.cs`
- `BoardGamesApp/BoardGames.Desktop/App.xaml2.cs`
- `BoardGamesApp/BoardGames.Shared/ProxyRepositories/*`
- `BoardGamesApp/BoardGames.Api/Controllers/*`

Those files are evidence for the contract. Actual cleanup belongs to later tasks unless the lead expands Task 1.

---

### Task 1.1: Create The Boundary And Identity Contract

**Files:**

- Create: `docs/audits/task-1-architecture-boundary-identity-contract.md`

- [ ] **Step 1: Write the contract document**

Create `docs/audits/task-1-architecture-boundary-identity-contract.md` with this structure and content:

```markdown
# Task 1: Architecture, Boundary, And Identity Contract

**Purpose:** Accepted contract for the `.Desktop + .Api` department before API cleanup and Desktop integration work continues.

**Status:** Proposed for lead acceptance.

## Final Dependency Direction

The final dependency direction is:

```text
BoardGames.Desktop -> BoardGames.Shared -> HTTP API
BoardGames.Web     -> BoardGames.Shared -> HTTP API
BoardGames.Api     -> BoardGames.Shared + BoardGames.Data
BoardGames.Data    -> database only
```

Inside the API, the final direction is:

```text
Controller -> Service -> Repository -> AppDbContext -> Database
```

## Forbidden Final Dependencies

The final application must not contain:

- `BoardGames.Desktop -> BoardGames.Api`;
- `BoardGames.Desktop -> BoardGames.Data`;
- `BoardGames.Desktop -> AppDbContext`;
- `BoardGames.Desktop -> repository interfaces or implementations`;
- `BoardGames.Shared -> BoardGames.Data`;
- `BoardGames.Data -> BoardGames.Shared`;
- API controllers injecting repositories directly.

Temporary violations must be listed in `docs/audits/task-1-boundary-violation-inventory.md` and removed by the task that owns that area.

## Project Ownership

| Project | Owns | Must Not Own |
| --- | --- | --- |
| `BoardGames.Desktop` | WinUI pages, viewmodels, navigation, local session state, local API-client configuration | API controllers, API services, repositories, `AppDbContext`, migrations, seed data |
| `BoardGames.Shared` | DTOs, API-client interfaces, API-client implementations, API response wrappers, transport enums | EF models, repository interfaces, repository implementations, `AppDbContext` |
| `BoardGames.Api` | Controllers, business services, mappers, auth logic, runtime/DI configuration | WinUI pages, Desktop navigation, direct UI state |
| `BoardGames.Data` | EF models, repositories, migrations, `AppDbContext`, persistence enums | Shared DTOs, Desktop session, HTTP clients |
| `ServerCommunication` | UDP message transport for optional local notification server | API business logic, database access |
| `NotificationServer` | Optional local notification process | Domain persistence, API route ownership |

## Shared/Data Ownership Rule

`.Shared` owns transport shapes. `.Data` owns persistence shapes. The API maps between them.

DTOs that Desktop/Web receive from API belong in `.Shared`.

EF entities and repository contracts belong in `.Data`.

Transport enums used by Desktop/Web/API contracts belong in `.Shared`. EF-only enums may stay in `.Data`. If both are needed, API maps between them.

## Identity Contract

The public identity sent to Desktop/Web is:

```text
AccountId: Guid
```

The legacy/internal identity used by old chat/rental/payment tables is:

```text
PamUserId: int
```

Desktop should primarily use `AccountId`. If an active Desktop feature still needs `PamUserId`, the login/profile response must provide it explicitly. Desktop must not guess it and must not use static Alice/Bob/Carol IDs in the final flow.

## Login/Profile Session Fields

The login/profile response needed by Desktop is:

```text
AccountId: Guid
PamUserId: int?
Username: string
DisplayName: string
Email: string
Role: string
AvatarUrl: string?
IsSuspended: bool
IsLocked: bool
Country: string?
City: string?
StreetName: string?
StreetNumber: string?
```

## Desktop Session Contract

Desktop should have one session source used by:

- Filter;
- Game Details;
- Games/My Games/Admin Games;
- Chat;
- Notifications;
- Dashboard/payment history;
- Account;
- Admin.

The final Desktop session stores:

```text
IsLoggedIn
AccountId
PamUserId, only while legacy chat/rental/payment code still needs it
Username
DisplayName
Email
Role
AvatarUrl
Account status
Profile fields required by Account page
```

The final Desktop session must replace:

- `BoardGames.Data.Enums.SessionContext`;
- hardcoded Alice/Bob/Carol switching;
- separate old project 1 and old project 2 session concepts.

## Handoff To Later Tasks

Task 2 uses this document to clean duplicate API controllers/services.

Tasks 3-6 use this document to expose service-layer APIs with stable DTO and identity rules.

Task 7 uses this document when wiring the local API runtime.

Task 8 uses this document to create the final Desktop startup, API-client configuration, and session implementation.

Tasks 9-10 use this document to make feature screens consume the same session and API-client contracts.

## What Counts As Accepted

The lead accepts this document as the rule for the department.

No later task should need to guess whether to use DTOs, EF models, `AccountId`, `PamUserId`, repositories, API services, or API clients.
```

- [ ] **Step 2: Read the contract for accidental overreach**

Run:

```powershell
Get-Content -Path 'docs/audits/task-1-architecture-boundary-identity-contract.md'
```

Expected:

```text
The document exists, says Task 1 is a contract gate, and does not assign Task 2-10 feature implementation to Task 1.
```

---

### Task 1.2: Create The Boundary Violation Inventory

**Files:**

- Create: `docs/audits/task-1-boundary-violation-inventory.md`

- [ ] **Step 1: Capture project-reference evidence**

Run:

```powershell
rg -n "<ProjectReference" BoardGamesApp -g '*.csproj'
```

Expected important findings:

```text
BoardGamesApp\BoardGames.Desktop\BoardGames.Desktop.csproj references BoardGames.Data, BoardGames.Shared, BoardGames.Api, ServerCommunication.
BoardGamesApp\BoardGames.Shared\BoardGames.Shared.csproj references BoardGames.Data.
BoardGamesApp\BoardGames.Api\BoardGames.Api.csproj references BoardGames.Data and BoardGames.Shared.
BoardGamesApp\BoardGames.Web\BoardGames.Web.csproj references BoardGames.Shared.
```

- [ ] **Step 2: Capture Shared/Data violations**

Run:

```powershell
rg -n "BoardGames\.Data|BookingBoardGames\.Data|Data\.Repositories|Data\.Models|AppDbContext|IRepository|Interface.*Repository" BoardGamesApp/BoardGames.Shared -g '*.cs'
```

Expected important findings:

```text
BoardGames.Shared/DTO/MessageDTO.cs imports BoardGames.Data.Enums.
BoardGames.Shared/ProxyRepositories/UserAPIProxy.cs imports BoardGames.Data.Models and BoardGames.Data.Repositories.
BoardGames.Shared/ProxyRepositories/ConversationAPIProxy.cs imports BoardGames.Data, Data.Enums, Data.Models, and Data.Repositories.
BoardGames.Shared/ProxyRepositories/GamesAPIProxy.cs imports BoardGames.Data and implements InterfaceGamesRepository.
BoardGames.Shared/ProxyRepositories/RepositoryPaymentAPIProxy.cs implements IRepositoryPayment.
BoardGames.Shared/ProxyRepositories/RentalAPIProxy.cs imports BookingBoardGames.Data.
```

- [ ] **Step 3: Capture Desktop direct API/Data violations**

Run:

```powershell
rg -n "BoardGames\.Api|BoardGames\.Data|BookingBoardGames\.Data|Data\.Repositories|AppDbContext|DatabaseBootstrap|GamesAPIProxy|RentalAPIProxy|RepositoryPaymentAPIProxy|PaymentAPIProxy|UserAPIProxy|ConversationAPIProxy" BoardGamesApp/BoardGames.Desktop -g '*.cs'
```

Expected important findings:

```text
BoardGames.Desktop/App.xaml.cs creates AppDbContext and repository/API service instances.
BoardGames.Desktop/DatabaseBootstrap.cs uses AppDbContext.
BoardGames.Desktop/Navigation/BookingNavigationArguments.cs depends on BoardGames.Api.Services.ConversationService.
Several payment/chat/filter/game-details viewmodels import Data enums, Data repositories, or API services.
Several old views use BookingBoardGames.Data.Enums.SessionContext.
```

- [ ] **Step 4: Capture API controller repository-injection violations**

Run:

```powershell
rg -n "Repository|AppDbContext|Interface.*Repository|I.*Repository" BoardGamesApp/BoardGames.Api/Controllers -g '*.cs'
```

Expected important findings:

```text
UsersController.cs injects IUserRepository.
GamesController.cs injects InterfaceGamesRepository.
RentalsController.cs injects IRentalRepository, IConversationRepository, and InterfaceGamesRepository.
ConversationController.cs injects IConversationRepository.
PaymentsController.cs injects IPaymentRepository and IRepositoryPayment.
```

- [ ] **Step 5: Write the inventory document**

Create `docs/audits/task-1-boundary-violation-inventory.md` with these sections:

```markdown
# Task 1: Boundary Violation Inventory

**Purpose:** Evidence list for the current codebase violations of the Task 1 architecture contract.

**Status:** Inventory only. This document does not assign cleanup ownership to Task 1.

## Project References

| File | Current Reference | Final Status | Owner To Remove Or Replace |
| --- | --- | --- | --- |
| `BoardGamesApp/BoardGames.Desktop/BoardGames.Desktop.csproj` | `BoardGames.Data` | forbidden final dependency | Task 8 / Desktop API-client setup |
| `BoardGamesApp/BoardGames.Desktop/BoardGames.Desktop.csproj` | `BoardGames.Api` | forbidden final dependency | Task 8 / Desktop API-client setup |
| `BoardGamesApp/BoardGames.Desktop/BoardGames.Desktop.csproj` | `BoardGames.Shared` | final allowed dependency | keep |
| `BoardGamesApp/BoardGames.Desktop/BoardGames.Desktop.csproj` | `ServerCommunication` | temporary notification dependency | Task 6 / Task 8 decide final local notification path |
| `BoardGamesApp/BoardGames.Shared/BoardGames.Shared.csproj` | `BoardGames.Data` | forbidden final dependency | Task 15 / Shared API-client contract |
| `BoardGamesApp/BoardGames.Api/BoardGames.Api.csproj` | `BoardGames.Data` | final allowed dependency | keep |
| `BoardGamesApp/BoardGames.Api/BoardGames.Api.csproj` | `BoardGames.Shared` | final allowed dependency | keep |
| `BoardGamesApp/BoardGames.Web/BoardGames.Web.csproj` | `BoardGames.Shared` | final allowed dependency | keep |

## Shared/Data Violations

List the Shared files that currently import Data models, Data enums, or Data repository interfaces.

## Desktop API/Data Violations

List the Desktop files that currently import Data, AppDbContext, repository types, or API service types.

## API Controller Boundary Violations

List the API controllers that currently inject repositories directly.

## Identity/Session Split

Current identity is split between:

- `Guid` account identity in account/profile/admin/game-management flow;
- `int` user identity in old filter/chat/rental/payment flow;
- static hardcoded user switching in old Desktop screens.

Final rule:

- public identity is `Guid AccountId`;
- legacy/internal identity is `int PamUserId`;
- API translates where needed;
- Desktop stores both only if the active legacy flow still needs the integer id.

## Cleanup Ownership

Task 1 documents the violations. It does not remove every violation.

Task 2 owns duplicate API cleanup.

Task 7 owns API runtime wiring.

Task 8 owns Desktop startup, API-client configuration, and final session implementation.

Task 15 or the Shared/API-client owner owns removing Shared proxy repositories that implement Data repository interfaces.
```

- [ ] **Step 6: Verify the inventory contains all major violation groups**

Run:

```powershell
Select-String -Path 'docs/audits/task-1-boundary-violation-inventory.md' -Pattern 'Project References','Shared/Data','Desktop API/Data','API Controller','Identity/Session'
```

Expected:

```text
Each pattern appears at least once.
```

---

### Task 1.3: Link Task 1 Outputs From The 10-Task Plan

**Files:**

- Modify: `docs/audits/desktop-api-10-task-assignment-plan.md`

- [ ] **Step 1: Add the output note under Task 1**

In `docs/audits/desktop-api-10-task-assignment-plan.md`, under `## Task 1: Architecture, Boundary, And Identity Contract`, add this short note after the `Output` list:

```markdown
### Task 1 Output Documents

- `docs/audits/task-1-architecture-boundary-identity-contract.md`
- `docs/audits/task-1-boundary-violation-inventory.md`
```

- [ ] **Step 2: Verify the links were added**

Run:

```powershell
Select-String -Path 'docs/audits/desktop-api-10-task-assignment-plan.md' -Pattern 'Task 1 Output Documents','task-1-architecture-boundary-identity-contract','task-1-boundary-violation-inventory'
```

Expected:

```text
The three patterns are found in the Task 1 section.
```

---

### Task 1.4: Final Task 1 Review

**Files:**

- Read: `docs/audits/task-1-architecture-boundary-identity-contract.md`
- Read: `docs/audits/task-1-boundary-violation-inventory.md`
- Read: `docs/audits/desktop-api-10-task-assignment-plan.md`

- [ ] **Step 1: Check that Task 1 did not become Task 2 or Task 8**

Run:

```powershell
Select-String -Path 'docs/audits/task-1-architecture-boundary-identity-contract.md','docs/audits/task-1-boundary-violation-inventory.md' -Pattern 'Task 2','Task 7','Task 8','Task 15'
```

Expected:

```text
The docs mention later task ownership, but do not assign duplicate controller cleanup, API runtime wiring, Desktop startup, or Shared API-client cleanup to Task 1.
```

- [ ] **Step 2: Check the final contract has the required architecture rule**

Run:

```powershell
Select-String -Path 'docs/audits/task-1-architecture-boundary-identity-contract.md' -Pattern 'BoardGames.Desktop -> BoardGames.Shared','Controller -> Service -> Repository','AccountId','PamUserId'
```

Expected:

```text
All four patterns are found.
```

- [ ] **Step 3: Check no tests or builds were run as Task 1 verification**

Do not run `dotnet build`.

Do not run tests.

Task 1 verification is document and inventory verification only.

Expected:

```text
The implementation stays inside Task 1's contract-gate scope.
```

---

## Implementation Order

Use this order:

```text
Task 1.1 -> Task 1.2 -> Task 1.3 -> Task 1.4
```

Do not run these in parallel, because the contract document should shape the inventory language and the 10-task-plan link.

## Out Of Scope For Task 1

Do not remove duplicate controllers.

Do not choose canonical `GamesController`, `RentalsController`, or `UsersController`.

Do not merge duplicate DTOs.

Do not remove `.Desktop -> .Api` or `.Desktop -> .Data` references during Task 1 unless the lead explicitly expands this task into an enforcement refactor.

Do not remove `.Shared -> .Data` during Task 1 unless the Shared proxy-repository replacements are also in scope.

Do not implement the final Desktop shell.

Do not implement API runtime dependency injection.

Do not create migrations or seed data.

Do not run tests.

Do not try to make the entire application build.

## Self-Review Checklist

- [ ] The plan creates the accepted architecture rule.
- [ ] The plan creates the accepted Shared/Data ownership rule.
- [ ] The plan creates the accepted identity/session contract.
- [ ] The plan records current codebase violations.
- [ ] The plan does not make Task 1 responsible for Task 2 duplicate API cleanup.
- [ ] The plan does not make Task 1 responsible for Task 8 Desktop shell/session implementation.
- [ ] The plan clearly says why project references should not be ripped out immediately.
- [ ] The plan uses only repo-local evidence.
