# Task 5: Request And Rental Lifecycle API

**Workflow source:** Section 2, API Cleanup  
**Type:** Parallel API feature lane, high risk  
**Can start after:** Task 1 and Task 2 route/service decisions  
**Can run in parallel with:** Tasks 3, 4, 6, but must coordinate strongly with Task 6  
**Suggested owner:** backend worker comfortable with business rules  
**Primary project area:** `BoardGames.Api`  
**Secondary coordination area:** `BoardGames.Shared`, `BoardGames.Desktop`, `BoardGames.Data`

## What This Task Is About

This task owns the core rental request lifecycle: selecting dates, creating a request, validating availability, approving/declining, and creating rental state.

This task should treat the project 2 rental request flow as the final workflow.

The important behavior is not the old standalone request/rental management screens. The important behavior is the lifecycle where a user requests a board game from Game Details, the owner responds, and the result is connected to chat, notifications, payment, and dashboard/history.

Project 1 request/rental artifacts should be treated as legacy unless they contain behavior that still supports the final lifecycle. Useful behavior should be preserved by moving it into the canonical request/rental API, not by keeping a second request/rental system.

After this task, there should be one backend meaning for request/rental state. Desktop, Web, chat, notifications, payment history, dashboard, and admin-related views should all read from or act on the same request/rental lifecycle.

## Canonical Direction For Requests And Rentals

The final application must use one request/rental lifecycle.

The canonical lifecycle is the project 2 flow, because it matches the final merged workflow:

```text
Filter / Game Details
-> select rental dates
-> create rental request
-> request appears in chat / notification flow
-> owner accepts or declines
-> accepted request creates confirmed rental state
-> payment/dashboard/chat can use the same request/rental data
```

The old project 1 request/rental pages are not the final business workflow:

```text
My Requests
Others' Requests
My Rentals
Others' Rentals
direct create rental/request pages
```

Those old pages may still contain useful display ideas or data needs, but they must not define a second backend flow. They should not create a separate request service, rental service, or route structure.

If any useful behavior from project 1 is still needed, it must be moved into or served by the canonical project 2-style `api/requests` and `api/rentals` services.

The final backend should not have two meanings for request/rental:

```text
Bad result:
Project 1 requests/rentals exist separately from project 2 chat rental requests.

Good result:
There is one request/rental lifecycle, and every UI reads or updates that same lifecycle.
```

## Related Audit Context

Helpful audit documents exist in `docs/audits`. They explain how this task relates to the other tasks in the `.Desktop + .Api` task set and should be read before implementation:

- `docs/audits/desktop-api-final-workflow.md`
- `docs/audits/desktop-api-contract-audit.md`
- `docs/audits/desktop-api-department-task-breakdown.md`
- `docs/audits/desktop-api-10-task-assignment-plan.md`

This task corresponds to Task 5 in the 10-task plan. It belongs to the parallel API feature lane after Task 1 and Task 2. It should not redo the architecture boundary work from Task 1 or the duplicate route ownership decisions from Task 2.

## Where This Fits In The Workflow

The intended department workflow is:

```text
Task 1 -> Task 2 -> Tasks 3 / 4 / 5 / 6 -> Task 7 -> Tasks 8 / 9 / 10
```

Task 5 can run in parallel with:

- Task 3, Auth / Account / Admin API;
- Task 4, Games / Filter / Search API;
- Task 6, Chat / Notifications / Payments API.

Task 5 must coordinate strongly with Task 6 because request/rental state is the business event that chat, notifications, payment history, and dashboard must attach to.

Task 5 also depends on Task 4 for the final game identity, owner identity, game activity, price, and availability-related game data. It depends on Task 3 for the final account identity rules used to decide renter and owner authorization.

Routing work and DB seed/setup work are assumed to be handled by separate owners. This task may document which routes, demo users, games, and date ranges are needed, but it should not become the routing task or the seed task.

## Main Goal

Transform the current state from this:

```text
legacy standalone request/rental screens
request/rental behavior split across old projects
repository-style booking behavior in legacy API code
request state, rental state, chat, notifications, and payments not fully unified
unclear final ownership between api/requests and api/rentals
```

Into this:

```text
one canonical request/rental lifecycle
one request service for request state transitions
one rental service for confirmed rental state
one availability rule used by request creation and date display
clear errors for Desktop and Web
clear handoff points for chat, notifications, payment history, and dashboard
legacy standalone request/rental flow no longer active as final behavior
```

## Current State From The Codebase

The active request route already exists in:

- `BoardGamesApp/BoardGames.Api/Controllers/RequestsController.cs`
- `BoardGamesApp/BoardGames.Api/Services/IRequestService.cs`
- `BoardGamesApp/BoardGames.Api/Services/RequestService.cs`
- `BoardGamesApp/BoardGames.Api/Services/RequestServiceErrors.cs`
- `BoardGamesApp/BoardGames.Api/Mappers/RequestMapper.cs`

The active controller uses the service layer and exposes:

```text
GET api/requests/owner/{ownerAccountId}
GET api/requests/renter/{renterAccountId}
GET api/requests/owner/{ownerAccountId}/open
POST api/requests
PUT api/requests/{requestId}/approve
PUT api/requests/{requestId}/deny
PUT api/requests/{requestId}/cancel
PUT api/requests/{requestId}/offer
GET api/requests/games/{gameId}/booked-dates
GET api/requests/games/{gameId}/availability
```

The active `RequestService` already contains important final-flow behavior:

- validates date ranges;
- checks that the requested game exists;
- prevents owner renting own game;
- checks availability before request creation;
- approves a request by creating confirmed rental state through repository behavior;
- denies and cancels requests;
- checks booked dates;
- sends some notification side effects when a request is denied, cancelled, approved, or made unavailable by a conflict.

These behaviors are close to the final direction, but this task still needs to verify whether the behavior is complete and whether each side effect belongs directly in Task 5 or should become a clean handoff to Task 6.

The active rental route already exists in:

- `BoardGamesApp/BoardGames.Api/Controllers/RentalsController.cs`
- `BoardGamesApp/BoardGames.Api/Services/IRentalService.cs`
- `BoardGamesApp/BoardGames.Api/Services/RentalService.cs`
- `BoardGamesApp/BoardGames.Api/Mappers/RentalMapper.cs`

The active controller uses the service layer and exposes:

```text
GET api/rentals/owner/{ownerAccountId}
GET api/rentals/renter/{renterAccountId}
POST api/rentals
GET api/rentals/games/{gameId}/availability
```

The active `RentalService` already contains confirmed rental behavior and owner/renter rental lists. This should remain behind the service boundary. Desktop and Web should not create confirmed rentals directly unless the final workflow explicitly requires that route. The normal user path should go through request creation and approval.

A legacy rental booking controller still exists in:

- `BoardGamesApp/BoardGames.Api/Legacy/Controllers/RentalsController.cs`

This legacy controller contains `BookGameWithRentalRequest`, which creates a rental and adds a rental request message to a conversation. That is useful behavior to understand, but the file is under `Legacy` and is excluded from compilation by `BoardGamesApp/BoardGames.Api/BoardGames.Api.csproj`.

Do not revive the legacy controller as a final active controller. If its chat-message creation behavior is still needed, coordinate with Task 6 and move the useful idea into the canonical `api/requests` lifecycle or into a Task 6 side-effect service.

The Shared API clients already point toward the canonical route shape:

- `BoardGamesApp/BoardGames.Shared/ProxyServices/IRequestService.cs`
- `BoardGamesApp/BoardGames.Shared/ProxyServices/RequestService.cs`
- `BoardGamesApp/BoardGames.Shared/ProxyServices/IRentalService.cs`
- `BoardGamesApp/BoardGames.Shared/ProxyServices/RentalService2.cs`

The Shared DTOs involved include:

- `BoardGamesApp/BoardGames.Shared/DTO/CreateRequestDTO.cs`
- `BoardGamesApp/BoardGames.Shared/DTO/RequestActionDTO.cs`
- `BoardGamesApp/BoardGames.Shared/DTO/RequestDTO.cs`
- `BoardGamesApp/BoardGames.Shared/DTO/RequestStatus.cs`
- `BoardGamesApp/BoardGames.Shared/DTO/CreateRentalDTO.cs`
- `BoardGamesApp/BoardGames.Shared/DTO/RentalDTO.cs`
- `BoardGamesApp/BoardGames.Shared/DTO/RentalDTO2.cs`

Important DTO detail: there are still two rental DTO concepts. `RentalDTO.cs` contains `RentalDataTransferObject` from the older integer-user flow, while `RentalDTO2.cs` contains `RentalDTO` for the account-based flow. Task 5 should not silently preserve both as final concepts. It should use the canonical account-based request/rental contract and coordinate DTO cleanup with the Shared owner if file naming or compatibility work is needed.

The Data project contains the persistence model and repository behavior for this lifecycle:

- `BoardGamesApp/BoardGames.Data/Models/Request.cs`
- `BoardGamesApp/BoardGames.Data/Models/Rental.cs`
- `BoardGamesApp/BoardGames.Data/Enums/RequestStatus.cs`
- `BoardGamesApp/BoardGames.Data/Repositories/IRequestRepository.cs`
- `BoardGamesApp/BoardGames.Data/Repositories/RequestRepository.cs`
- `BoardGamesApp/BoardGames.Data/Repositories/IRentalRepository.cs`
- `BoardGamesApp/BoardGames.Data/Repositories/RentalRepository.cs`

There are also older rental repository artifacts:

- `BoardGamesApp/BoardGames.Data/Repositories/IRentalRepository2.cs`
- `BoardGamesApp/BoardGames.Data/Repositories/RentalRepository2.cs`

Those should not become a second final rental backend. If useful behavior exists there, it should be preserved inside the canonical repository/service path or reported to the correct owner.

Desktop still contains old standalone request/rental screens and view models:

- `BoardGamesApp/BoardGames.Desktop/Views/CreateRequestView.xaml`
- `BoardGamesApp/BoardGames.Desktop/Views/CreateRentalView.xaml`
- `BoardGamesApp/BoardGames.Desktop/Views/RequestsFromOthersPage.xaml`
- `BoardGamesApp/BoardGames.Desktop/Views/RequestsToOthersPage.xaml`
- `BoardGamesApp/BoardGames.Desktop/Views/RentalsFromOthersPage.xaml`
- `BoardGamesApp/BoardGames.Desktop/Views/RentalsToOthersPage.xaml`
- matching view models under `BoardGamesApp/BoardGames.Desktop/ViewModels`

These pages are not the final business workflow. Desktop cleanup and visible navigation belong mostly to Tasks 9 and 10, but Task 5 must make sure those screens do not force the API to keep a second request/rental backend. If they are temporarily kept, they should consume the canonical `api/requests` and `api/rentals` routes.

## Owned Route Areas

This task owns the final implementation direction for these route areas:

```text
api/requests
api/requests/{requestId}/approve
api/requests/{requestId}/deny
api/requests/{requestId}/cancel
api/requests/games/{gameId}/availability
api/requests/games/{gameId}/booked-dates
api/rentals
```

The final route table should cover:

- create rental request from Game Details/date selection;
- list requests for renter;
- list requests for owner;
- list open requests for owner if still needed by owner-facing UI;
- approve request;
- deny request;
- cancel request if still kept;
- get booked dates for calendar/date-picker display;
- check availability for a selected date range;
- list confirmed rentals for renter;
- list confirmed rentals for owner;
- create confirmed rental only through the correct final path.

## Required Behavior

The canonical request/rental API must support:

- creating a rental request from Game Details/date selection;
- validating that the owner cannot rent their own game;
- validating date range;
- validating game existence;
- validating game active/available state;
- validating date availability against confirmed rentals;
- validating date availability against open or pending requests according to the final rule;
- returning clear API errors for invalid request;
- approving a request;
- declining a request;
- cancelling a request if the final workflow keeps cancellation;
- creating confirmed rental state after approval;
- resolving or invalidating conflicting open requests when one request is approved;
- exposing rental/request data needed by dashboard, payment, chat, and notifications.

The owner should decide and document the final meaning of each request state:

```text
Open
OfferPending, if still used
Accepted
Cancelled
Denied, if needed but not currently represented
```

If the current implementation deletes denied or cancelled requests instead of keeping a historical state, the owner must decide whether that is acceptable for dashboard/payment/history and chat. If history needs denied/cancelled requests, update the canonical rule instead of creating a separate legacy flow.

## Required Contract With Desktop And Web

Desktop Game Details and Web Game Details must be able to create a request with one clear DTO.

The create request contract should contain:

- game id;
- renter account id from the logged-in session;
- owner account id from game details or server lookup;
- start date;
- end date.

The create request response should contain:

- request id on success;
- clear error code and message on failure.

Desktop must be able to map API errors to friendly messages. The existing API already uses codes such as:

```text
invalid_date_range
game_not_found
owner_cannot_rent
dates_unavailable
request_not_found
request_forbidden
request_not_open
request_transaction_failed
rental_validation_failed
rental_conflict
```

This task should preserve or improve that idea. It should not return raw exceptions or database details to Desktop.

Desktop and Web must also be able to:

- ask whether a game is available for a selected range;
- load booked date ranges for a calendar;
- approve, deny, or cancel a request through the API;
- load renter-side request/rental history if the final UI needs it;
- load owner-side request/rental history if the final UI needs it;
- receive enough request/rental identifiers for Task 6 to connect chat messages, notifications, payment, and dashboard rows.

## State Transition Rules

The final state transition rules should be explicit and shared with Task 6.

Expected direction:

```text
Request created
-> Open request exists
-> owner approves or declines
-> approval creates confirmed rental state
-> chat and notifications reflect the same request/rental identifiers
```

Approval should:

- verify the current account owns the requested game;
- verify the request is still open or in the accepted final approvable state;
- verify the requested dates are still available;
- create one confirmed rental;
- mark or resolve the approved request according to the final state rule;
- resolve conflicting open requests for the same game/date range according to the final rule;
- expose the confirmed rental id to callers and Task 6.

Decline should:

- verify the current account owns the requested game;
- record or resolve the declined request according to the final state rule;
- keep enough information for chat/notification history if Task 6 needs it.

Cancel should:

- verify the cancelling account is allowed to cancel;
- resolve the request according to the final state rule;
- keep enough information for chat/notification history if Task 6 needs it.

Direct rental creation through `api/rentals` should not become a second user-facing booking flow. If `POST api/rentals` remains active, it should be clearly scoped as an internal/admin/support route or as compatibility only. The normal user path should create a request first and create the rental only after owner approval.

## Availability Rules

The final availability rule must be used consistently by request creation, availability checks, booked-date display, and rental confirmation.

The rule should account for:

- start date before end date or accepted same-day rule if the product allows same-day rental;
- no past start dates;
- maximum future booking window if the current one-month rule is kept;
- active game requirement;
- owner cannot rent own game;
- confirmed rental overlap;
- rental buffer hours from domain constants if kept;
- open or pending request overlap if the product reserves dates during pending requests;
- conflict resolution when one request is approved and other requests overlap.

If the current logic blocks a date range because another request is open, the owner must confirm that this is the desired product behavior. If pending requests should not block other renters until approval, update the rule and coordinate with Task 6 so chat/notification behavior still makes sense.

## Current Problems This Task Addresses

- Old project has standalone My Requests/Others Requests/My Rentals/Others Rentals.
- Final workflow should use one request/rental/chat/notification lifecycle instead.
- Request/rental services were duplicated during the merge.
- Some old controllers exposed repositories directly.
- The project must not keep one rental/request flow for old project 1 and another one for old project 2.
- The legacy booking controller contains chat-message behavior, but it is not the final active API surface.
- `api/requests` and `api/rentals` must have clear ownership so Task 6 can attach side effects safely.
- Rental DTO concepts are still mixed between old integer-user flow and account-based flow.
- Desktop error handling needs stable API error codes instead of raw exceptions.

## Coordination With Other Tasks

Coordinate with Task 3 for:

- account id format;
- renter and owner identity;
- authorization rules;
- admin override rules if admins can manage requests or rentals.

Coordinate with Task 4 for:

- game id;
- owner account id on game details;
- game active/deactivated behavior;
- game price if rental/payment totals depend on it;
- delete/deactivate rules for games that have open requests or confirmed rentals.

Coordinate with Task 6 for:

- request-created chat message;
- owner notification when a request is created;
- renter notification when a request is accepted or declined;
- finalization of rental request chat message after accept/decline;
- payment/dashboard history based on confirmed rentals;
- whether denied/cancelled requests must remain queryable for history.

Coordinate with Task 7 for:

- dependency injection registrations for `IRequestService`, `IRentalService`, mappers, repositories, and related helpers;
- final backend smoke flow using local API;
- required seed data for the separate DB seed/setup owner.

Coordinate with Task 9 for:

- Desktop Filter/Game Details date picker;
- availability display;
- request submission;
- friendly validation messages.

Coordinate with Task 10 for:

- Desktop Chat;
- Notifications;
- Dashboard/payment history;
- any temporary display of request/rental lists if the final navigation still exposes them.

## Implementation Hints

Use the active service-layer request path as the likely base:

- `RequestsController` should continue to call `IRequestService`;
- `RequestService` should continue to own request validation and state transitions;
- request repositories should stay behind API services;
- Desktop and Web should call API routes through Shared clients.

Use the active service-layer rental path as the likely base for confirmed rental reads and confirmed rental creation:

- `RentalsController` should continue to call `IRentalService`;
- `RentalService` should continue to own confirmed rental behavior;
- direct rental creation should be checked carefully so it does not bypass the request lifecycle.

Do not revive `BoardGamesApp/BoardGames.Api/Legacy/Controllers/RentalsController.cs` as a final route owner. Extract useful behavior from it only if it supports the canonical lifecycle.

Do not make the old Desktop request/rental pages define backend architecture. The final user journey starts from Filter/Game Details and uses chat/notification response, not old standalone request/rental CRUD pages.

Keep API errors stable and useful for Desktop. If a validation rule fails, return a structured error with a code and message instead of allowing raw exceptions to escape.

Be careful with ID types. The final account identity from Task 3 should be used for renter and owner. Do not reintroduce hardcoded integer users as the final request/rental contract.

Be careful with side effects. Task 5 owns the request/rental state. Task 6 owns the final chat, notification, payment, and dashboard side-effect contract. If Task 5 needs to call a notification or conversation service, the boundary must be agreed with Task 6.

The application may not build perfectly at the start of this task. Do not try to fix the whole solution just to make this task look complete. Fix task-related errors only and document unrelated blockers clearly.

## Expected Output

This task should produce:

- canonical request/rental API contract;
- final route ownership for `api/requests` and `api/rentals`;
- state transition rules;
- availability rules;
- error codes for Desktop;
- clear decision that the project 2 request lifecycle is canonical;
- clear list of any project 1 behavior preserved inside the canonical API;
- clear decision about direct rental creation through `api/rentals`;
- clear decision about denied/cancelled request history;
- notes for Task 6 about request/rental identifiers and side-effect handoff points;
- notes for Task 7 about required service, mapper, and repository registrations;
- notes for Task 9 and Task 10 about which routes Desktop should call.

## What Counts As Done

Desktop Game Details can create a request through API.

API rejects invalid date ranges and unavailable periods.

API rejects owner renting own game.

Approve creates consistent rental state.

Task 6 can attach chat/notification/payment side effects to the same request.

No second standalone request/rental backend flow remains active as final behavior.

`api/requests` and `api/rentals` have clear responsibilities.

Request/rental DTOs are clear enough that Desktop and Web do not have to guess which old DTO concept is final.

Direct rental creation is either safely scoped or removed from the normal user flow.

Unrelated build errors or unrelated merge problems are documented instead of silently taken over.

## Do Not Touch

Do not implement Desktop routing.

Do not implement Web pages.

Do not take over Task 3 auth/account/admin behavior.

Do not take over Task 4 games/filter/search behavior beyond the game data needed for request validation.

Do not take over Task 6 chat/notification/payment/dashboard behavior beyond the agreed request/rental state handoff.

Do not take over Task 7 runtime startup wiring, except to document required registrations.

Do not take over DB seed/setup work.

Do not revive deprecated duplicate controllers, services, repositories, or standalone request/rental pages as final behavior.

Do not keep project 1 request/rental flow and project 2 request/rental flow as separate active backend systems.

Do not move business logic into Desktop views, Web views, or Shared API clients.

Do not use hardcoded users or hardcoded ids to make the workflow appear to work.

Do not write tests unless the lead explicitly changes this task.

Do not try to fix unrelated build errors across the entire solution.

## Known Blockers And Assumptions

This task assumes Task 1 has prepared the dependency direction and identity/session contract.

This task assumes Task 2 has decided the canonical controller/service ownership and legacy quarantine direction.

This task assumes routing work is handled separately and should be consumed later by Desktop tasks.

This task assumes DB seed/setup is handled separately before final integration.

This task assumes Task 4 provides the final game details contract needed to know the game owner and active state.

This task assumes Task 6 will finish the final chat, notification, payment, and dashboard side effects that attach to request/rental state.

If any of those assumptions are not true, document the blocker and coordinate with the responsible task owner instead of expanding this task into setup work.

The current application may not build at the start of this task. The owner should fix request/rental-specific errors only and should not take ownership of unrelated Desktop, Web, Shared, Data, test, routing, or seed problems.
