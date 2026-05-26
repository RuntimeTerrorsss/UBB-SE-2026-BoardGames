# Desktop and API 10-Task Assignment Plan

**Purpose:** Condensed 10-task plan for the `.Desktop + .Api` department. This is designed for assigning people now, while keeping enough structure that each task can later become a separate detailed spec.

**Source documents:**

- `docs/audits/desktop-api-final-workflow.md`
- `docs/audits/desktop-api-contract-audit.md`
- `docs/audits/desktop-api-department-task-breakdown.md`

**Global order:**

```text
1 -> 2 -> 3/4/5/6 -> 7 -> 8/9/10
```

This keeps the necessary setup gates sequential, then opens parallel API lanes, then opens parallel Desktop lanes after the local backend is stable enough.

**Important staffing note:**

- The routing person is not assigned a new task here. These tasks may define expected navigation targets, but the button-routing implementation can stay with the routing person.
- The DB seed/final bug-fixing person is not assigned a normal feature task here. Task owners must coordinate with that person for required seed data and final stabilization, but should not move that person's ownership into feature work.

**Assumption for this plan:**

This 10-task plan assumes routing work and DB seed/setup work are handled outside this department assignment. The `.Desktop + .Api` tasks should not take over those responsibilities. They only consume them:

- routing is treated as an external dependency for final button/page navigation;
- DB seed/setup is treated as an external dependency for local demo data and final stabilization;
- API/Desktop task owners may define what data or navigation targets they need, but they should not own implementing the seed task or the routing task.

When a task mentions local backend smoke, navigation shell, demo credentials, or seed data, it means "coordinate with the existing owner and verify compatibility", not "replace that owner's work".

---

## Dependency Overview

```text
Task 1: Architecture, boundary, and identity contract
    -> Task 2: API duplicate cleanup and legacy quarantine
        -> Task 3: Auth/account/admin API
        -> Task 4: Games/filter/search API
        -> Task 5: Request/rental lifecycle API
        -> Task 6: Chat/notifications/payments API
            -> Task 7: API runtime wiring and local backend smoke
                -> Task 8: Desktop app shell, API client config, and auth/session
                -> Task 9: Desktop filter/game details/rental request integration
                -> Task 10: Desktop chat/notifications/games/account/admin/dashboard integration
```

Compact version:

```text
1 -> 2 -> 3/4/5/6 -> 7 -> 8/9/10
```

---

## Task 1: Architecture, Boundary, And Identity Contract

**Workflow source:** Section 1, Project Boundary Setup  
**Type:** Sequential setup task  
**Can start immediately:** yes  
**Can run in parallel with:** nothing critical; this is the first gate  
**Suggested owner:** strongest architecture person or department lead  

### What This Task Is About

This task defines the rules that all other tasks must follow. It prevents circular dependencies and prevents Desktop from becoming coupled to API internals.

Target direction:

```text
Desktop -> Shared DTOs/API clients -> API -> Services -> Data -> Database
```

API internal direction:

```text
Controllers -> Services -> Repositories -> AppDbContext -> Database
```

### Concrete Responsibilities

- Decide final dependency direction.
- Confirm Desktop must not reference `.Api` directly.
- Confirm Desktop must not access repositories or `AppDbContext` directly.
- Confirm `.Shared` should not depend on EF models from `.Data`.
- Confirm `.Data` should not depend on `.Shared` DTOs.
- Define which project owns DTOs, EF models, repositories, mappers, and API clients.
- Define the user identity contract:
  - public account identity, likely `Guid AccountId`;
  - legacy/internal user identity, likely `int PamUserId`;
  - login response fields required by Desktop.
- Define what the one Desktop session must contain.

### Current Problems This Task Addresses

- `.Shared` currently references `.Data`.
- `.Data` contains concepts that are also shared/transport concepts.
- Adding `.Data -> .Shared` would create a cycle.
- Desktop currently references `.Api`.
- Desktop has multiple session concepts.
- Filter/chat use integer/static users while account/admin use account identity.

### Output

- One accepted architecture rule.
- One accepted Shared/Data ownership rule.
- One accepted identity/session contract.
- A short list of "forbidden final dependencies".

### Acceptance Criteria

- Every other task can use this as its contract.
- No worker should need to guess whether to use `AccountId`, `PamUserId`, DTOs, EF models, repositories, or API clients.
- Lead accepts this before API and Desktop implementation lanes go deep.

### Maps From 35-Task Audit

- T01
- T02
- T03
- T04

---

## Task 2: API Duplicate Cleanup And Legacy Quarantine

**Workflow source:** Section 2, API Cleanup  
**Type:** Sequential gate before parallel API feature lanes  
**Can start after:** Task 1  
**Can run in parallel with:** limited investigation only; final decisions must be coordinated  
**Suggested owner:** API coordinator / senior backend worker  

### What This Task Is About

This task turns the API from "two APIs copied into one namespace" into one understandable backend surface.

The goal is not to finish all API features here. The goal is to remove ambiguity so Tasks 3-6 can work in parallel without stepping on duplicate controllers/services.

### Concrete Responsibilities

- Decide which duplicate controllers are canonical:
  - `GamesController.cs` vs `GamesController2.cs`;
  - `RentalsController.cs` vs `RentalsController2.cs`;
  - `UsersController.cs` vs `UsersController2.cs`.
- Decide which duplicate services/interfaces are canonical:
  - `IUserService.cs` vs `IUserService2.cs`;
  - `IRentalService.cs` vs `IRentalService2.cs`;
  - `UserService.cs` vs `UserService2.cs`;
  - `RentalService.cs` vs `RentalService2.cs`.
- Classify old files:
  - final active;
  - temporary compatibility;
  - legacy/quarantined;
  - remove/exclude later.
- Ensure final controllers expose services, not repositories.
- Create final API route ownership table.

### Current Problems This Task Addresses

- Duplicate controller class names.
- Duplicate service/interface names.
- Some controllers inject repositories directly.
- Old namespaces still appear in API files.
- Desktop/Web cannot safely call routes until route ownership is clear.

### Output

- API route ownership table.
- Canonical controller/service list.
- Legacy/quarantine list.
- Decision for every duplicate API artifact.

### Acceptance Criteria

- Tasks 3-6 know which controller/service files they own.
- No two API workers are editing competing versions of the same concept.
- Active final controllers are service-layer controllers.

### Maps From 35-Task Audit

- T05
- T06
- T16

---

## Task 3: Auth, Account, And Admin API

**Workflow source:** Section 2, API Cleanup and Section 3, API Runtime Wiring  
**Type:** Parallel API feature lane  
**Can start after:** Task 1 and Task 2 route/service decisions  
**Can run in parallel with:** Tasks 4, 5, 6  
**Suggested owner:** backend worker comfortable with auth/account rules  

### What This Task Is About

This task makes the project 1 account system the canonical identity system for the whole merged application.

### Concrete Responsibilities

Own these route groups:

```text
api/auth
api/accounts
api/admin
```

Required behavior:

- Login.
- Register.
- Logout.
- Forgot password if kept.
- Get profile.
- Update profile.
- Change password.
- Avatar upload/remove if kept.
- Admin account list.
- Admin suspend/unsuspend/unlock/reset password.
- Admin authorization enforced by API, not only hidden in Desktop.

### Required Contract With Desktop

Login response must provide enough data for one Desktop session:

- account ID;
- legacy user ID if chat/rental flow still needs it;
- username;
- display name;
- email;
- role;
- avatar URL;
- account status;
- profile fields needed by Account page.

### Current Problems This Task Addresses

- Duplicate sign-in/sign-up flow exists from old project 2.
- Account/admin use one identity concept while chat/filter use another.
- Desktop cannot unify session until login response is clear.

### Output

- Canonical auth/account/admin API contract.
- Login/profile DTO agreed with Desktop session task.
- Admin API behavior documented.

### Acceptance Criteria

- Desktop can login through API and populate one session.
- Desktop can load/update profile through API.
- Admin-only operations are blocked for non-admin users.
- This task gives Task 8 the data needed for Desktop auth/session.

### Maps From 35-Task Audit

- T07
- relevant parts of T14
- support for T33

---

## Task 4: Games, Filter, And Search API

**Workflow source:** Section 2, API Cleanup  
**Type:** Parallel API feature lane  
**Can start after:** Task 1 and Task 2 route/service decisions  
**Can run in parallel with:** Tasks 3, 5, 6  
**Suggested owner:** backend worker comfortable with game/search data  

### What This Task Is About

This task creates one game API used by both:

- Games/My Games/Admin Games;
- Filter/Discovery/Game Details.

### Concrete Responsibilities

Own these route areas:

```text
api/games
api/games/{gameId}
api/games/owner/{ownerAccountId}
api/games/search or chosen filter route
```

Required behavior:

- List all active games for filter.
- List owner games for My Games.
- Admin can list/manage all games.
- Get game details.
- Create game.
- Edit game.
- Delete or deactivate game.
- Search/filter by name, city/location, price, player count, availability, sorting.
- Decide final game image contract.

### Current Problems This Task Addresses

- There are duplicate `GameDTO` concepts.
- Filter and My Games may use different old data paths.
- Game image representation is inconsistent.
- Admin all-games behavior must be unified with normal game management.

### Output

- Canonical games/search API contract.
- One game-list/game-details DTO shape.
- Game image decision.
- Authorization rule for standard user vs admin.

### Acceptance Criteria

- Filter and My Games read from the same backend source.
- Standard user manages only own games.
- Admin can manage all games.
- Task 9 can build Desktop Filter/Game Details against this route.
- Task 10 can build Desktop Games/Admin Games against this route.

### Maps From 35-Task Audit

- T08
- T09
- relevant parts of T14
- support for T28, T32

---

## Task 5: Request And Rental Lifecycle API

**Workflow source:** Section 2, API Cleanup  
**Type:** Parallel API feature lane, high risk  
**Can start after:** Task 1 and Task 2 route/service decisions  
**Can run in parallel with:** Tasks 3, 4, 6, but must coordinate strongly with Task 6  
**Suggested owner:** backend worker comfortable with business rules  

### What This Task Is About

This task owns the core rental request lifecycle: selecting dates, creating a request, validating availability, approving/declining, and creating rental state.

### Concrete Responsibilities

Own these route areas:

```text
api/requests
api/requests/{requestId}/approve
api/requests/{requestId}/deny
api/requests/{requestId}/cancel
api/requests/games/{gameId}/availability
api/requests/games/{gameId}/booked-dates
api/rentals
```

Required behavior:

- Create rental request from game details/date selection.
- Validate owner cannot rent own game.
- Validate date range.
- Validate date availability.
- Return clear API errors for invalid request.
- Approve request.
- Decline request.
- Cancel request if needed.
- Create confirmed rental after approval.
- Provide rental/request data needed by dashboard/payment/chat/notifications.

### Current Problems This Task Addresses

- Old project has standalone My Requests/Others Requests/My Rentals/Others Rentals.
- Final workflow should use chat/request/notification flow instead.
- Request/rental services are duplicated.
- Some old controllers expose repositories directly.

### Output

- Canonical request/rental API contract.
- State transition rules.
- Availability rules.
- Error codes for Desktop.

### Acceptance Criteria

- Desktop Game Details can create a request through API.
- API rejects invalid date ranges and unavailable periods.
- API rejects owner renting own game.
- Approve creates consistent rental state.
- Task 6 can attach chat/notification side effects to the same request.

### Maps From 35-Task Audit

- T10
- relevant parts of T14
- support for T29

---

## Task 6: Chat, Notifications, And Payments API

**Workflow source:** Section 2, API Cleanup  
**Type:** Parallel API feature lane, high risk  
**Can start after:** Task 1 and Task 2 route/service decisions  
**Can run in parallel with:** Tasks 3, 4, 5, but must coordinate strongly with Task 5  
**Suggested owner:** backend worker comfortable with cross-feature workflows  

### What This Task Is About

This task connects the rental request lifecycle to user-visible chat, notifications, and dashboard/payment data.

### Concrete Responsibilities

Own these route areas:

```text
api/conversations
api/notifications
api/payments or payment-history route
```

Required behavior:

- Get conversations for logged-in user.
- Create or find conversation between renter and owner.
- Create rental request chat message.
- Update/finalize rental request message when accepted/declined.
- Create owner notification when request is created.
- Create renter notification when request is accepted/declined.
- Expose notification list for current user.
- Expose payment/dashboard history based on real data.

### Current Problems This Task Addresses

- Chat uses old integer user IDs.
- Notifications use account identity.
- Rental request must produce both chat and notification state.
- Dashboard/payment history must stop being dummy-only.
- Conversation and notification APIs are not yet part of one unified flow.

### Output

- Canonical conversation API.
- Canonical notification API.
- Payment/dashboard API contract.
- Side-effect rules:
  - request created -> owner chat + owner notification;
  - request accepted/declined -> renter notification + state update.

### Acceptance Criteria

- Owner can see rental request in chat.
- Owner can see rental request notification.
- Renter can see accepted/declined notification.
- Dashboard/payment history reads real API data.
- Task 10 can build Desktop Chat/Notifications/Dashboard against this contract.

### Maps From 35-Task Audit

- T11
- T12
- T13
- relevant parts of T14
- support for T30, T31, T34

---

## Task 7: API Runtime Wiring And Local Backend Smoke

**Workflow source:** Section 3, API Runtime Wiring and Section 6, Local Unified Backend Setup  
**Type:** Sequential backend gate  
**Can start after:** Tasks 3, 4, 5, 6 have stable enough contracts  
**Can run in parallel with:** DB seed/setup person for seed data, but not owned by DB seed person  
**Suggested owner:** backend integrator / API coordinator  

### What This Task Is About

This task makes the API run locally as the unified backend, then proves the backend flow works before Desktop integration goes deep.

### Concrete Responsibilities

- Register `AppDbContext`.
- Register repository interfaces/implementations.
- Register business services.
- Register mappers.
- Register auth/security helpers.
- Register chat/notification/payment helpers.
- Define local API URL.
- Define local connection string strategy with DB seed/setup person.
- Confirm Swagger exposes final route groups.
- Create backend smoke checklist.
- Execute smoke test against local API.

### Coordination With DB Seed/Final Bug Person

The DB seed/setup person owns:

- final seed data;
- local DB seed content;
- final bug-fix support.

This task owner owns:

- API runtime wiring;
- API dependency injection;
- backend smoke verification;
- telling DB seed person what demo data is required.

### Required Backend Smoke Flow

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

### Current Problems This Task Addresses

- `Program.cs` registers controllers and Swagger only.
- API does not yet register the real service layer.
- API appsettings/local DB setup is incomplete.
- Desktop cannot safely integrate until backend routes are stable.

### Output

- Locally runnable API.
- Stable local API URL for Desktop.
- Stable endpoint smoke checklist.
- Backend accepted for Desktop work.

### Acceptance Criteria

- API starts locally.
- Active controllers can be constructed.
- Swagger shows final route groups.
- Local backend flow works without remote deploy.
- Desktop workers can start Tasks 8-10 with stable routes and DTOs.

### Maps From 35-Task Audit

- T17
- T18
- T19
- T20 coordination only
- T21 coordination only
- T22

---

## Task 8: Desktop App Shell, API Client Config, And Auth Session

**Workflow source:** Section 4, Desktop Single Shell and part of Section 5, Desktop Feature Integration  
**Type:** Desktop setup gate  
**Can start lightly after:** Task 1  
**Final wiring can start after:** Task 7  
**Can run in parallel with:** Tasks 9 and 10 after Task 7, but this provides their session/API foundation  
**Suggested owner:** Desktop architecture worker  

### What This Task Is About

This task makes WinUI one application with one startup path, one API base URL, and one logged-in session.

This task does not replace the existing routing person's task. It defines the shell and session foundation. The routing person can still handle button-to-page navigation details.

### Concrete Responsibilities

- Choose one active app entry point.
- Choose one active main window.
- Choose one root frame.
- Create/choose one shell container.
- Make Filter the first screen.
- Define anonymous vs logged-in shell behavior.
- Configure Desktop API base URL.
- Register/use Shared API clients.
- Remove hardcoded remote IP dependency from final path.
- Create/choose one Desktop session context.
- Unify login/register flow.
- Login returns to Filter.
- Session stores:
  - account ID;
  - legacy user ID if needed;
  - username;
  - display name;
  - role;
  - email/avatar/profile basics.

### Current Problems This Task Addresses

- Two Desktop app identities.
- Two main windows.
- Two login/register flows.
- No one session context.
- Desktop directly references `.Api`.
- Desktop has hardcoded/static users in old flow.
- API base URL configuration is unclear.

### Output

- One Desktop startup path.
- One Desktop session source.
- One Desktop API client configuration.
- Auth flow ready for feature pages.

### Acceptance Criteria

- Desktop opens to Filter.
- Login/register uses API.
- Login populates the same session used by all features.
- Desktop is configured to call local API.
- Desktop feature tasks do not need to invent their own session.

### Maps From 35-Task Audit

- T23
- T24 shell foundation, excluding detailed routing person's ownership
- T25
- T26
- T27

---

## Task 9: Desktop Filter, Game Details, And Rental Request Integration

**Workflow source:** Section 5, Desktop Feature Integration  
**Type:** Parallel Desktop feature lane  
**Can start after:** Task 7 and Task 8 session/API foundation  
**Can run in parallel with:** Task 10  
**Suggested owner:** Desktop feature worker for renter-side flow  

### What This Task Is About

This task implements the renter-side Desktop workflow from anonymous filter to rental request submission.

### Concrete Responsibilities

Required flow:

```text
Open Desktop
-> Filter page loads active games
-> user searches/filters
-> user clicks game
-> Game Details loads real API data
-> user chooses date range
-> user submits rental request
-> API creates request
-> API creates chat message and owner notification
```

This task owns:

- Filter page API integration.
- Anonymous browsing.
- Logged-in filter behavior.
- Game Details API integration.
- Availability display.
- Date selection validation at UI level.
- Rental request submission.
- Friendly error display for:
  - not logged in;
  - owner cannot rent own game;
  - invalid date range;
  - unavailable dates;
  - game not found.

### Current Problems This Task Addresses

- Filter/game details are from old project 2 and are not unified with account identity.
- Filter/chat currently use static/hardcoded user IDs in places.
- Game request flow must use API, not dummy data.

### Output

- Working Desktop Filter.
- Working Desktop Game Details.
- Working rental request submission from Desktop.

### Acceptance Criteria

- Anonymous user can browse/filter games.
- Protected rental action requires login.
- Logged-in user can request a game.
- Request uses the logged-in session user.
- Owner receives backend chat/notification side effects from Tasks 5-6.

### Maps From 35-Task Audit

- T28
- T29

---

## Task 10: Desktop Chat, Notifications, Games, Account, Admin, And Dashboard Integration

**Workflow source:** Section 5, Desktop Feature Integration  
**Type:** Parallel Desktop feature lane  
**Can start after:** Task 7 and Task 8 session/API foundation  
**Can run in parallel with:** Task 9  
**Suggested owner:** Desktop feature worker or small pair, because this touches several existing screens  

### What This Task Is About

This task integrates the remaining logged-in Desktop sections with the unified API and session.

It is broader than Task 9, but most screens already exist. The work is to make them use the same backend and same session, and to remove old duplicate navigation paths.

### Concrete Responsibilities

Chat:

- Load conversations for current session user.
- Show rental request messages.
- Remove hardcoded Bob/Carol/Alice switching.
- Support accept/decline/finalize action if final UI owns it.

Notifications:

- Load notifications for current session user.
- Show owner notification for new request.
- Show renter notification for accepted/declined request.
- Support read/delete/update if required.

Games:

- Standard user sees own games.
- Admin sees all games.
- Create/edit/delete/deactivate games through API.

Account:

- Profile loads from API.
- Profile updates through API.
- Password/avatar behavior uses API if kept.

Admin:

- Admin section visible only for admin.
- Admin account operations call API.
- Admin can manage all games through Games area.

Dashboard:

- Dashboard/payment history uses API.
- Data is user-specific.
- No dummy-only history remains in final demo flow.

### Current Problems This Task Addresses

- Old menu contains My Rentals, Others' Rentals, My Requests, Others' Requests.
- Chat, dashboard, and filter are not integrated with account/admin session.
- Account/admin/games are from one old project, chat/dashboard from another.
- Desktop must feel like one application.

### Output

- Chat uses real session/API.
- Notifications use real session/API.
- Games uses real session/API.
- Account/Admin use real session/API.
- Dashboard uses real session/API.
- Duplicate old request/rental navigation is removed from final visible flow.

### Acceptance Criteria

- Owner sees request in Chat and Notifications.
- Renter sees accepted/declined notification.
- Standard user manages only own games.
- Admin can manage users and all games.
- Account page edits the logged-in account.
- Dashboard/payment history reflects backend data.
- The application does not visually feel like two apps glued together.

### Maps From 35-Task Audit

- T30
- T31
- T32
- T33
- T34
- T35 demo support

---

## Parallel Work Plan

### Stage A: First Setup Gate

```text
Task 1
```

One owner, quick but important. Other people can read code and prepare questions, but final decisions must be centralized.

### Stage B: API Cleanup Gate

```text
Task 2
```

One coordinator. This unblocks API feature lanes.

### Stage C: API Feature Lanes In Parallel

```text
Task 3 / Task 4 / Task 5 / Task 6
```

These four can run mostly in parallel after Task 2.

Coordination points:

- Task 3 provides identity/session data needed by all others.
- Task 4 provides game data needed by Task 5.
- Task 5 creates request/rental state needed by Task 6.
- Task 6 creates chat/notification/payment side effects for Task 5.

### Stage D: Backend Runtime Gate

```text
Task 7
```

One integrator. This should start once Tasks 3-6 are stable enough. It coordinates with the separate DB seed/setup person.

### Stage E: Desktop Lanes In Parallel

```text
Task 8 / Task 9 / Task 10
```

Task 8 provides the shell/session/API-client foundation. Task 9 and Task 10 can begin UI review earlier, but real backend integration should wait for Task 7 and Task 8.

---

## Suggested People Assignment

If you have around 10 people for `.Desktop + .Api`, assign one person per task:

```text
P1  -> Task 1
P2  -> Task 2
P3  -> Task 3
P4  -> Task 4
P5  -> Task 5
P6  -> Task 6
P7  -> Task 7
P8  -> Task 8
P9  -> Task 9
P10 -> Task 10
```

If you have fewer people, combine like this:

```text
Task 1 + Task 2
Task 3
Task 4
Task 5 + Task 6
Task 7
Task 8
Task 9
Task 10
```

If you have more people, split Task 10 first:

```text
Task 10A: Chat + Notifications
Task 10B: Games + Account + Admin
Task 10C: Dashboard + Payment History
```

---

## Highest-Risk Tasks

These should get stronger owners:

- Task 1: wrong boundary decisions create dependency cycles.
- Task 2: wrong duplicate cleanup creates unstable API routes.
- Task 5: request/rental lifecycle is the core business flow.
- Task 6: chat/notification/payment side effects prove that the two old apps are truly merged.
- Task 7: Desktop should not integrate deeply before this is accepted.
- Task 8: if session is wrong, every Desktop feature feels disconnected.

---

## Minimal Critical Path

If time becomes tight, protect this path:

```text
Task 1
-> Task 2
-> Task 3/4/5/6
-> Task 7
-> Task 8
-> Task 9
-> Task 10 Chat + Notifications subset
```

The rental request -> chat -> notification flow is the proof that the merge is real. Dashboard/admin polish is important, but this path should be protected first.
