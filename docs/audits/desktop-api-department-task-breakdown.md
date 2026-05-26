# Desktop and API Department Task Breakdown

**Purpose:** Concrete task list for the WinUI Desktop and ASP.NET Core API department, derived from `desktop-api-final-workflow.md` and `desktop-api-contract-audit.md`.

**Use:** This is a leadership task breakdown. Each task can later become its own detailed spec. This document intentionally does not include code-level implementation steps.

**Global workflow:**

```text
1 -> 2 -> 3 -> 6 -> 4/5
```

**Practical team workflow:**

```text
Wave 1: Boundary/contract gate
    -> Wave 2: API cleanup lanes in parallel
        -> Wave 3: API runtime wiring gate
            -> Wave 4: Local unified backend gate
                -> Wave 5: Desktop shell and feature lanes in parallel
                    -> Wave 6: End-to-end verification
```

---

## Dependency Map

### Sequential Gates

These must be completed in order:

```text
T01/T02/T03/T04
-> T05-T16
-> T17/T18/T19
-> T20/T21/T22
-> T23-T34
-> T35
```

### Parallel Groups

Can run mostly in parallel:

```text
API cleanup:
T05/T06/T07/T08/T09/T10/T11/T12/T13/T14/T15/T16

Desktop after local backend:
T23/T24/T25/T26/T27/T28/T29/T30/T31/T32/T33/T34
```

---

## Wave 1: Boundary And Contract Gate

This wave maps to workflow section `1. Project Boundary Setup`.

This wave is the most important setup gate. It prevents the team from solving compile errors by adding circular dependencies or wiring Desktop directly into API internals.

### T01: Finalize Project Dependency Direction

**Type:** Sequential gate  
**Primary area:** Architecture  
**Depends on:** none  
**Can run in parallel with:** T02, T03 evidence gathering  

**Task:** Decide and document the final dependency direction:

```text
Desktop -> Shared DTOs/API clients -> API -> Services -> Data -> Database
```

**Concrete decisions needed:**

- Desktop must not reference `.Api`.
- Desktop should not reference repositories.
- Desktop should not create/migrate/seed the database in final runtime.
- Shared should not depend on EF models.
- Data should not depend on Shared DTOs.
- API is the bridge between Shared DTOs and Data models.

**Output:**

- One accepted dependency rule for the team.
- A short note added to the department docs or team board.
- Agreement that `.Desktop -> .Api` project reference is not final architecture.

**Acceptance criteria:**

- Lead accepts the dependency direction.
- Every later task refers to this direction.
- Any exception is explicitly temporary and documented.

### T02: Inventory Current Boundary Violations

**Type:** Parallel evidence task  
**Primary area:** Architecture/cleanup  
**Depends on:** none  
**Can run in parallel with:** T01, T03, T04  

**Task:** List every current violation of the final dependency rule.

**Concrete items to inventory:**

- Desktop project references.
- Shared project references.
- API controllers injecting repositories.
- Desktop classes using Data repositories, `AppDbContext`, or API services directly.
- Shared classes using Data models or Data repository interfaces.
- Old namespaces still present:
  - `BookingBoardGames.*`
  - `BoardRentAndProperty.*`

**Output:**

- A list of files grouped by violation type.
- Mark each as "must remove before final" or "temporary allowed during transition".

**Acceptance criteria:**

- The team can see which files block clean architecture.
- No one has to rediscover the same boundary issue later.

### T03: Define Shared/Data Ownership

**Type:** Sequential decision after inventory  
**Primary area:** Shared/Data/API contract  
**Depends on:** T01, T02  
**Can run in parallel with:** T04  

**Task:** Decide which concepts live in `.Shared` and which live in `.Data`.

**Concrete decisions needed:**

- DTO ownership.
- Transport enum ownership.
- EF model ownership.
- Repository interface ownership.
- API client/proxy service ownership.

**Recommended ownership:**

- `.Shared`: DTOs, API client interfaces, API client implementations, API error response, transport enums.
- `.Data`: EF models, repository interfaces, repository implementations, migrations, `AppDbContext`.
- `.Api`: mappers, controllers, business services, dependency injection, auth, local runtime setup.

**Output:**

- A Shared/Data ownership table.
- A list of duplicate DTOs/enums that must be resolved later.

**Acceptance criteria:**

- Adding `.Data -> .Shared` is explicitly rejected as the cycle-causing solution.
- The team knows where DTOs and EF models should live.

### T04: Define User Identity And ID Contract

**Type:** Sequential decision  
**Primary area:** Auth/session/API/Desktop contract  
**Depends on:** T01  
**Can run in parallel with:** T03  

**Task:** Define how the final app identifies a user across Account, Games, Chat, Notifications, Dashboard, and Admin.

**Concrete decisions needed:**

- Public account ID type exposed to Desktop.
- How old integer user IDs are handled.
- What login response must contain.
- What Desktop session stores.

**Recommended contract:**

- Public user identity: `Guid AccountId`.
- Legacy/internal identity: `int PamUserId`.
- API translates between them.
- Desktop session stores both only if chat/rental legacy code still needs the integer ID.

**Output:**

- A user/session contract accepted by API and Desktop workers.
- Required login/profile DTO fields listed.

**Acceptance criteria:**

- No Desktop feature uses hardcoded Alice/Bob/Carol IDs in final flow.
- Chat, filter, account, notifications, dashboard, and games can all use the same logged-in user.

---

## Wave 2: API Cleanup Lanes

This wave maps to workflow section `2. API Cleanup`.

The lanes can run in parallel after Wave 1 decisions are accepted. Each lane must respect the same DTO, route, and identity contracts.

### T05: Resolve Duplicate API Controller Names

**Type:** Cross-lane API cleanup  
**Primary area:** API controllers  
**Depends on:** T01, T03, T04  
**Can run in parallel with:** T06-T16, but must coordinate with all API lanes  

**Task:** Decide which duplicate controllers are canonical and which are legacy/removed.

**Known duplicates:**

- `GamesController.cs` and `GamesController2.cs`.
- `RentalsController.cs` and `RentalsController2.cs`.
- `UsersController.cs` and `UsersController2.cs`.

**Output:**

- One canonical controller per route group.
- A written decision for every duplicate file.

**Acceptance criteria:**

- No two active controllers have the same class name in the same namespace.
- No two active controllers own the same final route.
- Legacy controllers, if kept temporarily, are clearly named and not used by Desktop.

### T06: Resolve Duplicate API Service And Interface Names

**Type:** Cross-lane API cleanup  
**Primary area:** API services  
**Depends on:** T01, T03  
**Can run in parallel with:** T05, but must coordinate with all API lanes  

**Task:** Decide which duplicate services/interfaces are canonical and which are legacy/removed.

**Known duplicates:**

- `IUserService.cs` and `IUserService2.cs`.
- `IRentalService.cs` and `IRentalService2.cs`.
- `UserService.cs` and `UserService2.cs`.
- `RentalService.cs` and `RentalService2.cs`.

**Output:**

- One active service interface per business concept.
- One active service implementation per business concept.
- A list of old service files to remove, rename, or quarantine.

**Acceptance criteria:**

- Dependency injection can later register one clear service per concept.
- Controllers do not depend on ambiguous service names.

### T07: Auth, Account, And Admin API Lane

**Type:** Parallel API lane  
**Primary area:** API auth/account/admin  
**Depends on:** T03, T04  
**Can run in parallel with:** T08, T09, T10, T11, T12  

**Task:** Make auth/account/admin the canonical identity system for the whole app.

**Routes to own:**

```text
POST /api/auth/login
POST /api/auth/register
POST /api/auth/logout
GET  /api/auth/forgot-password
GET  /api/accounts/{accountId}
PUT  /api/accounts/{accountId}
PUT  /api/accounts/{accountId}/password
POST /api/accounts/{accountId}/avatar
DELETE /api/accounts/{accountId}/avatar
GET  /api/admin/accounts
PUT  /api/admin/accounts/{accountId}/suspend
PUT  /api/admin/accounts/{accountId}/unsuspend
PUT  /api/admin/accounts/{accountId}/reset-password
PUT  /api/admin/accounts/{accountId}/unlock
```

**Concrete work to define later in spec:**

- Use one account/profile DTO.
- Ensure login returns role and all session fields needed by Desktop.
- Ensure admin authorization is enforced by API, not only hidden in Desktop.
- Remove duplicated sign-in/sign-up concepts from the other old project flow.

**Output:**

- Canonical auth/account/admin endpoints.
- Accepted login response contract.
- Admin role behavior documented.

**Acceptance criteria:**

- Desktop can login and populate one session.
- Desktop can load profile from API.
- Admin-only API operations are protected.

### T08: Games API Lane

**Type:** Parallel API lane  
**Primary area:** Games/my games/admin games  
**Depends on:** T03, T04  
**Can run in parallel with:** T07, T09, T10, T11, T12  

**Task:** Make one canonical games API for both "My Games" and filter/game details.

**Routes to own:**

```text
GET    /api/games
GET    /api/games/{gameId}
GET    /api/games/owner/{ownerAccountId}
GET    /api/games/owner/{ownerAccountId}/active
GET    /api/games/renter/{renterAccountId}/available
POST   /api/games
PUT    /api/games/{gameId}
DELETE /api/games/{gameId}
```

**Concrete work to define later in spec:**

- Decide canonical `GameDTO`.
- Decide game image contract.
- Ensure admin can see/manage all games.
- Ensure standard user can manage only own games.
- Ensure game details can serve filter/game-details pages.

**Output:**

- Canonical games controller/service contract.
- One `GameDTO` shape.
- Authorization rules for game management.

**Acceptance criteria:**

- My Games and Filter use the same game source.
- Admin game management does not need a separate data path.

### T09: Search And Filter API Lane

**Type:** Parallel API lane  
**Primary area:** Filter/discovery  
**Depends on:** T03, T04, T08 contract direction  
**Can run in parallel with:** T08 if route/DTO is coordinated  

**Task:** Define the final filter/search endpoint for anonymous and logged-in browsing.

**Route options to choose from:**

```text
GET  /api/games?filters...
POST /api/games/search
GET  /api/search/games?filters...
```

**Concrete filter requirements:**

- anonymous game browsing;
- logged-in game browsing;
- search by name;
- filter by city/location;
- filter by max price;
- filter by player count;
- filter by availability range;
- sorting.

**Output:**

- One chosen route style.
- One filter request contract.
- One game-list response contract.

**Acceptance criteria:**

- Desktop Filter can load games without login.
- Desktop Filter can apply filters through API.
- No dummy-only filter data remains in final flow.

### T10: Requests And Rentals API Lane

**Type:** Parallel API lane  
**Primary area:** Rental request lifecycle  
**Depends on:** T03, T04, T08  
**Can run in parallel with:** T11, but must coordinate chat/notification side effects  

**Task:** Make rental requests and rentals support the final rental workflow.

**Routes to own:**

```text
POST /api/requests
GET  /api/requests/owner/{ownerAccountId}
GET  /api/requests/renter/{renterAccountId}
PUT  /api/requests/{requestId}/approve
PUT  /api/requests/{requestId}/deny
PUT  /api/requests/{requestId}/cancel
GET  /api/requests/games/{gameId}/availability
GET  /api/requests/games/{gameId}/booked-dates
GET  /api/rentals/owner/{ownerAccountId}
GET  /api/rentals/renter/{renterAccountId}
```

**Concrete work to define later in spec:**

- Validate owner cannot rent own game.
- Validate date range.
- Validate availability.
- Create request.
- Approve request creates rental.
- Deny/cancel updates state.
- Expose enough data for chat/notification/dashboard.

**Output:**

- Canonical request/rental service.
- Clear state transitions.
- Clear error codes for Desktop.

**Acceptance criteria:**

- Game details can submit a rental request.
- Accept/decline updates request/rental state.
- Request/rental logic is API-owned, not Desktop-owned.

### T11: Chat And Conversation API Lane

**Type:** Parallel API lane  
**Primary area:** Conversations/chat  
**Depends on:** T04, T10 coordination  
**Can run in parallel with:** T10, T12  

**Task:** Make conversation APIs support the rental request chat workflow.

**Routes to own:**

```text
GET  /api/conversations/user/{userId}
GET  /api/conversations/{conversationId}
POST /api/conversations
POST /api/conversations/messages
PUT  /api/conversations/messages
POST /api/conversations/readreceipt
POST /api/conversations/rental/finalize/{messageId}
```

**Concrete work to define later in spec:**

- Decide if Desktop calls chat by `AccountId` or legacy `PamUserId`.
- Make conversation creation idempotent for renter/owner.
- Rental request message should be tied to request ID.
- Accept/decline interaction should update request/rental state.

**Output:**

- Canonical conversation API contract.
- Message DTO contract.
- User ID translation rule if still needed.

**Acceptance criteria:**

- Owner sees a chat message when renter requests a game.
- Chat message references the same request as notifications/rentals.

### T12: Notifications API Lane

**Type:** Parallel API lane  
**Primary area:** Notifications  
**Depends on:** T04, T10 coordination  
**Can run in parallel with:** T10, T11  

**Task:** Make notifications part of the rental request lifecycle.

**Routes to own:**

```text
GET    /api/notifications/user/{accountId}
GET    /api/notifications/{notificationId}
PUT    /api/notifications/{notificationId}
DELETE /api/notifications/{notificationId}
DELETE /api/notifications/request/{requestId}
```

**Concrete work to define later in spec:**

- Rental request creates owner notification.
- Accept creates renter notification.
- Decline creates renter notification.
- Notifications are tied to the same account identity used by Desktop session.
- Notification delete/read/update behavior is clear.

**Output:**

- Canonical notification service/controller.
- Notification DTO contract.
- Notification side-effect rules.

**Acceptance criteria:**

- Owner sees notification for incoming rental request.
- Renter sees notification when request is accepted/declined.

### T13: Payments And Dashboard API Lane

**Type:** Parallel API lane  
**Primary area:** Payments/dashboard  
**Depends on:** T04, T10  
**Can run in parallel with:** T07-T12  

**Task:** Make dashboard/payment history read real API data.

**Representative routes to decide:**

```text
GET /api/payments/history
GET /api/payments/history/{paymentId}
GET /api/payments/user/{accountId}/history
```

**Concrete work to define later in spec:**

- Decide whether dashboard reads payments, rentals, or a dedicated dashboard DTO.
- Make payment history user-specific.
- Remove dummy-only dashboard dependencies.
- Connect payment records to rental/request state.

**Output:**

- Dashboard/payment API contract.
- Payment history DTO.
- User-specific data rule.

**Acceptance criteria:**

- Desktop Dashboard can show real payment/rental data for logged-in user.
- Admin/user permissions are clear if admin can see more data.

### T14: API Error Contract Lane

**Type:** Cross-cutting API task  
**Primary area:** API errors/Desktop UX  
**Depends on:** T03  
**Can run in parallel with:** T07-T13  

**Task:** Standardize API error responses that Desktop clients can consume.

**Error contract:**

```text
status
code
error
```

**Concrete errors to cover:**

- invalid login;
- suspended/locked account;
- unauthorized action;
- game not found;
- request not found;
- owner cannot rent own game;
- invalid date range;
- dates unavailable;
- delete game conflict;
- validation errors.

**Output:**

- Error response contract.
- Desktop mapping list from API error code to message.

**Acceptance criteria:**

- Desktop does not parse random exception text.
- API returns predictable errors for expected business failures.

### T15: Shared API Client Contract Lane

**Type:** Cross-cutting Shared task  
**Primary area:** Shared API clients  
**Depends on:** T03, route decisions from T07-T13  
**Can run in parallel with:** API lanes, but finalization waits for routes  

**Task:** Align `.Shared` API client interfaces with final API routes.

**Concrete clients to define:**

- Auth client.
- Account client.
- Admin client.
- Games client.
- Search/filter client.
- Requests/rentals client.
- Conversations client.
- Notifications client.
- Payments/dashboard client.

**Output:**

- API client surface that Desktop can consume.
- No Shared API client depends on Data repositories/EF models.

**Acceptance criteria:**

- Desktop can use Shared clients without referencing API/Data internals.
- API client method names match final routes and DTOs.

### T16: Legacy API Quarantine Plan

**Type:** Cross-lane cleanup task  
**Primary area:** API/Shared/Desktop cleanup  
**Depends on:** T05, T06  
**Can run in parallel with:** T07-T15 after duplicate decisions  

**Task:** Decide what happens to old project artifacts that are not part of final flow.

**Artifacts to classify:**

- old direct repository API controllers;
- duplicated sign-in/sign-up;
- old request/rental pages and viewmodels;
- old namespace wrappers;
- old proxy repositories;
- Desktop database bootstrap code.

**Classification options:**

- final active;
- temporary compatibility;
- excluded from compilation;
- removed;
- renamed as legacy.

**Output:**

- A quarantine/removal list.
- No ambiguity about which old files should be used by new work.

**Acceptance criteria:**

- New tasks do not accidentally build on deprecated old files.
- Legacy code does not own final navigation or final API routes.

---

## Wave 3: API Runtime Wiring Gate

This wave maps to workflow section `3. API Runtime Wiring`.

This wave is mostly sequential because the API cannot be wired cleanly until the cleanup lanes agree on controllers, services, DTOs, and routes.

### T17: Create API Dependency Injection Registration Plan

**Type:** Sequential API setup  
**Primary area:** API runtime  
**Depends on:** T05-T16  
**Can run in parallel with:** T18 preparation only  

**Task:** List every active controller constructor dependency and register the needed services.

**Dependency groups:**

- `AppDbContext`.
- repositories.
- business services.
- mappers.
- auth/security helpers.
- avatar/file storage.
- conversation/chat helpers.
- notification helpers.
- payment/dashboard services.

**Output:**

- A complete DI registration list for `Program.cs`.
- A list of active controllers and their dependencies.

**Acceptance criteria:**

- Every active controller can be constructed by DI.
- No active controller depends on a legacy/duplicate service.

### T18: Define API Local Configuration

**Type:** API setup  
**Primary area:** API config  
**Depends on:** T01, T17  
**Can run in parallel with:** T17 planning  

**Task:** Define local API configuration.

**Concrete decisions:**

- local API URL;
- local DB connection string;
- SQL provider: LocalDB or SQL Server Express;
- development Swagger behavior;
- whether HTTPS is required locally.

**Recommended defaults:**

```text
API URL: http://localhost:5018
Database: MergedBoardGamesDb
Provider: LocalDB or SQL Server Express
```

**Output:**

- Local API configuration accepted by the team.

**Acceptance criteria:**

- Desktop team knows the API base URL.
- DB setup team knows the connection string target.

### T19: API Endpoint Smoke Checklist

**Type:** API verification planning  
**Primary area:** API quality gate  
**Depends on:** T07-T14  
**Can run in parallel with:** T17, T18  

**Task:** Create a smoke checklist for the local API before Desktop integration.

**Required smoke areas:**

- login;
- get games;
- filter/search games;
- get game details;
- create request;
- check conversation/chat message;
- check owner notification;
- approve/decline request;
- check renter notification;
- check dashboard/payment data.

**Output:**

- A backend smoke checklist that must pass before Desktop feature integration.

**Acceptance criteria:**

- The checklist proves backend behavior, not just API startup.

---

## Wave 4: Local Unified Backend Gate

This wave maps to workflow section `6. Local Unified Backend Setup`.

This gate comes before serious Desktop feature integration.

### T20: Local Database Migration And Schema Setup

**Type:** Sequential local backend setup  
**Primary area:** Database/API  
**Depends on:** T17, T18  
**Can run in parallel with:** T21 seed planning  

**Task:** Make the local database setup clear and repeatable.

**Concrete work to define later in spec:**

- decide migration command;
- decide whether existing migrations are valid after cleanup;
- decide if new migration is needed;
- document how to reset local DB;
- document expected database name.

**Output:**

- Local database setup procedure.
- Known schema state for `MergedBoardGamesDb`.

**Acceptance criteria:**

- A developer can create the local DB without guessing.
- API points to the same DB Desktop expects.

### T21: Local Seed And Demo Data Setup

**Type:** Local backend setup  
**Primary area:** Database/API demo data  
**Depends on:** T04, T20 planning  
**Can run in parallel with:** T20  

**Task:** Define local seed data needed for the final workflow.

**Required demo data:**

- one admin user;
- one normal game owner;
- one normal renter;
- active games;
- game owner relationships;
- enough data for filter/location;
- optional existing conversation/payment examples if useful.

**Output:**

- Demo credential list.
- Seed data contract.
- Decision about seed mechanism.

**Acceptance criteria:**

- The local demo can show the full rental request workflow.
- Seed data is useful and not random dummy noise.

### T22: Local Backend Smoke Test

**Type:** Sequential backend gate  
**Primary area:** API/local runtime  
**Depends on:** T19, T20, T21  
**Can run in parallel with:** none for final acceptance  

**Task:** Verify the backend workflow locally before Desktop integration.

**Smoke flow:**

```text
login
-> list/filter games
-> get game details
-> create rental request
-> verify conversation/message
-> verify owner notification
-> approve/decline request
-> verify renter notification
-> verify rental/payment/dashboard state
```

**Output:**

- Backend accepted as stable enough for Desktop feature wiring.
- Known issues listed if any are intentionally deferred.

**Acceptance criteria:**

- Desktop workers have stable routes and DTOs.
- Local API and DB are usable without remote deployment.

---

## Wave 5: Desktop Shell And Feature Integration

This wave maps to workflow sections `4. Desktop Single Shell` and `5. Desktop Feature Integration`.

Most tasks in this wave can run in parallel after T22. Shell design can start earlier, but final feature wiring should wait for T22.

### T23: Choose One Desktop App Entry Point

**Type:** Desktop shell setup  
**Primary area:** WinUI startup  
**Depends on:** T01; final wiring depends on T22  
**Can run in parallel with:** T24, T25  

**Task:** Decide which WinUI app entry point and main window are final.

**Current competing shapes:**

- `BookingBoardGames.App` / `BookingBoardGames.MainWindow`.
- `BoardRentAndProperty.App` / `BoardRentAndProperty.MainWindow`.

**Output:**

- One active app entry point.
- One active main window.
- One root frame.
- Old entry path removed, excluded, or clearly quarantined.

**Acceptance criteria:**

- Desktop opens one application identity.
- Startup does not depend on two old app models.

### T24: Build Final Desktop Navigation Shell

**Type:** Desktop shell task  
**Primary area:** WinUI navigation  
**Depends on:** T23  
**Can run in parallel with:** T25, T26  

**Task:** Define and implement the final navigation shell.

**Final navigation:**

```text
Filter
Games
Notifications
Dashboard
Chat
Account
Admin, only if admin
Logout, after login
```

**Navigation to remove from final visible flow:**

```text
My Rentals
Others' Rentals
My Requests
Others' Requests
Home
duplicate Sign In/Sign Up
```

**Output:**

- One shell with correct menu items.
- Anonymous vs logged-in menu behavior defined.

**Acceptance criteria:**

- App starts on Filter.
- Login is available from Filter.
- Admin appears only for admin.
- Old duplicated request/rental pages are not primary navigation.

### T25: Desktop API Client Configuration

**Type:** Desktop setup  
**Primary area:** API client config  
**Depends on:** T15, T18, T22  
**Can run in parallel with:** T23, T24  

**Task:** Make Desktop call the local API through Shared clients.

**Concrete decisions:**

- where API base URL is configured;
- default local API URL;
- how to handle missing config;
- whether Desktop uses HTTP or HTTPS locally.

**Output:**

- Desktop API URL setup.
- Desktop uses Shared API client registration.

**Acceptance criteria:**

- Desktop does not use hardcoded remote IP.
- Desktop does not call API project in-process.
- Desktop can be pointed at local API consistently.

### T26: Unified Desktop Session Context

**Type:** Cross-cutting Desktop task  
**Primary area:** Auth/session  
**Depends on:** T04, T07, T22  
**Can run in parallel with:** T27-T34 after contract is stable  

**Task:** Create or choose one Desktop session context for every feature.

**Session must support:**

- `AccountId`;
- `PamUserId` or equivalent if active chat still needs it;
- username;
- display name;
- email;
- role;
- avatar URL;
- login state.

**Output:**

- One session source used by all Desktop pages/viewmodels.
- Old static user switching removed from final flow.

**Acceptance criteria:**

- Account, Filter, Chat, Notifications, Games, Dashboard, and Admin all use the same logged-in user.

### T27: Desktop Auth Flow Integration

**Type:** Desktop feature task  
**Primary area:** Login/register  
**Depends on:** T07, T25, T26  
**Can run in parallel with:** T28-T34 after session contract  

**Task:** Unify Desktop login/register flow.

**Final flow:**

```text
Filter
-> Login button
-> Auth page
-> successful login/register
-> populate session
-> return to Filter
```

**Output:**

- One login/register UI flow.
- Login response populates unified session.

**Acceptance criteria:**

- No duplicate sign-in/sign-up flow is visible.
- Login returns to Filter, not Home.

### T28: Desktop Filter And Game Details Integration

**Type:** Desktop feature lane  
**Primary area:** Filter/discovery/game details  
**Depends on:** T08, T09, T25, T26  
**Can run in parallel with:** T29-T34  

**Task:** Connect Filter and Game Details to real API data.

**Required flow:**

```text
Open app
-> Filter loads active games
-> user filters/searches
-> user clicks game
-> Game Details loads real game data
```

**Output:**

- Filter uses API.
- Game Details uses API.
- Anonymous browsing works.

**Acceptance criteria:**

- Filter does not use dummy-only data.
- Game Details opens consistently from filtered results.

### T29: Desktop Rental Request Integration

**Type:** Desktop feature lane  
**Primary area:** Game rental request  
**Depends on:** T10, T11, T12, T26, T28  
**Can run in parallel with:** T30-T34 after game details works  

**Task:** Connect date selection and rental request creation to API.

**Required flow:**

```text
Game Details
-> choose dates
-> submit rental request
-> API creates request
-> API creates chat message
-> API creates owner notification
```

**Output:**

- Rental request UI calls API.
- Availability errors are displayed.
- Owner cannot rent own game.

**Acceptance criteria:**

- Owner receives request in chat/notification after renter submits.
- Request uses logged-in user from session.

### T30: Desktop Chat Integration

**Type:** Desktop feature lane  
**Primary area:** Chat/conversations  
**Depends on:** T11, T26  
**Can run in parallel with:** T31-T34  

**Task:** Connect Chat page to real conversations for logged-in user.

**Required behavior:**

- load conversations for current user;
- show rental request messages;
- support message sending where final flow needs it;
- support accept/decline/finalize interaction if chat owns that UI;
- remove hardcoded user switching.

**Output:**

- Chat uses real API data and real session user.

**Acceptance criteria:**

- Rental request from T29 appears in owner's chat.
- Chat does not rely on static Bob/Carol IDs.

### T31: Desktop Notifications Integration

**Type:** Desktop feature lane  
**Primary area:** Notifications  
**Depends on:** T12, T26  
**Can run in parallel with:** T30, T32, T33, T34  

**Task:** Connect Notifications page to real notification API for logged-in user.

**Required behavior:**

- show notifications for current user;
- show owner notification for new rental request;
- show renter notification for accepted/declined request;
- support read/delete/update if final UI needs it.

**Output:**

- Notifications use real API data.

**Acceptance criteria:**

- Notifications match the same request/chat lifecycle.
- Notifications use current session identity.

### T32: Desktop Games/My Games/Admin Games Integration

**Type:** Desktop feature lane  
**Primary area:** Games management  
**Depends on:** T08, T26  
**Can run in parallel with:** T28-T31, T33, T34  

**Task:** Connect Games navigation to API.

**Required behavior:**

- standard user sees own games;
- admin sees all games;
- create/edit/delete or deactivate goes through API;
- authorization errors are handled.

**Output:**

- My Games/Admin Games unified under Games navigation.

**Acceptance criteria:**

- Standard user cannot manage another user's game.
- Admin can manage all games.

### T33: Desktop Account And Admin Integration

**Type:** Desktop feature lane  
**Primary area:** Account/profile/admin users  
**Depends on:** T07, T26  
**Can run in parallel with:** T28-T32, T34  

**Task:** Connect Account and Admin pages to the canonical account/admin APIs.

**Required behavior:**

- profile loads from API;
- profile updates through API;
- password/avatar behavior uses API if kept;
- admin account list loads from API;
- admin suspend/unsuspend/unlock/reset actions call API.

**Output:**

- Account/Admin use same session and API.

**Acceptance criteria:**

- Admin section is hidden for standard user.
- Admin operations are blocked by API if non-admin attempts them.

### T34: Desktop Dashboard And Payment History Integration

**Type:** Desktop feature lane  
**Primary area:** Dashboard/payment history  
**Depends on:** T13, T26  
**Can run in parallel with:** T28-T33  

**Task:** Connect Dashboard and payment history to real API data.

**Required behavior:**

- dashboard loads user-specific data;
- payment history loads from API;
- no dummy-only history remains in final demo flow;
- payment/rental records are consistent with accepted requests.

**Output:**

- Dashboard/Payment History use real backend data.

**Acceptance criteria:**

- Logged-in user sees their relevant payment/rental history.
- Data matches the local database and API behavior.

---

## Wave 6: End-To-End Verification

This wave validates the department's work as one application.

### T35: Desktop + API End-To-End Demo Script

**Type:** Final verification task  
**Primary area:** Full workflow  
**Depends on:** T23-T34  
**Can run in parallel with:** none for final acceptance  

**Task:** Define and execute the final demo script for Desktop + API.

**Demo script:**

```text
start local API
start local Desktop
open Filter as anonymous user
login as renter
return to Filter
select game
choose date range
send rental request
login/open second user as owner if needed
verify owner chat message
verify owner notification
owner accepts or declines
verify renter notification
verify dashboard/payment/rental state
login as admin
verify admin can manage users and all games
verify standard user cannot manage other users/games
```

**Output:**

- Final department demo checklist.
- Known limitations list if anything is deferred.
- Confirmation that no remote deploy is needed.

**Acceptance criteria:**

- The application feels like one Desktop app backed by one local API.
- The user identity is consistent across all tested features.
- The workflow can be repeated by another team member from documentation.

---

## Recommended Assignment Structure

### Setup/Core Team

Best for strongest architecture/API people:

- T01: Project dependency direction.
- T03: Shared/Data ownership.
- T04: User identity contract.
- T17: DI registration plan.
- T18: API local configuration.
- T20: local DB setup.
- T22: backend smoke gate.

### API Team A: Auth/Account/Admin

- T07.
- Parts of T14.
- Support T33.

### API Team B: Games/Search/Requests/Rentals

- T08.
- T09.
- T10.
- Support T28, T29, T32.

### API Team C: Chat/Notifications/Payments

- T11.
- T12.
- T13.
- Support T30, T31, T34.

### Shared/API Client Team

- T15.
- Support DTO parts of T07-T13.
- Support Desktop client setup T25.

### Desktop Shell Team

- T23.
- T24.
- T27.

### Desktop Feature Team A: Filter/Rental

- T28.
- T29.

### Desktop Feature Team B: Chat/Notifications

- T30.
- T31.

### Desktop Feature Team C: Games/Account/Admin/Dashboard

- T32.
- T33.
- T34.

---

## Suggested Work Board Order

Use this board order:

```text
READY FIRST:
T01, T02, T03, T04

READY AFTER BOUNDARY CONTRACT:
T05, T06, T07, T08, T09, T10, T11, T12, T13, T14, T15, T16

READY AFTER API CLEANUP AGREEMENT:
T17, T18, T19

READY AFTER API RUNTIME WIRING:
T20, T21, T22

READY AFTER LOCAL BACKEND ACCEPTED:
T23, T24, T25, T26, T27, T28, T29, T30, T31, T32, T33, T34

FINAL:
T35
```

## Highest-Risk Tasks

These tasks should get the most senior attention:

- T03: Shared/Data ownership, because this is where dependency cycles are born.
- T04: user identity contract, because it affects every feature.
- T05/T06: duplicate API cleanup, because wrong choices here make the backend unstable.
- T10/T11/T12: request/chat/notification lifecycle, because this is the heart of the merged workflow.
- T17/T22: API runtime wiring and backend smoke test, because Desktop work depends on them.
- T26: unified Desktop session, because it determines whether the app feels like one application.

## Minimal Critical Path

If time is short, the minimum critical path is:

```text
T01 -> T03 -> T04 -> T05/T06 -> T07/T08/T09/T10/T11/T12
-> T17 -> T18 -> T20 -> T21 -> T22
-> T23 -> T24 -> T25 -> T26 -> T27 -> T28 -> T29 -> T30/T31
-> T35
```

Dashboard/admin polishing can be parallel or slightly later, but the rental request -> chat -> notification path must be protected because it proves the merge is real.

