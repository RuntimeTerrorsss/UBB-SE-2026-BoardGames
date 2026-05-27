# Desktop and API Final Workflow Plan

**Purpose:** This document defines the order of work for the WinUI Desktop and ASP.NET Core API department so the merged board-game rental application becomes one coherent local application.

**Scope:** This covers the `.Desktop`, `.Api`, `.Shared`, and `.Data` boundaries that affect Desktop/API work. Web is mentioned only when it affects the shared backend contract.

**Final order:**

```text
1 -> 2 -> 3 -> 6 -> 4/5
```

This order matters. Desktop integration should not be the main work until the local unified backend exists. Desktop needs stable routes, stable DTOs, one user/session model, and one real database source.

---

## Current State Summary

The current project is a minimal merge of two older applications:

- Project 1 contributed login, account/profile, account administration, my games, notifications, and request/rental management pages.
- Project 2 contributed filter/discovery, game details, booking/rental request flow, chat, dashboard, payment history, and payment-related flows.

The merged repository currently contains both application shapes in the same solution, but they are not yet one unified application.

Observed current issues:

- `BoardGames.Shared` references `BoardGames.Data`, while `.Data` contains concepts that should be shared DTO/enum concepts. Adding `.Data -> .Shared` would create a dependency cycle.
- `BoardGames.Desktop` references `BoardGames.Api` directly, even though Desktop should run beside the API and call it over HTTP.
- `BoardGames.Api` contains duplicate controllers and services from both old projects.
- `BoardGames.Api/Program.cs` currently registers controllers and Swagger only. It does not register the database, repositories, business services, mappers, or authentication/session infrastructure.
- `BoardGames.Desktop` contains two application entry shapes: `BookingBoardGames` and `BoardRentAndProperty`.
- Desktop has two login/register flows and two session concepts.
- The filter/chat/dashboard flow uses old static or hardcoded users in places, while account/profile/admin uses the other account model.
- The intended duplicate pages are still visible in the old Desktop menu: My Rentals, Others' Rentals, My Requests, Others' Requests.

The department goal is not just "make it build". The goal is to make the Desktop and API behave as one local application backed by one database and one user identity.

---

## Target Product Workflow

The intended user workflow is:

```text
Open Desktop app
-> Filter page as anonymous user
-> Login button
-> Auth page
-> Filter page as logged-in user
```

After login, the navigation should expose:

- Games: my games, create game, edit game, delete/deactivate game.
- Notifications: rental request notifications and request accepted/declined notifications.
- Dashboard: payment history and payment-related status.
- Chat: conversations, rental request messages, payment-related conversation flow.
- Account: profile edit and password/avatar/account details.
- Admin-only section: account administration and all-games administration.

The intended rental flow is:

```text
Filter
-> click game
-> game details / rental section
-> choose date range
-> send rental request
-> owner receives chat message
-> owner receives notification
-> owner accepts or declines
-> renter receives notification
-> rental/payment/dashboard state updates
```

This must use the same logged-in user everywhere. The user in Account must be the same user in Chat, Filter, Notifications, Games, Dashboard, and Admin.

---

## 1. Project Boundary Setup

### What This Section Is About

This section defines the architecture direction. It prevents circular dependencies and makes the solution understandable before feature teams start wiring screens.

Target dependency direction:

```text
Desktop -> Shared DTOs/API clients -> Api -> Services -> Data -> Database
Web     -> Shared DTOs/API clients -> Api -> Services -> Data -> Database
```

Inside the API:

```text
Controllers -> Services -> Repositories -> AppDbContext -> SQL database
```

### Why It Must Be First

The current `.Shared` and `.Data` relationship is confused. `BoardGames.Shared` currently references `BoardGames.Data`, and some shared concepts also appear in data/model namespaces. If someone tries to fix missing imports by adding `BoardGames.Data -> BoardGames.Shared`, the solution creates a dependency cycle.

The team must first decide which project owns which concept:

- `.Shared` owns DTOs, API client interfaces, proxy services, API response wrappers, and transport enums used by both Desktop and Web.
- `.Data` owns EF Core models, repository interfaces/implementations, migrations, and `AppDbContext`.
- `.Api` owns controllers, business services, mappers between `.Data` models and `.Shared` DTOs, auth logic, and local runtime configuration.
- `.Desktop` owns WinUI pages, viewmodels, navigation, local session state, and calls to `.Shared` API clients.

### Required Decisions

- Desktop must not reference `.Api` directly.
- Desktop should not call repositories directly.
- Desktop should not create or migrate the database directly in the final runtime path.
- Shared DTOs should not require EF Core models.
- Data models should not require Shared DTOs.
- API is the only project that should know both DTOs and EF models.
- API routes must expose business services, not repositories.

### Department Outputs

At the end of section 1, the department should have:

- A written dependency rule that every team member follows.
- A decision for where duplicated DTOs/enums live.
- A decision for which ID is public in DTOs and which ID is internal to legacy data.
- A decision that Desktop calls local API over HTTP.
- A decision that local development uses one API base URL and one database.

### Parallelism

This section is mostly sequential. Everyone can discuss it, but one final dependency contract must be agreed before deeper work starts.

Allowed parallel work during this section:

- One person lists current project references.
- One person lists duplicate DTOs/enums.
- One person lists Desktop direct usages of repositories/API services.

The final decision must be sequential and shared with everyone.

---

## 2. API Cleanup

### What This Section Is About

This section makes `.Api` look like one backend instead of two APIs copied into the same namespace.

Current duplicated API artifacts include:

- `BoardGames.Api/Controllers/GamesController.cs`
- `BoardGames.Api/Controllers/GamesController2.cs`
- `BoardGames.Api/Controllers/RentalsController.cs`
- `BoardGames.Api/Controllers/RentalsController2.cs`
- `BoardGames.Api/Controllers/UsersController.cs`
- `BoardGames.Api/Controllers/UsersController2.cs`
- `BoardGames.Api/Services/UserService.cs`
- `BoardGames.Api/Services/UserService2.cs`
- `BoardGames.Api/Services/RentalService.cs`
- `BoardGames.Api/Services/RentalService2.cs`
- `BoardGames.Api/Services/IUserService.cs`
- `BoardGames.Api/Services/IUserService2.cs`
- `BoardGames.Api/Services/IRentalService.cs`
- `BoardGames.Api/Services/IRentalService2.cs`

Some controllers currently inject repositories directly. Others inject service-layer interfaces. The assignment requires the API to expose the service layer, not the repository layer.

### Required API Shape

The API should have one canonical route group per concept:

```text
api/auth
api/accounts
api/admin
api/games
api/search or api/games/search
api/requests
api/rentals
api/conversations
api/notifications
api/payments
```

Controllers should call services:

```text
Controller -> Service -> Repository
```

Controllers should not call:

```text
Controller -> Repository
```

### Feature Decisions

Auth/account/admin:

- Keep the login/register/profile/admin account system from project 1 as the canonical identity system.
- Remove the duplicated sign-in/sign-up path from project 2, because it duplicates project 1.
- Keep admin account administration.

Games:

- Keep one canonical games API for my games, admin all-games, create, edit, delete/deactivate, and game details.
- Ensure filter/discovery can use the same game data.

Requests/rentals:

- The user-facing rental flow should be the chat/request flow from project 2.
- Old standalone My Requests, Others' Requests, My Rentals, Others' Rentals pages should not drive the final Desktop navigation.
- API may still keep internal request/rental endpoints if chat, notifications, payments, and dashboard need them.

Chat/notifications:

- Chat request creation and notification creation must be connected.
- A rental request must produce both a chat message and a notification.
- Accept/decline must update request/rental state and notify the renter.

Dashboard/payment:

- Dashboard/payment history must read real payment/rental data through API services.

### Department Outputs

At the end of section 2, the department should have:

- One controller per route group.
- One service interface per service concept.
- One service implementation per service concept.
- No duplicate controller class names in the same namespace.
- No duplicate service class/interface names in the same namespace.
- Controllers calling service interfaces, not repositories.
- Old controllers either removed from compilation, renamed as legacy routes, or intentionally replaced.

### Parallelism

This section can be split in parallel after the boundary contract is agreed:

- Lane A: Auth, accounts, admin.
- Lane B: games, filter/search, requests, rentals.
- Lane C: conversations/chat, notifications, payments/dashboard.

Sequential gate:

- The lanes must agree on DTO names, route names, and ID rules before final cleanup is merged.

---

## 3. API Runtime Wiring

### What This Section Is About

This section makes `.Api` actually run as the local backend.

Current `BoardGames.Api/Program.cs` registers:

- Controllers.
- Swagger.

It does not currently register the real application dependencies needed by the controllers and services.

### Required Wiring

`Program.cs` must register:

- `AppDbContext`.
- SQL Server connection string.
- Repository interfaces and implementations.
- Business service interfaces and implementations.
- Mapper classes.
- Auth/password/security helpers.
- Avatar/file storage service if profile avatars remain supported.
- Conversation/chat helper services.
- Notification helper services.
- Payment/dashboard services.

The API must be constructable by dependency injection. Every active controller constructor must be satisfiable.

### Required Configuration

The API local config should define:

- local HTTP URL, for example `http://localhost:5018`;
- optional HTTPS URL if the team chooses to use HTTPS locally;
- one local connection string for `MergedBoardGamesDb`;
- development environment setup;
- Swagger in development.

The backend should not depend on a remote IP address for the local workflow.

### Required Route Verification

Swagger should expose the canonical routes only. If legacy routes are temporarily kept, they must be clearly named as legacy and not used by Desktop as the primary flow.

Expected route groups:

- Auth.
- Accounts/profile.
- Admin.
- Games.
- Filter/search.
- Requests/rentals.
- Conversations/chat.
- Notifications.
- Payments/dashboard.

### Department Outputs

At the end of section 3, the department should have:

- `.Api` locally startable.
- Swagger visible.
- Dependency injection complete for active controllers.
- One connection string strategy.
- One database strategy.
- No controller failing at runtime because a service/repository/mapper is missing.

### Parallelism

This section is mostly sequential because it depends on section 2 cleanup.

Allowed parallel support:

- One person prepares the DI list from repositories/services.
- One person prepares appsettings/launch profile.
- One person prepares endpoint smoke-check list.

Final API startup wiring must be integrated sequentially.

---

## 6. Local Unified Backend Setup

### What This Section Is About

This section replaces deployment work with local runtime setup. It is section 6 because it was originally the final end-to-end/deployment step, but for this team it must happen before serious Desktop integration.

The department is giving up on remote deploy for now. The objective is:

```text
Make .Api + one local database work as the unified backend on a developer machine.
```

### Why It Comes Before Desktop Work

Desktop integration depends on the backend. If Desktop starts before the local backend is stable, developers will wire pages to routes and DTOs that may be renamed or removed.

Desktop needs:

- stable API base URL;
- stable route names;
- stable DTOs;
- known login users;
- known admin user;
- real games data;
- real request/rental/chat/notification behavior;
- one database that both API and Web can use.

### Required Local Setup

The local backend setup should define:

- API startup project: `BoardGames.Api`.
- API local URL, for example `http://localhost:5018`.
- Local database name, for example `MergedBoardGamesDb`.
- SQL Server provider, likely LocalDB or SQL Server Express.
- Connection string location.
- Migration command.
- Seed/demo data approach.
- Known users and passwords for demo.
- Known admin account.
- Whether `NotificationServer` is required for the demo.

### Database Requirements

The local database must be the single source of truth for:

- users/accounts;
- roles;
- games;
- rentals;
- requests;
- conversations;
- messages;
- notifications;
- payments/payment history.

Dummy data should be replaced with useful local seed data that supports the real workflow:

- at least one standard user who owns games;
- at least one standard user who can rent games;
- at least one admin user;
- several active games;
- enough city/location data for filter;
- optional existing conversations/payments only if they help demo the feature.

### Required Local Backend Smoke Flow

Before Desktop integration, the API should support this local flow through Swagger/Postman/browser:

```text
Register or login user
-> get filter/games list
-> get game details
-> create rental request
-> verify chat/conversation message exists
-> verify owner notification exists
-> approve or decline request
-> verify renter notification exists
-> verify rental/payment/dashboard data updates as expected
```

### Department Outputs

At the end of section 6, the department should have:

- A working local API.
- A working local DB.
- A written local setup guide.
- Known demo credentials.
- Stable routes for Desktop.
- Stable DTOs for Desktop.
- Proof that the backend workflow exists before Desktop screens call it.

### Parallelism

This section comes after sections 1, 2, and 3. It is a sequential gate before real Desktop integration.

Allowed parallel work:

- One person verifies DB/migrations.
- One person verifies Swagger endpoints.
- One person documents local setup and demo credentials.

Final gate:

- Desktop feature wiring should start only after the local backend smoke flow is accepted.

---

## 4. Desktop Single Shell

### What This Section Is About

This section makes WinUI look like one application instead of two old apps glued together.

Current Desktop contains:

- `BookingBoardGames.App` and `BookingBoardGames.MainWindow`.
- `BoardRentAndProperty.App` and `BoardRentAndProperty.MainWindow`.
- old `LoginView` and newer `LoginPage`.
- old `RegisterView` and newer `RegisterPage`.
- filter/chat/dashboard pages from project 2.
- account/admin/my-games pages from project 1.

The final application should have one startup path, one window, one shell, and one navigation model.

### Target Desktop Startup Flow

```text
Open app
-> Filter page as anonymous user
-> user can browse/filter games
-> Login button opens Auth page
-> successful login returns to Filter page
-> logged-in navigation becomes available
```

### Target Navigation

Logged-in navigation should contain:

- Games.
- Notifications.
- Dashboard.
- Chat.
- Account.
- Admin section only for administrators.

Navigation should not contain final user-facing entries for:

- My Rentals.
- Others' Rentals.
- My Requests.
- Others' Requests.

Those workflows are replaced by chat/request/notification/dashboard flows.

### Required Desktop Shell Decisions

- Choose one active `App.xaml` and one active `MainWindow`.
- Choose one root `Frame`.
- Choose one menu shell.
- Decide how anonymous pages and authenticated pages share the same shell.
- Decide where Login/Register/Auth is shown.
- Decide whether the left navbar appears before login, after login, or with limited anonymous items.
- Decide where Admin appears for admin users.

### Department Outputs

At the end of section 4, the department should have:

- One Desktop app entry point.
- One main window.
- One root navigation frame.
- One menu shell.
- Filter as the first screen.
- Duplicate old pages removed from the visible flow.
- Login/register flow unified.
- Admin-only navigation based on the same logged-in user role.

### Parallelism

Section 4 can run in parallel with section 5 after the local backend is stable.

It can be lightly designed earlier, but final navigation wiring should wait until section 6 is complete.

---

## 5. Desktop Feature Integration

### What This Section Is About

This section connects Desktop screens to the real local API and same session identity.

It should not start deeply until section 6 gives Desktop a stable local backend.

### Required Integration Rules

Desktop should:

- call `.Shared` API client/proxy services;
- use configured local API base URL;
- use one session context;
- never use hardcoded Bob/Carol/Alice user switching in the final flow;
- not call repositories directly;
- not call `.Api` classes directly;
- not own database migrations or seeding in the normal runtime flow.

### Required Feature Flows

Anonymous filter:

```text
Open Desktop
-> Filter page
-> list available games
-> can search/filter
-> clicking protected action asks user to login
```

Auth:

```text
Login/Register
-> API auth endpoint
-> receive account profile, role, and required user identifiers
-> populate Desktop session
-> return to Filter
```

Games:

```text
Games nav
-> My Games for standard user
-> All Games for admin
-> create/edit/delete or deactivate games
-> API persists changes
```

Rental request:

```text
Filter
-> Game Details
-> select dates
-> submit rental request
-> API creates request
-> API creates or updates conversation
-> API creates owner notification
```

Chat:

```text
Chat nav
-> conversations for logged-in user
-> rental request message visible to owner
-> owner can accept/decline where final UI supports it
-> chat state stays in sync with request/rental state
```

Notifications:

```text
Notifications nav
-> owner sees new rental request notification
-> renter sees accepted/declined notification
-> notifications use same logged-in user identity
```

Dashboard:

```text
Dashboard nav
-> payment history
-> rental/payment state from API
-> no dummy-only history
```

Account/Admin:

```text
Account nav
-> edit profile/password/avatar
-> admin account administration visible only for admin
-> admin can manage other users
-> admin can manage all games
```

### Department Outputs

At the end of section 5, Desktop should demonstrate the intended user story:

```text
Anonymous user opens Filter
-> logs in
-> returns to Filter
-> requests a game
-> owner sees chat and notification
-> owner accepts/declines
-> renter sees notification
-> dashboard/account/admin use the same user/database
```

### Parallelism

Section 5 can be split in parallel:

- Lane A: Filter, game details, date selection, rental request.
- Lane B: Chat, notifications, request accept/decline interaction.
- Lane C: Games, account, admin, dashboard/payment history.

Sequential gate:

- All lanes must use the same Desktop session context and the same `.Shared` API client contract.

---

## Final Dependency Graph

```text
1. Project Boundary Setup
    -> 2. API Cleanup
        -> 3. API Runtime Wiring
            -> 6. Local Unified Backend Setup
                -> 4. Desktop Single Shell
                -> 5. Desktop Feature Integration
```

Compact form:

```text
1 -> 2 -> 3 -> 6 -> 4/5
```

## Work That Can Run In Parallel

Parallel inside section 2:

- Auth/account/admin cleanup.
- Games/filter/requests/rentals cleanup.
- Chat/notifications/payments cleanup.

Parallel inside section 6:

- DB/migration verification.
- Swagger endpoint verification.
- Local setup documentation.

Parallel after section 6:

- Desktop shell/navigation.
- Desktop feature wiring.
- Desktop admin/account/dashboard polishing.

## Work That Must Be Sequential

Sequential gates:

1. Boundary decision before API cleanup.
2. API cleanup before API runtime wiring.
3. API runtime wiring before local backend setup.
4. Local backend setup before real Desktop feature integration.
5. One session/user contract before joining Desktop feature lanes.

## Definition Of Done For The Department

The Desktop/API department is done when:

- API runs locally as one backend.
- API uses one database.
- API controllers expose service-layer operations.
- Desktop runs beside API, not inside API.
- Desktop calls API over HTTP using shared clients/DTOs.
- Desktop starts on Filter, not Home.
- Login returns to Filter.
- One logged-in user identity drives account, chat, notifications, games, dashboard, and admin.
- Duplicate old request/rental navigation is removed from final Desktop flow.
- Rental request creates chat and notification state.
- Accept/decline creates renter notification and updates rental/request state.
- Admin can manage users and all games.
- Standard user can manage only their own games.
- No required demo flow depends on remote deployment.

