# Merge Spec: Task 1-6 Branch With Main

## Purpose

This document defines how to resolve the current merge between the `desktop` branch, which contains Tasks 1-6 for the `.Desktop + .Api` department, and `origin/main`, which contains earlier important changes such as namespace/import cleanup and Desktop navigation redirections.

The goal of the merge is not to choose one side blindly. The goal is to preserve the architecture and feature contracts created by Tasks 1-6 while also preserving useful main-branch UI/routing/import work.

The final merged code must still follow the assignment direction:

```text
BoardGames.Desktop -> BoardGames.Shared -> HTTP API
BoardGames.Api -> BoardGames.Shared + BoardGames.Data
BoardGames.Shared -> no BoardGames.Data
BoardGames.Desktop -> no BoardGames.Api / BoardGames.Data
```

The final merged code must also preserve the unified application workflow:

```text
unauthenticated user opens Filter / Discovery
-> login when needed
-> one account/session identity
-> games/filter/details
-> request rental
-> chat + notification side effects
-> accept/decline
-> dashboard/payment history
```

## Current Merge State

The current branch is `desktop`, and it is in the middle of merging `origin/main`.

There are 86 unresolved files:

```text
BoardGames.Api      39
BoardGames.Desktop  38
BoardGames.Shared    7
BoardGames.Data      2
```

The merge is mostly conflicting in these areas:

- API controller/service ownership;
- duplicate API artifacts that Task 2 already quarantined;
- Shared DTO naming and Data-coupling;
- Desktop startup path and namespace direction;
- Desktop navigation/session/auth helpers;
- request/rental/chat/notification/payment flow integration.

Because Tasks 1-6 were intentionally architectural, the merge must be resolved by rule, not by accepting whichever side has fewer lines.

## Source Of Truth Rules

### 1. Tasks 1-6 Are Canonical For Architecture

The `desktop` branch is the source of truth for:

- no Desktop direct reference to `BoardGames.Api`;
- no Desktop direct reference to `BoardGames.Data`;
- no Shared direct reference to `BoardGames.Data`;
- API controllers exposing services, not repositories;
- duplicate API controllers/services being quarantined or removed from the active compile path;
- `Guid AccountId` as the public identity contract;
- `PamUserId` only as an optional bridge for old chat/rental/payment internals;
- project 2-style request/rental/chat/notification lifecycle as the final workflow;
- one backend API surface instead of two old APIs.

### 2. Main Is A Source For Useful UI And Naming Changes

`origin/main` is useful for:

- namespace/import cleanup from old `BookingBoardGames.*` names toward `BoardGames.Desktop.*` and `BoardGames.Shared.*`;
- Desktop Discovery / Filter first-screen work;
- navigation buttons from Discovery to Login, Games, Notifications, Dashboard, Chat, and Account;
- fixes from older DTO names to current DTO names, for example `LoginDTO`, `RegisterDTO`, `CreateRequestDTO`, `RequestActionDTO`, `BookedDateRangeDTO`;
- small style changes such as `this.` usage and copyright headers.

Main must be mined for those useful changes, but it must not restore the old architecture.

### 3. Main Must Not Reintroduce These Things

Do not restore:

- Desktop-created `AppDbContext`;
- `DatabaseBootstrap.Initialize()` from Desktop startup;
- Desktop repository proxies as the active business path;
- `BoardGames.Shared.ProxyRepositories` as compiled code;
- API controllers that inject repositories directly;
- `GamesController2.cs`, `RentalsController2.cs`, `UsersController2.cs`, `UserService2.cs`, `RentalService2.cs`, `IUserService2.cs`, `IRentalService2.cs` as active final files;
- `BoardGames.Shared` DTOs importing `BoardGames.Data`;
- `BoardGames.Data.Enums.SessionContext` as the Desktop final session;
- old integer user id as the public API identity;
- old standalone My Requests / Others' Requests / My Rentals / Others' Rentals flow as the final request/rental workflow.

## Merge Resolution Order

Resolve the merge in this order. Do not start with random page conflicts.

```text
1. Project boundaries and compile includes
2. Shared DTO / API-client contracts
3. API canonical controllers and services
4. API mappers and Data bridge points
5. Desktop startup/session/navigation shell
6. Desktop feature pages that depend on the previous contracts
7. Verification and blocker list for Task 7
```

This order matters because Desktop cannot be merged correctly until Shared and API contracts are stable.

## 1. Project Boundaries

### Files To Check

```text
BoardGamesApp/BoardGames.Desktop/BoardGames.Desktop.csproj
BoardGamesApp/BoardGames.Shared/BoardGames.Shared.csproj
BoardGamesApp/BoardGames.Api/BoardGames.Api.csproj
```

### Required Final State

`BoardGames.Desktop.csproj`:

- keep `ProjectReference` to `BoardGames.Shared`;
- keep temporary `ProjectReference` to `ServerCommunication` if notification server support still needs it;
- do not add `BoardGames.Api`;
- do not add `BoardGames.Data`;
- keep old direct-DB Desktop files excluded if they are not the active final path.

`BoardGames.Shared.csproj`:

- keep no project reference to `BoardGames.Data`;
- keep `ProxyRepositories/**` excluded from compilation;
- keep old duplicate DTOs excluded if they are not the selected active contract.

`BoardGames.Api.csproj`:

- keep references to `BoardGames.Data` and `BoardGames.Shared`;
- keep `Legacy/**` excluded from compilation;
- keep main's PDF packages only if Task 6 payment/receipt generation still needs them.

### Merge Rule

Project references are architecture, so Tasks 1-6 win here. Main can add packages, but not forbidden project references.

## 2. Shared DTO And API Client Contracts

Shared must be resolved before API/Desktop.

### Account Profile

Conflict:

```text
BoardGamesApp/BoardGames.Shared/DTO/AccountProfileDTO.cs
```

Required resolution:

- keep `AccountProfileDTO` as the class name;
- keep `Guid Id`;
- keep `int? PamUserId`;
- keep username, display name, email, role, avatar URL, status, and profile fields;
- keep null-safe defaults from the Task 1/Task 3 side;
- main's copyright/style can be kept if desired.

Do not use the non-existing `AccountProfileDataTransferObject` name in final code. Any API/Desktop references to `AccountProfileDataTransferObject` must be changed to `AccountProfileDTO` or deliberately aliased in one place only.

### Login / Register

Required resolution:

- use `LoginDTO`;
- use `RegisterDTO`;
- do not keep `LoginDataTransferObject`;
- do not keep `RegisterDataTransferObject`.

Main is correct on this naming direction.

### Game DTOs

Conflicts:

```text
BoardGamesApp/BoardGames.Shared/DTO/GameDTO.cs
BoardGamesApp/BoardGames.Shared/DTO/GameDTO2.cs
BoardGamesApp/BoardGames.Shared/ProxyServices/IGameService.cs
BoardGamesApp/BoardGames.Shared/ProxyServices/GameService.cs
```

Required resolution:

- do not keep both `GameDTO` and `GameDTO2` as active final contracts;
- do not make Shared depend on `BoardGames.Data.Models.Game`;
- keep Task 4's API contracts as canonical for backend routes:
  - `GameSummaryDTO`;
  - `GameDetailDTO`;
  - `GameCreateDTO`;
  - `GameUpdateDTO`;
  - `GameSearchCriteriaDTO`.

If Desktop still needs the old card/list shape from main's `GameDTO`, keep it only as a temporary Shared-owned compatibility DTO and map it from the canonical API response. Do not use it as proof that the final backend should return raw legacy game shapes.

Recommended final direction:

```text
Filter/list screens -> GameSummaryDTO
Game details screen -> GameDetailDTO
Create game -> GameCreateDTO
Edit game -> GameUpdateDTO
Legacy Desktop pages still using GameDTO -> temporary compatibility adapter only
```

### Message DTO

Conflict:

```text
BoardGamesApp/BoardGames.Shared/DTO/MessageDTO.cs
```

Required resolution:

- use one final message DTO name;
- recommended name is `MessageDTO`, because most main/current chat code expects it;
- keep `MessageType` in `BoardGames.Shared.DTO`;
- do not import `BoardGames.Data.Enums.MessageType` into Shared;
- if Task 6 code currently uses `MessageDataTransferObject`, rename or adapt it to the chosen `MessageDTO`.

Bad result:

```csharp
using BoardGames.Data.Enums;
```

inside `BoardGames.Shared`.

Good result:

```csharp
using BoardGames.Shared.DTO;
```

and `MessageDTO.Type` uses the Shared `MessageType` enum.

### Conversation DTO

Current problem:

```text
BoardGamesApp/BoardGames.Shared/DTO/ConversationDTO.cs
```

uses `ConversationParticipant`, which is a Data model.

Required resolution:

- replace `ConversationParticipant` with a Shared transport DTO, for example `ConversationParticipantDTO`;
- include only transport fields needed by Desktop/Web, such as account id, optional `PamUserId`, display name, avatar URL, and last-read time;
- keep entity-to-DTO mapping inside API services/mappers.

### Address Validator

Current problem:

```text
BoardGamesApp/BoardGames.Shared/Validators/AddressValidator.cs
```

uses `Address`, which is a Data model.

Required resolution:

- either create a Shared `AddressDTO` and validate that;
- or move `AddressValidator` out of Shared and into Desktop if it is only a Desktop UI validator;
- do not make Shared reference `BoardGames.Data.Models.Address`.

## 3. API Controllers And Services

### General API Rule

For API conflicts, the Task 1-6 side is canonical when it:

- uses `BoardGames.Shared.DTO`;
- uses `Guid AccountId` as public identity;
- injects a service interface;
- calls services instead of repositories;
- owns the final route group from the audit.

Main's API side often contains older controllers that inject repositories directly. Those must not become active final controllers again.

### Duplicate Controllers And Services

Conflicts include:

```text
Controllers/GamesController.cs
Controllers/GamesController2.cs
Controllers/RentalsController.cs
Controllers/RentalsController2.cs
Controllers/UsersController.cs
Controllers/UsersController2.cs
Services/UserService.cs
Services/UserService2.cs
Services/RentalService.cs
Services/RentalService2.cs
Services/IUserService.cs
Services/IUserService2.cs
Services/IRentalService.cs
Services/IRentalService2.cs
```

Required resolution:

- keep one active non-`2` file per business concept;
- do not restore active `*2.cs` duplicates;
- keep legacy behavior only under `BoardGames.Api/Legacy/**`, which is excluded from compilation;
- if main has useful behavior inside a deleted `*2.cs` file, copy the behavior into the canonical service later or record it as a follow-up. Do not keep both files active.

### Auth / Account / Admin

Conflicts:

```text
Controllers/AuthController.cs
Controllers/AccountsController.cs
Controllers/AdminController.cs
Services/AuthService.cs
Services/AccountService.cs
Services/AdminService.cs
Mappers/AccountProfileMapper.cs
```

Required resolution:

- keep project 1 account/auth/admin as canonical;
- use `api/auth`, `api/accounts`, `api/admin`;
- preserve `PamUserId` in login/profile mapping;
- use `LoginDTO`, `RegisterDTO`, `AccountProfileDTO`, `UpdateProfileDTO`;
- do not restore old `api/users/login` or `api/users/register` as final routes;
- admin authorization must remain an API responsibility, even if Task 7 still needs to wire middleware.

Main DTO-name corrections are useful. Main direct/repository user flows are not.

### Games / Filter / Search

Conflicts:

```text
Controllers/GamesController.cs
Services/GameService.cs
Services/IGameService.cs
Mappers/GameMapper.cs
Services/GameInputHelper.cs
Data/Repositories/GameRepository2.cs
```

Required resolution:

- keep Task 4's canonical `api/games` service-controller path;
- do not restore main's repository-injected `GamesController`;
- do not return raw `BoardGames.Data.Models.Game` from API final routes;
- preserve main's Discovery/filter needs by mapping them to canonical `api/games` or `api/games/search`;
- do not revive old feed endpoints unless Task 4 explicitly decides they are still required.

Main's old endpoints:

```text
GET api/games/feed/tonight
GET api/games/feed/remaining
```

should not be restored automatically. Task 4 already documents that feed behavior should be covered through the canonical search/filter API unless the team explicitly chooses otherwise.

### Requests / Rentals

Conflicts:

```text
Controllers/RequestsController.cs
Controllers/RentalsController.cs
Services/RequestService.cs
Services/RentalService.cs
Services/IRequestService.cs
Services/IRentalService.cs
Mappers/RequestMapper.cs
Mappers/RentalMapper.cs
Data/Models/Rental.cs
```

Required resolution:

- keep Task 5's project 2-style lifecycle as canonical;
- keep `api/requests` and `api/rentals` service-layer routes;
- keep create request, validate dates, owner-cannot-rent, availability, approve, deny, cancel, and confirmed rental creation;
- preserve DTO naming from main when it matches active Shared DTO files:
  - `CreateRequestDTO`;
  - `RequestActionDTO`;
  - `BookedDateRangeDTO`;
- do not restore raw Data entity request/rental bodies as final API contracts;
- do not let old standalone request/rental pages define backend architecture.

For `Data/Models/Rental.cs`, keep Task 5/6 additions such as the `Renter` alias if services depend on it. Main's style changes can be kept, but they must not remove fields needed by request/rental/payment/chat mapping.

### Conversations / Notifications / Payments

Conflicts:

```text
Controllers/ConversationController.cs
Controllers/NotificationsController.cs
Controllers/PaymentsController.cs
Services/ConversationService.cs
Services/ConversationApiService.cs
Services/NotificationService.cs
Services/ServicePayment.cs
Services/PaymentService.cs
```

Required resolution:

- keep Task 6's rule that request/rental events produce chat and notification side effects from the same source of truth;
- keep service-layer conversation/payment APIs where Task 6 introduced them;
- do not restore repository-injected final controllers from main;
- do not use `BoardGames.Data.Enums.MessageType` in Shared DTOs;
- if main has UI expectation around chat/payment routes, map those expectations to canonical Task 6 routes.

Bad result:

```text
request created through Task 5
but notification still comes from old project 1 request pages
and chat still uses unrelated project 2 message flow
```

Good result:

```text
request created
-> same request id creates/updates chat message
-> same request id creates notification
-> accept/decline finalizes same message and notification path
```

## 4. Desktop Startup, Namespace, And Session

### Startup Files

Conflicts:

```text
BoardGamesApp/BoardGames.Desktop/App.xaml
BoardGamesApp/BoardGames.Desktop/App.xaml.cs
BoardGamesApp/BoardGames.Desktop/App.xaml2.cs
BoardGamesApp/BoardGames.Desktop/MainWindow.xaml
BoardGamesApp/BoardGames.Desktop/MainWindow.xaml.cs
BoardGamesApp/BoardGames.Desktop/MainWindow2.xaml
BoardGamesApp/BoardGames.Desktop/MainWindow.xaml2.cs
```

Required resolution:

- keep the Task 1 HTTP-client startup logic;
- do not restore main's Desktop startup that constructs `AppDbContext`, repositories, and API services directly;
- keep one active app/window path;
- keep one root navigation frame;
- keep `AddBoardRentApiClient` / Shared proxy-service registration as the Desktop API path.

Namespace decision:

- final namespace should trend toward `BoardGames.Desktop` for a singular application identity;
- if the merge switches `App.xaml` and `MainWindow.xaml` to `BoardGames.Desktop.App` / `BoardGames.Desktop.MainWindow`, port the Task 1 startup logic into that namespace;
- do not switch namespace by restoring the old direct-DB startup implementation.

In other words:

```text
Accept main's naming direction if useful.
Reject main's old startup architecture.
```

### Initial Screen And Navigation

Main contains useful redirection/navigation work in Discovery:

```text
Login
Games
Notifications
Dashboard
Chat
Account
```

Required resolution:

- preserve these navigation intentions;
- wire them later through the final Desktop shell/session/navigation service;
- unauthenticated startup should eventually open Filter / Discovery, not the old home page;
- login should be required when the user tries to rent, chat, see dashboard, see notifications, or access account;
- admin-only account administration must depend on session role, not only hidden UI buttons.

Do not preserve main's old user-switching/demo `SessionContext.GetInstance().UserId` behavior as the final session model.

### Desktop Session

Conflicts and problem files:

```text
BoardGamesApp/BoardGames.Desktop/Helpers/AuthSession.cs
BoardGamesApp/BoardGames.Desktop/Services/ISessionContext.cs
BoardGamesApp/BoardGames.Desktop/Services/SessionContext.cs
BoardGamesApp/BoardGames.Desktop/Services/DesktopAuthorizationService.cs
BoardGamesApp/BoardGames.Data/Enums/SessionContext.cs
```

Required resolution:

- keep `BoardGames.Desktop.Services.ISessionContext` / `SessionContext` as the final Desktop session contract;
- fix it to use `AccountProfileDTO`;
- keep `AccountId`;
- keep optional `PamUserId`;
- keep role/status/profile fields;
- do not use `BoardGames.Data.Enums.SessionContext` as the final Desktop auth state;
- do not keep `Helpers/AuthSession` as a second active session system.

If old project 2 screens still require integer `PamUserId`, bridge it from `ISessionContext.PamUserId`. Do not create a separate logged-in state.

### Desktop Feature Pages

Main's Desktop page changes are useful mostly when they:

- remove old namespaces;
- add navigation buttons;
- redirect from Discovery to the correct pages;
- add login-required checks.

They are not useful when they:

- depend on `App.SearchAndFilterService` from Desktop-created services;
- depend on `SessionContext.GetInstance()` from Data;
- depend on `App.UserRepository`, `App.GameRepository`, or other Desktop static repository/service fields;
- route to old pages that Tasks 5/6 declared legacy.

For old request/rental pages:

```text
CreateRentalView
CreateRequestView
RequestsFromOthersPage
RequestsToOthersPage
RentalsFromOthersPage
RentalsToOthersPage
```

do not let these pages drive the final flow. They may remain temporarily if the project still needs compatibility, but they must consume canonical `api/requests` and `api/rentals` routes and must not become a second request/rental system.

## 5. Data Layer

Only two Data files are currently conflicted:

```text
BoardGamesApp/BoardGames.Data/Models/Rental.cs
BoardGamesApp/BoardGames.Data/Repositories/GameRepository2.cs
```

Required resolution:

- preserve fields/aliases needed by Task 5/6 request/rental/payment/chat mapping;
- do not change database schema just for style;
- do not revive duplicate repositories as active final API dependencies unless the canonical API service actually requires them;
- main's style/copyright changes are safe only if they do not remove Task 5/6 behavior.

Data is not the place to solve Desktop navigation or API route duplication.

## 6. What To Do With Main's Useful Changes

Main change | Merge action
--- | ---
Namespace/import cleanup | Keep where it points to final namespaces and does not restore forbidden dependencies.
Discovery default first screen | Preserve as final navigation intent, but wire through Task 1/Task 8 Desktop shell.
Login/Games/Notifications/Dashboard/Chat/Account buttons | Preserve the intent; connect to final pages/session guards.
DTO name corrections | Keep, especially `LoginDTO`, `RegisterDTO`, `CreateRequestDTO`, `RequestActionDTO`, `BookedDateRangeDTO`.
Direct `AppDbContext` in Desktop startup | Reject.
Desktop static repositories/services | Reject as final active path.
API repository-injected controllers | Reject as final active path.
Shared `using BoardGames.Data.*` | Reject.
Old integer session singleton | Reject as final session; bridge only through `PamUserId` if unavoidable.

## 7. Practical Conflict Resolution Strategy

Recommended process for the merge worker:

1. Resolve `.csproj` files first and confirm boundaries.
2. Resolve Shared DTOs so all projects agree on names.
3. Resolve API duplicate files by preserving canonical Task 2 decisions.
4. Resolve API feature files area by area:
   - auth/account/admin;
   - games/filter/search;
   - requests/rentals;
   - chat/notifications/payments.
5. Resolve Desktop startup by porting Task 1 active logic into the final namespace/class shape.
6. Resolve Desktop navigation by preserving main's useful buttons but connecting them to the final session/shell direction.
7. Only after conflicts are removed, run static checks for forbidden dependencies.
8. Only after static checks pass, run build in dependency order:

```powershell
dotnet build BoardGamesApp\BoardGames.Shared\BoardGames.Shared.csproj
dotnet build BoardGamesApp\BoardGames.Data\BoardGames.Data.csproj
dotnet build BoardGamesApp\BoardGames.Api\BoardGames.Api.csproj
dotnet build BoardGamesApp\BoardGames.Desktop\BoardGames.Desktop.csproj
```

Do not start by building the full solution with tests. Tests are not the merge gate for this department.

## 8. Required Static Checks After Merge

Run these checks after conflicts are resolved:

```powershell
rg -n "using BoardGames\.Data|BoardGames\.Data" BoardGamesApp\BoardGames.Shared
rg -n "ProjectReference Include=\"\.\.\\BoardGames\.Data|ProjectReference Include=\"\.\.\\BoardGames\.Api" BoardGamesApp\BoardGames.Desktop\BoardGames.Desktop.csproj
rg -n "AppDbContext|DatabaseBootstrap|ProxyRepositories|BoardGames\.Data\.Enums\.SessionContext" BoardGamesApp\BoardGames.Desktop
rg -n "<<<<<<<|=======|>>>>>>>" BoardGamesApp
rg -n "Controller.*Repository|IConversationRepository|IPaymentRepository" BoardGamesApp\BoardGames.Api\Controllers
rg -n "AccountProfileDataTransferObject|LoginDataTransferObject|RegisterDataTransferObject|MessageDataTransferObject" BoardGamesApp
```

Expected result:

- no conflict markers;
- no Shared reference to Data;
- no Desktop direct Data/API reference;
- no Desktop startup dependency on `AppDbContext`;
- no active API controller exposing repositories directly, except files explicitly assigned to follow-up if Task 6 did not complete;
- no references to DTO names that do not exist.

## 9. Build Expectation After This Merge

This merge should make Tasks 1-6 coherent again and should prepare the code for Task 7.

The merge worker should aim for:

```text
BoardGames.Shared builds
BoardGames.Data builds
BoardGames.Api builds or has only Task 7 DI/runtime blockers
BoardGames.Desktop restores/builds enough to expose remaining Task 8-10 work
```

If `BoardGames.Api` fails because `Program.cs` lacks dependency injection, that is Task 7.

If `BoardGames.Shared` fails because DTOs still depend on Data, that is not a Task 7 runtime problem. It is a merge/contract problem that must be fixed before Task 7 can do meaningful backend wiring.

If `BoardGames.Desktop` fails because feature pages still call old static Desktop services, that belongs to Task 8-10 unless those calls are in the active startup/session path.

## 10. Final Acceptance Criteria For The Merge

The merge is acceptable when:

- the active project references still respect Task 1 boundaries;
- Tasks 2-6 canonical API route/service decisions are not overwritten by main;
- main's useful Desktop navigation intent is preserved;
- main's useful DTO/import fixes are preserved;
- no duplicate active API controller/service versions remain;
- Shared no longer imports Data types;
- Desktop has one active session direction;
- Desktop startup does not create repositories or `AppDbContext`;
- old request/rental pages do not define a second final backend workflow;
- all unresolved conflicts are gone;
- any remaining build failures are clearly assigned to Task 7, Task 8-10, Web/Shared, DB seed, or tests.

## Short Decision Summary

Use this rule when resolving individual conflicts:

```text
Architecture and backend ownership -> keep Tasks 1-6.
DTO naming fixes and namespace cleanup -> usually keep main.
Desktop route/button intent -> keep main intent, adapt to Task 1 session/shell.
Direct DB/repository use in Desktop -> reject.
Repository-injected API controllers -> reject.
Shared Data imports -> reject.
Duplicate *2 active files -> reject or keep only under Legacy/excluded path.
```

