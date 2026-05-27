# Task 2: API Duplicate Cleanup And Legacy Quarantine

**Workflow source:** Section 2, API Cleanup  
**Type:** Sequential gate before parallel API feature lanes  
**Can start after:** Task 1, Architecture, Boundary, And Identity Contract  
**Can run in parallel with:** limited investigation only; final canonical decisions must be coordinated  
**Suggested owner:** API coordinator or senior backend worker  
**Primary project area:** `BoardGames.Api`  
**Secondary coordination area:** `BoardGames.Shared` contracts only when DTO or route decisions must be recorded  

## What This Task Is About

This task cleans the API structure after the merge of the two old projects.

Right now, the API contains duplicated controllers and duplicated services from both previous applications. In several places, there are two versions of the same concept, for example two game controllers, two rental controllers, two user controllers, two user services, and two rental services. This makes the backend unclear because different features may call different versions of what should be the same API functionality.

The purpose of this task is not to fully implement all API features. The purpose is to decide and prepare the unified API surface so the next API tasks can work safely in parallel.

The final result should be one unified controller/service per business concept, not two old versions living side by side.

The unified service must preserve useful behavior from both older versions. If old project 1 supports account, my-games, admin, and notification behavior, and old project 2 supports filter, rental request, chat, dashboard, and payment behavior, the final API should not randomly delete one side. The owner of this task must inspect both versions, understand what each one provides, and define which behavior must be kept in the unified version.

After this task, Tasks 3-6 should be able to work on auth/account/admin, games/filter, rentals/requests, and chat/notifications/payments without stepping on duplicate controllers or guessing which service is the real one.

## Related Audit Context

Helpful audit documents exist in `docs/audits`. They explain how this task relates to the rest of the `.Desktop + .Api` task set and should be read before implementation:

- `docs/audits/desktop-api-final-workflow.md`
- `docs/audits/desktop-api-contract-audit.md`
- `docs/audits/desktop-api-department-task-breakdown.md`
- `docs/audits/desktop-api-10-task-assignment-plan.md`

This task corresponds to Task 2 in the 10-task plan. It is the API cleanup gate between Task 1 and the parallel API feature lanes, Tasks 3-6.

## Where This Fits In The Workflow

The department workflow is:

```text
Task 1
-> Task 2
-> Task 3 / Task 4 / Task 5 / Task 6
-> Task 7
-> Task 8 / Task 9 / Task 10
```

Task 2 must happen before Tasks 3-6 make deeper API changes. Without this cleanup, one developer may edit `GamesController.cs` while another edits `GamesController2.cs`, or one developer may register `IRentalService` while another version of `IRentalService` still exists with a different meaning.

This task does not own routing work and does not own DB seed/setup work. Those are assumed to be handled by other owners before the full task set is assigned. This task may mention which routes and data are needed, but it should not become the routing task or the seed task.

## Main Goal

Transform the API from this situation:

```text
old service/controller version A
old service/controller version B
unclear routes
repository exposed directly in some controllers
old namespaces mixed into active API files
duplicate class and interface names in the same namespace
```

Into this situation:

```text
one canonical controller per route area
one canonical service per business area
all useful old behavior preserved or assigned to a later task
controllers expose service-layer behavior
repositories stay behind services
legacy files are clearly quarantined or removed later
Tasks 3-6 know exactly which files and routes they own
```

## Current State From The Codebase

The API currently has a partial merge shape. `BoardGames.Api/Program.cs` registers controllers, Swagger, HTTPS redirection, and authorization only. It does not yet register the database, repositories, business services, mappers, or authentication helpers. Full runtime wiring belongs to Task 7, but Task 2 must make the active controller/service set clear enough for Task 7 to register it later.

Known duplicate controller files:

- `BoardGamesApp/BoardGames.Api/Controllers/GamesController.cs`
- `BoardGamesApp/BoardGames.Api/Controllers/GamesController2.cs`
- `BoardGamesApp/BoardGames.Api/Controllers/RentalsController.cs`
- `BoardGamesApp/BoardGames.Api/Controllers/RentalsController2.cs`
- `BoardGamesApp/BoardGames.Api/Controllers/UsersController.cs`
- `BoardGamesApp/BoardGames.Api/Controllers/UsersController2.cs`

Important detail: the `*Controller2.cs` files do not declare `GamesController2`, `RentalsController2`, and `UsersController2` classes. They declare `GamesController`, `RentalsController`, and `UsersController` again in the same `BoardGames.Api.Controllers` namespace. This is not only confusing architecturally; it is also a direct source of class-name conflicts.

Known duplicate service/interface files:

- `BoardGamesApp/BoardGames.Api/Services/IUserService.cs`
- `BoardGamesApp/BoardGames.Api/Services/IUserService2.cs`
- `BoardGamesApp/BoardGames.Api/Services/UserService.cs`
- `BoardGamesApp/BoardGames.Api/Services/UserService2.cs`
- `BoardGamesApp/BoardGames.Api/Services/IRentalService.cs`
- `BoardGamesApp/BoardGames.Api/Services/IRentalService2.cs`
- `BoardGamesApp/BoardGames.Api/Services/RentalService.cs`
- `BoardGamesApp/BoardGames.Api/Services/RentalService2.cs`

Important detail: the `*Service2.cs` and `I*Service2.cs` files also declare the same class/interface names as the non-2 files in the same `BoardGames.Api.Services` namespace. This creates ambiguity for dependency injection and blocks clean ownership of the business services.

Current direct repository controller examples:

- `GamesController.cs` injects `InterfaceGamesRepository` directly.
- `RentalsController.cs` injects `IRentalRepository`, `IConversationRepository`, and `InterfaceGamesRepository` directly.
- `UsersController.cs` injects `IUserRepository` directly.
- `ConversationController.cs` injects `IConversationRepository` directly.
- `PaymentsController.cs` injects `IPaymentRepository` and `IRepositoryPayment` directly.

The final API should expose service-layer behavior from controllers. Repositories should sit behind services.

Current service-layer controller examples that are closer to the final direction:

- `AuthController.cs` uses `IAuthService`.
- `AccountsController.cs` uses `IAccountService`.
- `AdminController.cs` uses `IAdminService`.
- `GamesController2.cs` uses `IGameService`.
- `RentalsController2.cs` uses `IRentalService`.
- `RequestsController.cs` uses `IRequestService`.
- `NotificationsController.cs` uses `INotificationService`.
- `UsersController2.cs` uses `IUserService`.

These service-layer examples are not automatically correct as final files. They still need to be inspected because some use old namespaces, duplicate DTOs, or only cover one old project's behavior.

Current DTO and namespace problems that affect this task:

- `BoardGamesApp/BoardGames.Shared/DTO/GameDTO.cs` and `GameDTO2.cs` both declare `GameDTO` in `BoardGames.Shared.DTO`.
- `BoardGamesApp/BoardGames.Shared/DTO/RentalDTO.cs` and `RentalDTO2.cs` contain competing rental transport concepts.
- Some API files import old namespaces such as `BookingBoardGames.*`, `BookingBoardGames.Sharing.DTO`, and `BoardRentAndProperty.*`.
- Some route areas still use integer user IDs while account/profile/admin use `Guid` account IDs.

Task 2 should not become the full Shared/Data cleanup task, but it must record DTO and ID conflicts that block the final API route decisions.

## Behavior That Must Be Preserved

The goal is not to choose one old project and throw away the other. The goal is to merge the useful behavior into one final API design.

Games behavior to preserve or assign:

- list all games for filter/discovery;
- get game details by ID;
- search/filter games;
- get game price where payment/rental logic still needs it;
- feed/filter behavior if still used by Desktop/Web;
- list games owned by an account;
- list active games owned by an account;
- list games available for a renter;
- create, update, delete, or deactivate games;
- support admin all-games behavior later through the canonical games service.

Rentals and requests behavior to preserve or assign:

- check availability for a game and date range;
- get booked or unavailable dates;
- create a request from game details and selected dates;
- prevent owner renting own game;
- approve, deny, or cancel requests;
- create confirmed rental state after approval;
- keep the link between request, rental, chat message, notification, payment, and dashboard history.

Users/account behavior to preserve or assign:

- keep project 1 auth/account/admin as the canonical identity direction;
- do not keep a second final login/register API under `api/users` if `api/auth` owns login/register;
- preserve user lookup behavior needed by chat and conversation participant lists;
- preserve balance/address behavior only if payment/dashboard/account still need it, and move it under the correct canonical account/user service.

Chat, notification, and payment behavior to preserve or assign:

- rental request creation must lead to chat and notification behavior in the final workflow;
- existing conversation/payment endpoints that still expose repositories directly must be marked for service-layer conversion by the correct later task;
- Task 2 should identify route ownership, not fully implement Task 6 behavior.

## Concrete Responsibilities

Decide which duplicate controllers are canonical:

- `GamesController.cs` vs `GamesController2.cs`
- `RentalsController.cs` vs `RentalsController2.cs`
- `UsersController.cs` vs `UsersController2.cs`

Decide which duplicate services/interfaces are canonical:

- `IUserService.cs` vs `IUserService2.cs`
- `IRentalService.cs` vs `IRentalService2.cs`
- `UserService.cs` vs `UserService2.cs`
- `RentalService.cs` vs `RentalService2.cs`

For each duplicate pair, inspect both old versions and answer:

- Which one has the better final structure?
- Which one already exposes service-layer behavior correctly?
- Which one still exposes repositories directly?
- Which endpoints or methods are still needed by Desktop/Web?
- Which useful methods from the other version must be moved into the canonical version?
- Which behavior belongs to Tasks 3-6 instead of Task 2?
- Which old file should become legacy/quarantined?

Classify each old file as one of:

- final active;
- temporary compatibility;
- legacy/quarantined;
- remove or exclude later.

Ensure final active controllers expose services, not repositories. If a repository-based endpoint contains useful behavior, move or assign that behavior into the canonical service path. Do not keep repository-based controllers as final active route owners.

Create a final API route ownership table for these route groups:

```text
api/auth
api/accounts
api/admin
api/games
api/games/search or chosen filter route
api/requests
api/rentals
api/conversations
api/notifications
api/payments
```

The table should show:

- final route group;
- final active controller;
- final active service interface;
- final active service implementation;
- old files that contributed behavior;
- old files that are quarantined;
- task owner that will complete the behavior later.

## Important Rule

Do not delete useful behavior only because it comes from the older version.

The goal is not to keep all `2` files or delete all `2` files. The goal is to choose one final API shape and preserve useful behavior from both previous applications.

Bad result:

```text
Keep GamesController2 and ignore filter/search/feed endpoints from GamesController that the Filter screen still needs.
```

Good result:

```text
Choose one canonical GamesController and make sure the final games API supports both My Games/Admin behavior and Filter/Game Details behavior.
```

Bad result:

```text
Keep RentalsController only because it has the booking endpoint, while leaving repository access and integer-only identity in the final controller.
```

Good result:

```text
Move the useful booking, availability, and chat-trigger behavior into the canonical request/rental service path, then let Task 5 and Task 6 finish the lifecycle details.
```

## Expected Final API Shape

The final API should move toward this structure:

```text
Controller -> API Service -> Repository -> AppDbContext -> Database
```

The final API should not keep this structure for active route owners:

```text
Controller -> Repository
```

Expected canonical ownership direction:

| Route area | Expected final owner direction |
| --- | --- |
| `api/auth` | `AuthController` -> `IAuthService` |
| `api/accounts` | `AccountsController` -> `IAccountService` |
| `api/admin` | `AdminController` -> `IAdminService` |
| `api/games` | one canonical `GamesController` -> one canonical `IGameService` |
| `api/games/search` or chosen filter route | canonical games/search service path, not direct repository access |
| `api/requests` | `RequestsController` -> `IRequestService` |
| `api/rentals` | one canonical `RentalsController` -> one canonical `IRentalService` |
| `api/conversations` | canonical conversation controller -> `IConversationService` |
| `api/notifications` | `NotificationsController` -> `INotificationService` |
| `api/payments` | canonical payments/dashboard controller -> payment/dashboard service |

If a route group needs a different final name, the owner must record the decision clearly so Desktop, Web, and Shared API clients do not target the wrong endpoint.

## Dependencies And Parallel Work

This task starts after Task 1 because it depends on the accepted architecture and identity rules:

- final dependency direction;
- Shared/Data ownership;
- public account ID vs legacy/internal user ID;
- Desktop session fields needed by later tasks.

Limited parallel investigation is possible while this task is running:

- Task 3 owner can inspect auth/account/admin behavior.
- Task 4 owner can inspect games/filter behavior.
- Task 5 owner can inspect requests/rentals behavior.
- Task 6 owner can inspect chat/notifications/payments behavior.

Final duplicate decisions must be centralized. Two people should not independently decide different canonical controllers or services.

Tasks 3-6 should not deeply implement until Task 2 has produced the route ownership table and duplicate artifact decisions.

## Coordination With Later Tasks

Task 3, Auth/Account/Admin API, consumes the decision for:

- whether `api/users` remains a final route area or becomes compatibility only;
- which service owns user lookup needed by chat;
- how duplicated login/register behavior is removed from the final API surface.

Task 4, Games/Filter/Search API, consumes the decision for:

- final `api/games` controller;
- final `IGameService`;
- which filter/search/feed methods must be preserved from the old repository controller.

Task 5, Request/Rental Lifecycle API, consumes the decision for:

- final request/rental route split;
- final `IRentalService`;
- whether booking-style behavior from old `RentalsController` moves into requests, rentals, or a helper service;
- availability and booked-date ownership.

Task 6, Chat/Notifications/Payments API, consumes the decision for:

- which repository-based controllers still need service-layer conversion;
- which routes belong to conversations, notifications, and payments;
- how rental request behavior will connect to chat and notification side effects.

Task 7, API Runtime Wiring, consumes the final active controller/service list for dependency injection. Task 2 should make the DI target clear, but Task 2 should not become the full `Program.cs` wiring task.

## Implementation Hints

Prefer the service-layer version as the structural base when it already matches the final direction, but do not assume it contains all behavior.

For games, `GamesController2.cs` is closer to the service-layer architecture because it uses `IGameService`, but `GamesController.cs` contains filter/search/feed/price behavior that may still be required by Filter, Game Details, rental pricing, or Dashboard. The final games API must account for both.

For rentals, `RentalsController2.cs` is closer to the service-layer architecture because it uses `IRentalService`, but `RentalsController.cs` contains booking, availability, and chat-message creation behavior. The final rental/request API must not lose this behavior. Some of it probably belongs to Task 5 and Task 6, but Task 2 must assign it clearly.

For users, `UsersController.cs` contains old login/register/balance/address behavior through a repository, while `UsersController2.cs` contains service-based user lookup for accounts except one account. The final app should treat `api/auth`, `api/accounts`, and `api/admin` as the canonical identity/account direction unless Task 1 says otherwise. Any remaining user lookup route should be clearly separated from login/register.

Avoid simply renaming files from `*2.cs` to non-2 names without merging behavior. That only hides the merge problem.

Avoid keeping duplicate routes as a convenience. Temporary compatibility routes are allowed only when they are named and documented as temporary.

Do not solve missing imports by adding project references that violate the Task 1 dependency direction. In particular, do not create a `.Data <-> .Shared` cycle.

If duplicate DTOs in `BoardGames.Shared` block this cleanup, record the blocker and coordinate with the Shared owner. Do not redesign all Shared DTOs inside this task unless the duplicate directly prevents the API duplicate cleanup.

## Output

This task should produce:

- API route ownership table;
- canonical controller/service list;
- duplicate artifact decision list;
- legacy/quarantine list;
- clear note of behavior that must be preserved from each old version;
- clear note of behavior that will be implemented by Tasks 3-6;
- clear note of DTO or identity conflicts that must be coordinated with Shared or Task 1;
- clear note of any unrelated build errors that remain outside this task.

The output may be included in the task handoff, pull request description, or a small markdown decision file under `docs/audits` if the team wants repository-tracked decisions.

## What Counts As Done

Every duplicate API artifact named in this task has an explicit decision:

- active final;
- temporary compatibility;
- legacy/quarantined;
- remove/exclude later.

There is no ambiguity about which controller owns:

- games;
- rentals;
- users/account lookup;
- auth/account/admin;
- requests;
- conversations;
- notifications;
- payments/dashboard.

The active API direction is service-layer based. Any active final controller in the duplicate cleanup area should depend on services, not repositories.

Useful behavior from both old projects is either:

- moved into the canonical route/service path;
- assigned to Task 3, 4, 5, or 6 with enough detail that it will not be forgotten;
- documented as intentionally legacy or removed.

Tasks 3-6 can start without guessing which file is canonical.

The task owner has documented unrelated blockers instead of taking ownership of the whole solution build.

## Do Not Touch

Do not fully implement auth, account, admin, games, filter, request, rental, chat, notification, payment, or dashboard behavior. Those are Tasks 3-6.

Do not try to fix the entire solution build. The current application may not build yet because the merge is incomplete and unrelated areas may still contain errors.

Do not write tests for this task unless the lead explicitly changes the task.

Do not touch Desktop feature wiring.

Do not take over Web work.

Do not take over DB seed/setup work.

Do not take over routing work.

Do not move business logic into Desktop, Web, or Shared API clients.

Do not create hardcoded users or hardcoded IDs to make old workflows appear to work.

Do not revive deprecated duplicate controllers or services as new final routes.

## Known Blockers And Assumptions

This task assumes Task 1 has already decided the architecture boundary and user identity contract. If Task 1 is not done, the owner of Task 2 should stop at inventory and not make final route/DTO decisions.

The application may not build at the start of this task. The owner should fix conflicts directly related to duplicate API artifacts, but should not fix unrelated errors from Desktop, Web, tests, DB seed, or routing.

`Program.cs` runtime dependency injection is not the main ownership of this task. Task 2 should make the active service list clear. Task 7 should wire the final active services and repositories.

Shared DTO duplicates may affect this task, especially `GameDTO` and `RentalDTO` shapes. If resolving those duplicates requires broad Shared work, the owner should coordinate with the Shared department instead of silently expanding scope.

The final route table must be accepted before the parallel API lanes go deep. This is the main value of the task: it turns the API from two merged old backends into one understandable backend surface.
