# Task 8: Desktop App Shell, API Client Config, And Auth Session

**Workflow source:** Section 4, Desktop Single Shell and part of Section 5, Desktop Feature Integration  
**Type:** Desktop setup gate  
**Can start lightly after:** Task 1  
**Final wiring can start after:** Task 7  
**Can run in parallel with:** Tasks 9 and 10 after Task 7, but this provides their session/API foundation  
**Suggested owner:** Desktop architecture worker  
**Primary project area:** `BoardGames.Desktop`  
**Secondary coordination area:** `BoardGames.Shared` API clients and DTOs needed for login/session/profile state

## What This Task Is About

This task makes the WinUI application behave like one application with one startup path, one root shell, one API base URL, and one logged-in session.

The final Desktop application should not look like two old projects placed beside each other. It should open into the final browsing experience, allow anonymous users to browse games, allow users to log in through the unified API, and then keep that same session available to Filter, Game Details, Chat, Notifications, Dashboard, Games, Account, and Admin.

This task does not replace the existing routing person's task. It defines the Desktop shell and session foundation. The routing person can still handle detailed button-to-page navigation. This task should make sure there is one app container and one session source for those routes to use.

## Related Audit Context

Helpful audit documents exist in `docs/audits`. They explain how this task relates to the other tasks in the `.Desktop + .Api` task set and should be read before implementation:

- `docs/audits/desktop-api-final-workflow.md`
- `docs/audits/desktop-api-contract-audit.md`
- `docs/audits/desktop-api-department-task-breakdown.md`
- `docs/audits/desktop-api-10-task-assignment-plan.md`
- `docs/audits/task-1-architecture-boundary-identity-contract.md`
- `docs/audits/task-1-boundary-violation-inventory.md`
- `docs/audits/task-7-api-runtime-wiring-analysis.md`

This task corresponds to Task 8 in the 10-task plan. It belongs to the Desktop setup lane after the local API runtime gate is stable enough.

This task should not redo Task 3 auth/account API work, Task 4 games API work, Task 5 request/rental API work, Task 6 chat/notification/payment API work, Task 7 backend runtime wiring, Task 9 Filter/Game Details integration, or Task 10 remaining Desktop feature integration. It should provide the app shell, API-client configuration, and session foundation those tasks consume.

## Where This Fits In The Workflow

The intended department workflow is:

```text
Task 1 -> Task 2 -> Tasks 3 / 4 / 5 / 6 -> Task 7 -> Tasks 8 / 9 / 10
```

Task 8 can begin light structural investigation after Task 1 because Task 1 defines the dependency and identity rules.

Final implementation should wait until Task 7 provides:

- the local API base URL;
- the final runtime configuration;
- a working login route;
- the route groups needed by Desktop feature pages;
- a known local database setup path.

Task 8 can run beside Tasks 9 and 10 after Task 7, but Tasks 9 and 10 should not invent their own shell, API base URL, or session. They should consume the result of this task.

Routing work and DB seed/setup work are assumed to be handled by separate owners. This task may define where Desktop should start and what navigation states should exist, but it should not become the routing task or the seed task.

## Main Goal

Transform the Desktop application from this:

```text
multiple app/startup paths
multiple main windows
old direct service/repository startup code
unclear API URL configuration
hardcoded remote IP fallback
old integer/static users in Desktop flow
old SessionContext and newer SessionContext both visible
login/register paths split between old project pages and newer API-client pages
feature pages guessing their own user identity
```

Into this:

```text
one active App.xaml/App.xaml.cs path
one active MainWindow path
one root Frame
one shell container
Filter is the first visible screen
anonymous and logged-in shell states are clearly defined
one local API base URL configuration is used
Shared ProxyServices API clients are registered through DI
one Desktop session context stores the logged-in account
login/register use the unified API contract
login returns the user to Filter or the agreed shell route
feature pages read user identity from the same session
```

## Current State From The Codebase

The current Desktop project already contains pieces of the final direction, but they are not fully active as one clean startup path.

Relevant project file:

- `BoardGamesApp/BoardGames.Desktop/BoardGames.Desktop.csproj`

The project currently references:

- `BoardGames.Shared`
- `ServerCommunication`

It does not currently reference `BoardGames.Api` or `BoardGames.Data`, which matches the final dependency direction. However, the active and excluded Desktop files still contain old startup patterns, old namespaces, and old session logic that must be resolved by this task or consumed later by Tasks 9 and 10.

Current startup files:

- `BoardGamesApp/BoardGames.Desktop/App.xaml`
- `BoardGamesApp/BoardGames.Desktop/App.xaml.cs`
- `BoardGamesApp/BoardGames.Desktop/App2.xaml`
- `BoardGamesApp/BoardGames.Desktop/App.xaml2.cs`
- `BoardGamesApp/BoardGames.Desktop/MainWindow.xaml`
- `BoardGamesApp/BoardGames.Desktop/MainWindow.xaml.cs`
- `BoardGamesApp/BoardGames.Desktop/MainWindow2.xaml`
- `BoardGamesApp/BoardGames.Desktop/MainWindow.xaml2.cs`

The project file currently excludes the newer-looking `App.xaml2.cs`, `MainWindow.xaml2.cs`, `App2.xaml`, and `MainWindow2.xaml` path. The active `App.xaml.cs` still contains old startup behavior, including direct creation of API proxy repositories, direct service objects, database bootstrap calls, and static global service properties.

The active `App.xaml.cs` contains:

- `BaseApiUrl = "http://localhost:5000/api/"`
- `RemoteApiUrl = "http://172.30.250.124:5000/api/"`
- static `HttpClient`
- `AppDbContext` creation
- repository proxy creation
- old service creation
- `DatabaseBootstrap.Initialize()`

That is not the final Desktop direction. Desktop should configure Shared API clients and session state. It should not own database setup or API service construction.

The newer `App.xaml2.cs` contains more of the final direction:

- `ServiceCollection`
- `AddBoardRentApiClient`
- `ISessionContext`
- `ICurrentUserContext`
- `IDesktopAuthorizationService`
- notification-related services
- view model registrations
- `App.NavigateTo`
- `App.OnUserLoggedIn`
- `App.OnUserLoggedOut`

However, this newer path is currently excluded from compilation and still needs cleanup before it can be considered final. It reads `ApiBaseUrl` from `App.config`, but the expected config file is not clearly present in the current Desktop project. Its launch behavior currently navigates to `LoginPage`, while the final workflow should open to Filter for anonymous browsing.

The active `MainWindow.xaml.cs` still has hardcoded demo users:

```text
loggedInUserAlice
loggedInUserBob
```

Those static users are not a final session model. Any remaining feature page that needs a user must read from the final Desktop session.

The XAML/code-behind pairing also needs attention. The current active XAML and code-behind paths do not all clearly point to the same namespace/class path. The task owner must choose one active app identity and one active main window identity, then make the project file, XAML `x:Class`, and code-behind namespaces agree.

Current session-related files:

- `BoardGamesApp/BoardGames.Desktop/Services/ISessionContext.cs`
- `BoardGamesApp/BoardGames.Desktop/Services/SessionContext.cs`
- `BoardGamesApp/BoardGames.Desktop/Services/ICurrentUserContext.cs`
- `BoardGamesApp/BoardGames.Desktop/Services/CurrentUserContext.cs`
- `BoardGamesApp/BoardGames.Desktop/Services/DesktopAuthorizationService.cs`
- `BoardGamesApp/BoardGames.Desktop/Services/IDesktopAuthorizationService.cs`
- `BoardGamesApp/BoardGames.Desktop/Services/AppRoles.cs`

These files are the best current foundation for the final Desktop session. The session stores:

- account id;
- optional legacy `PamUserId`;
- username;
- display name;
- email;
- role;
- avatar URL;
- suspension/lock state;
- phone and address/profile fields.

This matches the identity direction from Task 1 and the auth/profile contract from Task 3.

Older session usage still appears in Desktop feature areas:

- `BoardGames.Data.Enums.SessionContext`
- `SessionContext.GetInstance().UserId`
- static hardcoded users from `MainWindow`
- helper paths such as `BoardGamesApp/BoardGames.Desktop/Helpers/AuthSession.cs`

Task 8 owns the final Desktop session source. Tasks 9 and 10 own converting their assigned feature pages to consume that source.

Current Shared API-client direction:

- `BoardGamesApp/BoardGames.Shared/ProxyServices/ServiceCollectionExtensions.cs`
- `BoardGamesApp/BoardGames.Shared/ProxyServices/IAuthService.cs`
- `BoardGamesApp/BoardGames.Shared/ProxyServices/IAccountService.cs`
- `BoardGamesApp/BoardGames.Shared/ProxyServices/IAdminService.cs`
- `BoardGamesApp/BoardGames.Shared/ProxyServices/IGameService.cs`
- `BoardGamesApp/BoardGames.Shared/ProxyServices/IRequestService.cs`
- `BoardGamesApp/BoardGames.Shared/ProxyServices/INotificationService.cs`

The final Desktop app should use these Shared `ProxyServices` API clients through dependency injection. It should not use old repository-shaped proxy repositories as the final Desktop architecture.

There is also a current Desktop package restore/build risk:

- `H.NotifyIcon.WinUI` version `2.4.1` is currently referenced while the Desktop project targets `net8.0-windows10.0.19041.0`.

If this blocks Desktop startup or build verification for this task, the owner should fix it only as part of making the Desktop shell runnable. They should not expand the task into unrelated feature repairs.

## Owned Desktop Areas

This task owns:

```text
Desktop app entry point
Desktop main window
root Frame
shell container choice
anonymous vs logged-in shell behavior
Desktop API base URL configuration
Shared API-client registration
Desktop session context
login/register session population
logout session clearing
authorization checks for shell-level access
```

This task may touch Shared only when required to use existing Shared API-client contracts correctly. It should not redesign broad Shared DTO architecture.

This task may touch API only to coordinate a missing auth/profile contract with Task 3 or a missing local URL/runtime dependency with Task 7. It should not implement API feature behavior.

## Required Startup Behavior

The final Desktop startup should be:

```text
open Desktop
-> create one main window
-> create one root Frame
-> configure Shared API clients with the local API base URL
-> create one session context
-> show Filter/Discovery as the first screen
```

Filter must be the first user-visible workflow because the final application allows anonymous browsing. The old Home page is not the final first screen.

The app shell should clearly support two states:

```text
anonymous user
-> can browse Filter
-> can open Game Details
-> sees Login/Register entry point
-> cannot submit protected actions

logged-in user
-> same Filter remains available
-> protected menu sections become available
-> request/chat/notifications/dashboard/games/account/admin use the same session
```

The shell may show different navigation options depending on session state, but it should not create two separate applications or two separate root windows.

## Required API Client Configuration

Desktop should have one final local API base URL source.

The final path should not depend on:

- `http://172.30.250.124:5000/api/`;
- a deployed backend;
- a hardcoded remote IP;
- static `HttpClient` construction scattered through views or view models.

The expected direction is:

```text
Desktop config
-> one API base URL
-> Shared AddBoardRentApiClient
-> injected Shared ProxyServices clients
-> API routes from Tasks 3-7
```

If the project uses `App.config`, then the file must be present, copied to output if needed, and documented for local runs. If the project chooses another configuration source, it must still be one clear source that Tasks 9 and 10 can rely on.

The task owner must coordinate the final local URL with Task 7. Do not guess between `http://localhost:5000`, `http://localhost:5018`, `https://localhost:7125`, or any other port without checking the current backend runtime configuration.

## Required Session Contract

The final Desktop session must store the account identity returned by the auth/profile API.

Required fields:

```text
IsLoggedIn
AccountId
PamUserId, only if active legacy chat/rental/payment code still needs it
Username
DisplayName
Email
Role
AvatarUrl
IsSuspended
IsLocked
PhoneNumber
Country
City
StreetName
StreetNumber
```

The final session should be represented by one Desktop-owned service, not by EF models, repositories, static users, or old project-specific singleton state.

The current `BoardGames.Desktop.Services.ISessionContext` and `SessionContext` are the preferred foundation unless the implementer finds a clear blocking reason. If they are changed, the final shape must still satisfy the Task 1 identity contract and Task 3 login/profile contract.

The final Desktop session must be usable by:

- Filter and Game Details;
- rental request submission;
- Chat;
- Notifications;
- Dashboard/payment history;
- Games/My Games/Admin Games;
- Account/Profile;
- Admin.

Task 8 does not need to wire every feature page. It must provide the one session source that those pages use.

## Required Login, Register, And Logout Behavior

Login and register must use the unified API through Shared API clients.

Required login flow:

```text
user opens Login
-> Desktop calls api/auth/login through Shared auth client
-> API returns account/profile data
-> Desktop populates one session context
-> Desktop returns to Filter or agreed logged-in shell route
```

Required register flow:

```text
user opens Register
-> Desktop calls api/auth/register through Shared auth client
-> Desktop either logs in/populates session if API contract supports it
   or routes user back to Login with a clear success message
```

Required logout flow:

```text
user logs out
-> Desktop clears the session context
-> protected shell sections become inaccessible
-> user returns to anonymous Filter or Login depending on final shell decision
```

The important rule is that login/register/logout must not create a second identity system. They must use the same session context consumed by Tasks 9 and 10.

## Current Problems This Task Addresses

The current Desktop state has:

- two visible app identity paths;
- two visible main window paths;
- old and newer startup code living side by side;
- old direct database/service construction in the active startup path;
- hardcoded local and remote API URLs;
- unclear configuration for the final API base URL;
- static demo users in the active main window path;
- old integer session usage in feature pages;
- newer account-session services that exist but are not yet the obvious single app-wide source;
- login/register flows that are not clearly the only final auth flow;
- feature pages that can still rely on old static or integer user state.

This blocks Tasks 9 and 10 because Desktop feature workers cannot safely know:

- which app startup path is final;
- which frame they should navigate in;
- which session object is final;
- which API base URL they should use;
- whether anonymous browsing starts at Filter or Login;
- whether old request/rental/chat pages should use hardcoded users.

## Coordination With Other Tasks

Task 3, Auth/Account/Admin API:

- provides login/register/profile/logout routes;
- provides profile fields needed by the session;
- provides role and account status fields.

Task 7, API Runtime Wiring And Local Backend Smoke:

- provides local API URL;
- confirms API starts locally;
- confirms login route and route groups can be called.

Task 9, Desktop Filter/Game Details/Rental Request:

- consumes the first-screen Filter decision;
- consumes anonymous/logged-in checks;
- consumes `AccountId` and optional `PamUserId` from the session;
- uses configured Shared game/request API clients.

Task 10, Desktop Chat/Notifications/Games/Account/Admin/Dashboard:

- consumes the session role for Admin visibility;
- consumes session identity for Chat, Notifications, Dashboard, Games, and Account;
- consumes configured Shared API clients.

Routing owner:

- owns detailed button-to-page navigation;
- consumes the root shell and frame decisions from this task;
- should not need to create a second app shell.

DB seed/setup owner:

- owns local demo data and credentials;
- may provide usernames/passwords for testing login;
- does not own Desktop shell/session code.

## Implementation Hints

Choose one final startup path and make the project file, XAML, and code-behind agree.

The newer `App.xaml2.cs` direction has useful DI/session/API-client setup, but it is currently excluded and should not be blindly activated without cleanup. The active `App.xaml.cs` is not the final architecture because it creates database/service/repository objects directly.

The task owner should inspect and decide how to preserve the useful newer startup behavior while removing the old direct database/service path from the final runtime.

Use `BoardGames.Shared.ProxyServices.ServiceCollectionExtensions.AddBoardRentApiClient` for API clients unless Task 7 or the Shared owner has replaced it with a newer agreed registration method.

Use the existing Desktop session services if possible:

- `BoardGames.Desktop.Services.ISessionContext`
- `BoardGames.Desktop.Services.SessionContext`
- `BoardGames.Desktop.Services.ICurrentUserContext`
- `BoardGames.Desktop.Services.CurrentUserContext`
- `BoardGames.Desktop.Services.DesktopAuthorizationService`

Clean up old aliases and namespaces only inside the assigned shell/session path. For example, Desktop files that still refer to `BoardGames.ApiClient` should be aligned with the actual Shared proxy service namespace if those files are part of this task's final active path.

Do not fix every old feature page that still uses `SessionContext.GetInstance().UserId`. Tasks 9 and 10 own those feature conversions. Task 8 should make the final session available and update only the login/shell/session code needed to prove the foundation.

## Expected Output

This task should produce:

- one active Desktop app entry point;
- one active Desktop main window;
- one root frame;
- one shell container decision;
- Filter as the first screen;
- clear anonymous vs logged-in shell behavior;
- one local API base URL configuration;
- Shared API clients registered and injectable;
- one Desktop session context;
- login/register flow connected to the API session contract;
- logout session clearing;
- no final hardcoded remote IP dependency;
- no final Desktop dependency on API internals, repositories, `AppDbContext`, or database bootstrap.

## What Counts As Done

Desktop starts through one chosen startup path.

The app opens to Filter/Discovery, not the old Home page and not a mandatory login screen.

There is one root frame and one shell container for routing to use.

The Desktop API base URL is configured from one local source and uses the Task 7 backend URL.

Desktop registers and uses Shared API clients instead of direct API/Data/repository construction.

Login calls the unified auth API and populates the one Desktop session.

Register uses the unified auth API and follows the agreed post-register behavior.

Logout clears the same Desktop session.

The session stores account id, optional legacy user id, username, display name, role, email, avatar/profile basics, and account status.

Feature tasks can consume the session and API clients without creating their own copies.

The final shell path does not rely on hardcoded Alice/Bob users or the hardcoded remote IP.

## Do Not Touch

Do not implement Task 9 Filter/Game Details/rental request behavior.

Do not implement Task 10 Chat, Notifications, Dashboard, Games, Account, Admin, or remaining page integrations.

Do not take over detailed routing button work owned by the routing person.

Do not implement API feature behavior from Tasks 3-7.

Do not create a second Desktop session just to satisfy one page.

Do not use `AppDbContext`, repositories, or API service classes directly from Desktop.

Do not reintroduce `BoardGames.Desktop -> BoardGames.Api` or `BoardGames.Desktop -> BoardGames.Data` project references.

Do not use hardcoded users to make feature pages appear logged in.

Do not make tests for this task unless the lead explicitly asks for tests.

Do not try to fix the whole solution build. Fix only errors directly related to making the Desktop shell, API-client configuration, and session foundation work. Document unrelated build errors or feature-page failures as blockers.

## Known Blockers And Assumptions

This task assumes Task 7 has produced a locally runnable API and a stable local API URL.

This task assumes Task 3 has produced login/profile data with the fields required by the Desktop session.

This task assumes routing and DB seed/setup are owned by separate people. The implementer should coordinate with them, not replace their work.

The current application may not build perfectly before this task starts. The implementer should not repair unrelated Web, API, test, or feature-page problems only to make the full solution green.

The Desktop project may currently have a package compatibility issue involving `H.NotifyIcon.WinUI`. If that prevents Desktop shell startup or build for this task, handle it as a scoped Desktop shell blocker and document the decision.

Old excluded pages and view models may still contain useful feature code. Do not delete useful behavior just because it is old. If a file belongs to Task 9 or Task 10, leave it for that owner and document the dependency.

The final proof of this task is not that every Desktop feature works. The final proof is that every Desktop feature has one shell, one configured API path, and one session source to build on.
