# Desktop and API Contract Audit

**Purpose:** This document defines the contract that the WinUI Desktop and ASP.NET Core API department must satisfy for the merged board-game rental application.

**Scope:** This is a leadership audit, not a code implementation ticket. It identifies the required contracts, current gaps, and acceptance criteria that can later be split into tasks.

**Department responsibility:** `.Desktop + .Api`, with required coordination around `.Shared` and `.Data`.

---

## Executive Summary

The current project is not yet one unified application. It is a minimal merge of two previous applications:

- one application centered on account/login/admin/my-games/notifications;
- one application centered on filter/game details/rental request/chat/dashboard/payment.

The final product must hide that history. A user should not see two applications sharing a window. The same user account must drive all features.

The department should treat the API as the single local backend and Desktop as a client of that backend.

Target architecture:

```text
Desktop -> Shared DTOs/API clients -> Api -> Services -> Data -> Database
```

The biggest current blockers are:

- project boundary confusion between `.Shared` and `.Data`;
- duplicate API controllers/services;
- API runtime not wired with services/repositories/database;
- Desktop directly referencing `.Api`;
- two Desktop startup/navigation/login models;
- no single session identity across account, chat, filter, notifications, dashboard, and games.

---

## 1. Architecture Contract

### Required Direction

The allowed direction is:

```text
BoardGames.Desktop
    -> BoardGames.Shared
        -> HTTP API calls

BoardGames.Api
    -> BoardGames.Shared
    -> BoardGames.Data
        -> SQL database
```

The API internally follows:

```text
Controller -> Service -> Repository -> AppDbContext -> Database
```

### Forbidden Final Direction

The final application should not have:

```text
Desktop -> Api project reference
Desktop -> Repository
Desktop -> AppDbContext
Shared -> Data EF model dependency
Data -> Shared DTO dependency
Controller -> Repository
```

### Current Evidence

Current code shows boundary violations:

- `BoardGames.Desktop/BoardGames.Desktop.csproj` references `BoardGames.Api/BoardGames.Api.csproj`.
- `BoardGames.Shared/BoardGames.Shared.csproj` references `BoardGames.Data/BoardGames.Data.csproj`.
- `BoardGames.Shared/ProxyRepositories` contains API proxies that implement data repository interfaces and use data models.
- API controllers such as old `GamesController`, `RentalsController`, `UsersController`, `PaymentsController`, and `ConversationController` still inject repository interfaces directly.

### Required Outcome

At completion:

- `.Desktop` references `.Shared`, not `.Api`.
- `.Desktop` gets data through HTTP API clients.
- `.Shared` contains transport contracts, not EF persistence contracts.
- `.Data` contains persistence contracts, not Desktop/API transport DTOs.
- `.Api` is the bridge between `.Shared` DTOs and `.Data` models.

---

## 2. Local Runtime Contract

### Required Local Runtime

Remote deployment is not part of this department plan. The department must make the application work locally.

Required local runtime:

```text
BoardGames.Api local URL
BoardGames.Desktop running beside it
MergedBoardGamesDb local SQL database
optional NotificationServer if used for local notification demo
optional BoardGames.Web if needed for full parallel GUI demo
```

### API URL Contract

Desktop must use one configurable local API base URL.

Recommended local default:

```text
http://localhost:5018
```

Desktop should not depend on:

- hardcoded remote IP addresses;
- random ports that differ per developer without documentation;
- direct in-process API project references.

### Database Contract

The local database must be the single source of truth for:

- accounts/users;
- roles;
- games;
- rentals;
- requests;
- conversations;
- messages;
- notifications;
- payments/payment history.

Recommended local database name:

```text
MergedBoardGamesDb
```

The team must document:

- connection string location;
- migration command;
- seed command or startup behavior;
- demo credentials;
- admin credentials;
- whether LocalDB or SQL Server Express is expected.

### Current Evidence

Current local setup is incomplete or mixed:

- API `appsettings.json` does not define a database connection string.
- API `Program.cs` does not register `AppDbContext`.
- Desktop has `DatabaseBootstrap` and `DatabaseConfig`, meaning Desktop currently participates in database setup, which should not be the final runtime ownership.
- Old Desktop code contains `RemoteApiUrl = "http://172.30.250.124:5000/api/"`.
- New Desktop code expects `ApiBaseUrl` from `App.config`, but no clear Desktop `App.config` was visible in the project file list.

### Required Outcome

At completion:

- API runs locally with one connection string.
- API owns migrations and DB access.
- Desktop calls local API URL from configuration.
- No final demo requires remote deploy or remote DB.

---

## 3. Identity And Session Contract

### Required Identity Rule

The same logged-in user must be used by:

- Account/Profile.
- Admin authorization.
- My Games.
- Filter and game details.
- Rental request.
- Chat.
- Notifications.
- Dashboard/payment history.

### Public And Internal IDs

The final contract should expose a stable account identity to Desktop:

```text
AccountId: Guid
Username
DisplayName
Role
```

The existing merged data also uses:

```text
PamUserId: int
```

The recommended rule is:

- Desktop primarily stores and passes `AccountId: Guid`.
- API translates `AccountId` to `PamUserId` internally when older chat/rental/payment tables require integer user IDs.
- If Desktop chat still needs integer IDs temporarily, the login/profile response must explicitly include the required integer ID so it is not guessed or hardcoded.

### Session Contract

Desktop session must contain:

- `AccountId`;
- `PamUserId` or equivalent internal chat user ID if still required by active Desktop pages;
- username;
- display name;
- email;
- role;
- avatar URL;
- account status;
- profile fields needed by Account page.

Desktop session must not use:

- static Bob/Alice/Carol switching;
- hardcoded `MainWindow.loggedInUserAlice`;
- separate session objects for old project 1 and old project 2 flows.

### Current Evidence

Current identity is split:

- Account/login/profile/admin uses `AccountProfileDataTransferObject` and role-based `Guid` account identity.
- Filter/chat/dashboard code uses `SessionContext.GetInstance().UserId` with integer IDs.
- `DiscoveryView` contains static switching between hardcoded users.
- `MainWindow` contains hardcoded logged-in user IDs.
- Desktop has multiple `LoginViewModel` files with the same class name but different concepts.

### Required Outcome

At completion:

- There is one Desktop session context.
- Login populates that context.
- Filter, Chat, Notifications, Games, Dashboard, Account, and Admin read the same context.
- Admin visibility is based on the same role returned by login.
- Rental request and chat messages are associated with the same real logged-in user.

---

## 4. DTO And Shared Contract

### Required DTO Rule

`.Shared` should define the transport shapes that Desktop and Web receive from API.

Required canonical DTO families:

- Auth DTOs: login, register, reset password.
- Account DTOs: profile, role, avatar upload.
- Game DTO.
- Search/filter DTO or query contract.
- Request DTO.
- Rental DTO.
- Conversation DTO.
- Message DTO.
- Notification DTO.
- Payment/history DTO.
- API error response.

### Duplicate DTOs To Resolve

Current duplicates include:

- `BoardGames.Shared/DTO/GameDTO.cs`
- `BoardGames.Shared/DTO/GameDTO2.cs`
- `BoardGames.Shared/DTO/RentalDTO.cs`
- `BoardGames.Shared/DTO/RentalDTO2.cs`
- mixed naming between `AccountProfileDTO` and `AccountProfileDataTransferObject`.

The final contract should have one canonical DTO name per concept. If aliases are kept temporarily, they should not create two competing shapes.

### Enum Contract

Shared transport enums should be available without creating `.Data <-> .Shared` cycles.

Enums that require a decision:

- request status;
- notification type;
- payment method;
- filter/sort options if Desktop and API both use them.

Recommended rule:

- EF-only enums can stay in `.Data`.
- transport enums used by Desktop/Web/API contracts should live in `.Shared`.
- API maps between `.Shared` enums and `.Data` enums if both are needed.

### Image Contract

Game images are currently represented inconsistently:

- some DTOs use `byte[]`;
- some older filter DTOs use `string Image` or image URL style;
- some web views use generated placeholders or mapped image URLs.

Recommended rule:

- My Games create/edit can send image bytes or multipart upload, but the API contract must choose one.
- Filter/game lists should return an image URL or small safe representation, not force every page to understand database image bytes.
- Desktop and Web should receive the same image contract.

### API Error Contract

API errors should use a consistent response shape:

```text
status
code
error
```

Desktop API clients should translate these into user-facing messages without parsing random exception text.

### Current Evidence

Current DTO/shared problems:

- Duplicate DTO files define the same class names.
- Some API files import old namespaces like `BoardRentAndProperty.Contracts.DataTransferObjects`.
- Some API and Desktop files import old namespaces like `BookingBoardGames.Sharing.DTO`.
- `.Shared` currently references `.Data`, which pulls persistence concepts into transport.

### Required Outcome

At completion:

- One DTO per concept.
- One enum location strategy.
- Shared does not depend on Data EF models.
- Desktop and API agree on the same DTO properties.
- Web can use the same DTO contract later.

---

## 5. API Endpoint Contract

### Auth

Required routes:

```text
POST /api/auth/login
POST /api/auth/register
POST /api/auth/logout
GET  /api/auth/forgot-password
```

Login response must provide enough identity data for Desktop session:

- account ID;
- integer legacy user ID if still needed;
- username;
- display name;
- role;
- email;
- avatar URL;
- account status.

### Accounts

Required routes:

```text
GET    /api/accounts/{accountId}
PUT    /api/accounts/{accountId}
PUT    /api/accounts/{accountId}/password
POST   /api/accounts/{accountId}/avatar
DELETE /api/accounts/{accountId}/avatar
```

Account routes must use service layer.

### Admin

Required routes:

```text
GET /api/admin/accounts
PUT /api/admin/accounts/{accountId}/suspend
PUT /api/admin/accounts/{accountId}/unsuspend
PUT /api/admin/accounts/{accountId}/reset-password
PUT /api/admin/accounts/{accountId}/unlock
```

Admin-only behavior must be enforced by role, not only hidden in Desktop UI.

### Games

Required routes:

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

Admin must be able to view/manage all games. Standard users must only manage their own games.

### Filter/Search

The filter feature must have one API contract for:

- anonymous game browsing;
- logged-in game browsing;
- name search;
- city/location filter;
- price filter;
- player count filter;
- availability date range;
- sorting.

The route can be one of:

```text
GET  /api/games?filters...
POST /api/games/search
GET  /api/search/games?filters...
```

The team should choose one canonical route and make Desktop/Web use it.

### Requests And Rentals

Required behavior:

- create rental request from game details/date selection;
- get requests for owner/renter if Desktop still needs internal screens;
- approve request;
- deny request;
- cancel request;
- check availability;
- get booked/unavailable dates;
- create confirmed rental after approval.

Representative routes:

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

The final user-facing Desktop flow should not rely on separate My Requests/Others Requests/My Rentals/Others Rentals pages as primary navigation.

### Conversations And Chat

Required routes:

```text
GET  /api/conversations/user/{userId}
GET  /api/conversations/{conversationId}
POST /api/conversations
POST /api/conversations/messages
PUT  /api/conversations/messages
POST /api/conversations/readreceipt
POST /api/conversations/rental/finalize/{messageId}
```

If chat continues to use integer user IDs, API must translate from account identity clearly.

### Notifications

Required routes:

```text
GET    /api/notifications/user/{accountId}
GET    /api/notifications/{notificationId}
PUT    /api/notifications/{notificationId}
DELETE /api/notifications/{notificationId}
DELETE /api/notifications/request/{requestId}
```

Rental request creation should create owner notification. Accept/decline should create renter notification.

### Payments And Dashboard

Required routes should support:

- payment history;
- payment details;
- card/cash payment result if those flows remain;
- dashboard read model.

Representative routes:

```text
GET /api/payments/history
GET /api/payments/history/{paymentId}
GET /api/payments/user/{accountId}/history
```

Dashboard must not depend on dummy-only data.

---

## 6. Desktop Contract

### Startup Contract

Desktop final startup:

```text
Open app
-> Filter page
```

There should be no required Home page.

### Anonymous Contract

Anonymous user can:

- open filter;
- browse games;
- search/filter;
- open login/register.

Anonymous user cannot:

- create rental request;
- manage games;
- open account;
- open admin;
- access personalized notifications/dashboard/chat.

Protected actions should redirect to login or show a login prompt.

### Logged-In Contract

Logged-in user can:

- browse filter;
- request a game;
- open chat;
- view notifications;
- view dashboard/payment history;
- manage own games;
- edit account/profile.

### Admin Contract

Admin user can:

- do everything a normal user can;
- see all games in Games/My Games area;
- manage all games;
- access account administration;
- suspend/unsuspend/unlock/reset users.

### Navigation Contract

Final Desktop navigation:

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

The final navigation should not show:

```text
My Rentals
Others' Rentals
My Requests
Others' Requests
Home
duplicate Sign In/Sign Up
```

### Current Evidence

Current Desktop does not satisfy the final contract:

- It contains `BookingBoardGames.App` and `BoardRentAndProperty.App`.
- It contains `MainWindow.xaml` and `MainWindow2.xaml`.
- It contains old and new login/register pages.
- `MenuBarPage` still lists old request/rental pages.
- Filter/chat/dashboard live under `BookingBoardGames.Src.Views`.
- Account/admin/my-games live under `BoardRentAndProperty.Views`.
- Some filter/chat pages use hardcoded user switching.
- Desktop directly references API project.

### Required Outcome

At completion:

- one Desktop app entry point;
- one main window;
- one shell;
- one session context;
- filter first;
- auth redirects back to filter;
- no visible duplicate old workflow pages;
- all feature pages read/write through API clients.

---

## 7. Business Workflow Contract

### Full Rental Request Workflow

Required final workflow:

```text
User opens Filter
-> selects a game
-> chooses date range
-> sends rental request
-> API validates availability
-> API creates request
-> API creates or updates conversation
-> API creates rental request chat message
-> API creates owner notification
-> owner sees chat and notification
-> owner accepts or declines
-> API updates request/rental state
-> API creates renter notification
-> dashboard/payment history reflects final state where applicable
```

### Data Consistency Rule

The same request must connect:

- game;
- renter;
- owner;
- date range;
- chat message;
- notification;
- rental after approval;
- payment/dashboard after payment.

### Failure Rules

API should reject:

- owner renting own game;
- invalid date range;
- unavailable date range;
- unauthenticated rental request;
- unauthorized owner/admin actions;
- operations on missing games/requests/accounts.

Desktop should show user-friendly messages for these API errors.

---

## 8. Current Gap Audit By Area

### Project References

Gap:

- Desktop references API project.
- Shared references Data project.

Impact:

- causes build/runtime coupling;
- creates risk of dependency cycles;
- violates client/server separation.

Needed:

- Desktop references Shared API clients only.
- API references Shared and Data.
- Shared/Data boundary is cleaned.

### API

Gap:

- duplicate controllers;
- duplicate services/interfaces;
- direct repository controllers;
- empty runtime wiring in `Program.cs`;
- old namespaces still present;
- services are not consistently registered.

Impact:

- API cannot reliably run as one backend;
- Swagger would expose duplicate or conflicting routes;
- Desktop cannot safely target stable endpoints.

Needed:

- one canonical controller/service per concept;
- DI registration;
- local DB setup;
- service-layer endpoints only.

### Shared

Gap:

- duplicate DTOs;
- old namespace references;
- API proxies mixed with repository interfaces/data models;
- shared/data cycle risk.

Impact:

- Desktop and API disagree on data shape;
- import fixes can create cycles;
- different features use different DTO meanings.

Needed:

- one DTO contract per feature;
- no EF model dependency in shared transport layer;
- API maps between DTOs and data models.

### Desktop

Gap:

- two app identities;
- two startup paths;
- duplicate login/register;
- old menu still exposes duplicate request/rental pages;
- filter/chat/dashboard not integrated into the account shell;
- hardcoded users in filter/chat path;
- Desktop owns some DB setup.

Impact:

- user sees two glued applications;
- session identity is not unified;
- request/chat/notification cannot be trusted end-to-end.

Needed:

- one shell;
- filter first;
- one session;
- API client only;
- old duplicate nav removed.

### Local Backend

Gap:

- no final local setup guide;
- API appsettings missing DB connection;
- Desktop API URL config unclear;
- seed/demo data not contract-driven.

Impact:

- team cannot reliably run the same local app;
- Desktop work blocks on backend uncertainty.

Needed:

- documented local backend;
- known DB;
- known API URL;
- known demo users;
- smoke-tested workflow.

---

## 9. Department Deliverables

### API Deliverables

- One active controller per route group.
- One active service interface/implementation per business concept.
- API controllers use service layer.
- API services use repositories.
- Repositories use `AppDbContext`.
- `Program.cs` registers all active dependencies.
- API runs locally.
- Swagger exposes final route groups.
- API uses one local database.
- API supports rental request -> chat -> notification workflow.

### Desktop Deliverables

- One active WinUI app entry point.
- One active main window.
- One navigation shell.
- Filter first.
- Login/register unified.
- One session context.
- Desktop uses API clients.
- Desktop does not reference API project directly.
- Desktop does not access repositories or database directly.
- Duplicate old request/rental nav removed.
- Admin-only navigation enforced by role.

### Shared/Data Deliverables

- One canonical DTO per concept.
- Shared DTOs do not depend on Data EF models.
- Data models do not depend on Shared DTOs.
- API maps between Shared DTOs and Data models.
- Transport enums and persistence enums have an agreed owner.

### Local Runtime Deliverables

- Local API base URL documented.
- Local DB connection documented.
- Migrations documented.
- Seed/demo users documented.
- API + Desktop run together locally.
- Optional NotificationServer local role documented.

---

## 10. Acceptance Checklist

Use this checklist before saying the Desktop/API department is done.

Architecture:

- [ ] Desktop does not reference API project.
- [ ] Desktop calls API over HTTP.
- [ ] API references Data and Shared.
- [ ] Shared no longer depends on Data EF models.
- [ ] Data does not depend on Shared DTOs.

API:

- [ ] API has one active controller per route group.
- [ ] API has one active service per concept.
- [ ] Controllers call services, not repositories.
- [ ] `Program.cs` registers DbContext, repositories, services, and mappers.
- [ ] API runs locally.
- [ ] Swagger shows final route groups.

Local backend:

- [ ] Local DB is created or migrated.
- [ ] Demo users exist.
- [ ] Admin user exists.
- [ ] Games exist.
- [ ] Request/chat/notification data can be created through API.
- [ ] No remote deploy is required.

Desktop:

- [ ] Desktop starts on Filter.
- [ ] Login returns to Filter.
- [ ] There is one session context.
- [ ] Games page uses real API data.
- [ ] Notifications page uses real API data.
- [ ] Chat uses the logged-in user.
- [ ] Dashboard/payment history uses real API data.
- [ ] Account uses real API data.
- [ ] Admin section appears only for admin.
- [ ] Duplicate old request/rental pages are removed from final navigation.

End-to-end workflow:

- [ ] User can login.
- [ ] User can browse/filter games.
- [ ] User can open game details.
- [ ] User can request a rental period.
- [ ] Owner receives chat request.
- [ ] Owner receives notification.
- [ ] Owner can accept or decline.
- [ ] Renter receives notification.
- [ ] Rental/payment/dashboard state is consistent.

---

## 11. Suggested Future Task Split

This audit can later be split into tasks in this order:

```text
1 -> 2 -> 3 -> 6 -> 4/5
```

Suggested task groups:

- Boundary cleanup task group.
- API duplicate cleanup task group.
- API DI/runtime wiring task group.
- Local DB/API setup task group.
- Desktop shell/navigation task group.
- Desktop feature integration task group.
- End-to-end manual verification task group.

The important leadership decision is that Desktop feature wiring waits for the unified local backend. Desktop shell design can start earlier, but deep feature integration should wait until API routes and DTOs are stable.

