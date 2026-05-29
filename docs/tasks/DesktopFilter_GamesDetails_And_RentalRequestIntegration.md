# Task 9: Desktop Filter, Game Details, And Rental Request Integration

**Workflow source:** Section 5, Desktop Feature Integration  
**Type:** Parallel Desktop feature lane  
**Can start after:** Task 7 and Task 8 session/API foundation  
**Can run in parallel with:** Task 10  
**Suggested owner:** Desktop feature worker for renter-side flow  
**Primary project area:** `BoardGames.Desktop`  
**Secondary coordination area:** `BoardGames.Shared` API clients and DTOs needed by Filter, Game Details, and request submission

## What This Task Is About

This task implements the renter-side Desktop workflow from anonymous filter browsing to rental request submission.

The final Desktop app should open into the filter/discovery experience. A user should be able to browse active games without logging in, search or filter the games, open Game Details, and then log in when they want to send a rental request.

The rental request must use the unified backend flow from Tasks 4, 5, and 6. It must not use the old Desktop booking service, static users, direct repository calls, local database access, or dummy data.

The final flow is:

```text
Open Desktop
-> Filter page loads active games from API
-> user searches/filters
-> user clicks game
-> Game Details loads real API data
-> user chooses date range
-> user submits rental request
-> API creates request
-> API creates chat message and owner notification
```

This task proves the renter-side part of the merged application. It connects the old project 2 Filter/Game Details experience to the final project 1 account identity and to the unified request/chat/notification backend.

## Related Audit Context

Helpful audit documents exist in `docs/audits`. They explain how this task relates to the other tasks in the `.Desktop + .Api` task set and should be read before implementation:

- `docs/audits/desktop-api-final-workflow.md`
- `docs/audits/desktop-api-contract-audit.md`
- `docs/audits/desktop-api-department-task-breakdown.md`
- `docs/audits/desktop-api-10-task-assignment-plan.md`

This task corresponds to Task 9 in the 10-task plan. It belongs to the Desktop integration lane after the API runtime gate and Desktop session/API foundation are ready.

This task should not redo Task 4 games API work, Task 5 request/rental API work, Task 6 chat/notification side effects, Task 7 backend runtime wiring, or Task 8 Desktop shell/session setup. It should consume those results through the agreed Shared API clients and Desktop session context.

## Where This Fits In The Workflow

The intended department workflow is:

```text
Task 1 -> Task 2 -> Tasks 3 / 4 / 5 / 6 -> Task 7 -> Tasks 8 / 9 / 10
```

Task 9 can begin once:

- Task 7 has the local API running with the final route groups;
- Task 8 has one Desktop shell, one API base URL, and one Desktop session;
- Task 4 has stable games/filter/search routes;
- Task 5 has stable request/availability routes;
- Task 6 has the request-created chat and notification side effects connected or clearly contracted.

Task 9 can run in parallel with Task 10 because Task 9 owns the renter-side Filter/Game Details/request entry flow, while Task 10 owns the remaining logged-in Desktop areas such as Chat, Notifications, Dashboard, My Games, Account, Admin, and Games management.

Routing work and DB seed/setup work are assumed to be handled by separate owners. This task may identify which navigation targets and seed data it needs, but it should not become the routing task or the seed task.

## Main Goal

Transform the current Desktop renter-side flow from this:

```text
Filter and Game Details use old project 2 service interfaces
Game Details uses BookingDTO and InterfaceBookingService
request submission uses App.BookingService and old integer user ids
session checks use old SessionContext singleton in places
filter/chat/rental flow can still depend on hardcoded or static users
API errors are not consistently mapped to friendly Desktop messages
```

Into this:

```text
Filter loads active games from the unified API
search/filter uses the Task 4 games/search contract
Game Details loads one game's real API details
availability uses the Task 5 booked-dates/availability contract
request submission uses the logged-in Desktop session account id
request creation calls api/requests through Shared ProxyServices
Desktop shows friendly validation and API error messages
chat/notification side effects are produced by the backend, not Desktop
```

## Current State From The Codebase

The current Desktop renter-side UI is mostly present, but it still follows old project 2 service patterns.

Relevant Desktop views:

- `BoardGamesApp/BoardGames.Desktop/Views/DiscoveryView.xaml`
- `BoardGamesApp/BoardGames.Desktop/Views/DiscoveryView.xaml.cs`
- `BoardGamesApp/BoardGames.Desktop/Views/FilteredSearchView.xaml`
- `BoardGamesApp/BoardGames.Desktop/Views/FilteredSearchView.xaml.cs`
- `BoardGamesApp/BoardGames.Desktop/Views/GameDetailsView.xaml`
- `BoardGamesApp/BoardGames.Desktop/Views/GameDetailsView.xaml.cs`
- `BoardGamesApp/BoardGames.Desktop/Views/ConfirmBookingView.xaml`
- `BoardGamesApp/BoardGames.Desktop/Views/ConfirmBookingView.xaml.cs`
- `BoardGamesApp/BoardGames.Desktop/Views/Controls/GameCard.xaml`
- `BoardGamesApp/BoardGames.Desktop/Views/Controls/GameCard.xaml.cs`

Relevant Desktop view models:

- `BoardGamesApp/BoardGames.Desktop/ViewModels/DiscoveryViewModel.cs`
- `BoardGamesApp/BoardGames.Desktop/ViewModels/FilteredSearchViewModel.cs`
- `BoardGamesApp/BoardGames.Desktop/ViewModels/GameDetailsViewModel.cs`
- `BoardGamesApp/BoardGames.Desktop/ViewModels/ConfirmBookingViewModel.cs`

The current `DiscoveryViewModel` and `FilteredSearchViewModel` use old service interfaces:

- `InterfaceSearchAndFilterService`
- `InterfaceGeographicalService`
- old `GameDTO`
- old `FilterCriteria`
- old `TimeRange`

The current `GameDetailsViewModel` and `ConfirmBookingViewModel` use:

- `InterfaceBookingService`
- `BookingDTO`
- old `SessionContext.GetInstance().UserId`
- `App.BookingService`
- old integer user ids for renter/owner decisions.

The current `ConfirmBookingViewModel.ConfirmBooking` calls `bookingService.AddBooking(...)`. That is not the final behavior. The final behavior is to create a rental request through the canonical request API from Task 5.

The current `GameDetailsView.xaml.cs` navigates to `ConfirmBookingView` after date selection. Keeping a confirmation step is acceptable if it still submits through the final `api/requests` route. The confirmation step must not create a separate booking/rental flow.

The newer Desktop session foundation exists under:

- `BoardGamesApp/BoardGames.Desktop/Services/ISessionContext.cs`
- `BoardGamesApp/BoardGames.Desktop/Services/SessionContext.cs`
- `BoardGamesApp/BoardGames.Desktop/Services/ICurrentUserContext.cs`
- `BoardGamesApp/BoardGames.Desktop/Services/CurrentUserContext.cs`

Task 9 should use this Task 8 session foundation after it is active. It should not use `BoardGames.Data.Enums.SessionContext` as the final user/session source.

The newer Shared API clients exist under:

- `BoardGamesApp/BoardGames.Shared/ProxyServices/IGameService.cs`
- `BoardGamesApp/BoardGames.Shared/ProxyServices/GameService.cs`
- `BoardGamesApp/BoardGames.Shared/ProxyServices/IRequestService.cs`
- `BoardGamesApp/BoardGames.Shared/ProxyServices/RequestService.cs`
- `BoardGamesApp/BoardGames.Shared/ProxyServices/ServiceCollectionExtensions.cs`

The Shared request client already exposes useful methods for this task:

```text
CreateRequestAsync
CheckAvailabilityAsync
GetBookedDatesAsync
```

The Shared game client already exposes:

```text
GetAllGamesAsync
GetGameByIdAsync
GetGamesForOwnerAsync
GetAvailableGamesForRenterAsync
GetActiveGamesForOwnerAsync
```

However, the current Shared game client may still need coordination with Task 4 and the Shared owner. The API `GET api/games/{gameId}` returns a game details shape, while the current Shared `IGameService.GetGameByIdAsync` returns `GameSummaryDTO`. The API also exposes `POST api/games/search`, but the current Shared game client does not clearly expose a search method. Task 9 should not work around this by calling old services or repositories. If the final Task 4 contract requires a Shared client adjustment, report and coordinate that dependency.

Relevant Shared DTOs include:

- `BoardGamesApp/BoardGames.Shared/DTO/GameSummaryDTO.cs`
- `BoardGamesApp/BoardGames.Shared/DTO/GameDetailDTO.cs`
- `BoardGamesApp/BoardGames.Shared/DTO/GameSearchCriteriaDTO.cs`
- `BoardGamesApp/BoardGames.Shared/DTO/CreateRequestDTO.cs`
- `BoardGamesApp/BoardGames.Shared/DTO/BookedDateRangeDTO.cs`
- `BoardGamesApp/BoardGames.Shared/DTO/ApiErrorResponse.cs`

The Desktop request error mapper already exists:

- `BoardGamesApp/BoardGames.Desktop/Services/RequestErrorMapper.cs`
- `BoardGamesApp/BoardGames.Desktop/Services/RequestServiceErrors.cs`

This task should use or extend that direction for friendly UI messages instead of displaying raw exceptions.

## Owned Desktop Areas

This task owns the Desktop implementation for:

```text
Filter / Discovery page
Filtered Search page
Game Details page
date selection and availability display
rental request submission
friendly renter-side validation and error messages
```

This task may touch Shared API clients or DTOs only when required to consume already-agreed API contracts from Tasks 4 and 5. It should not redesign broad Shared architecture.

This task should not own the global shell, menu routing, login page implementation, chat page implementation, notification page implementation, dashboard implementation, My Games, Account, or Admin.

## Required Behavior

Anonymous user behavior:

- Desktop opens to the Filter/Discovery experience.
- Anonymous users can load active games from the API.
- Anonymous users can search and filter games.
- Anonymous users can open Game Details.
- Anonymous users cannot submit a rental request.
- If an anonymous user tries to request a game, Desktop shows a friendly login-required message and routes to login only through the routing behavior agreed outside this task.

Logged-in user behavior:

- logged-in users see the same active games from the same API source;
- logged-in users can search/filter without using hardcoded user ids;
- logged-in users can open Game Details;
- logged-in users can select a valid date range;
- logged-in users can see unavailable dates or availability feedback from the API;
- logged-in users can submit a rental request through `IRequestService.CreateRequestAsync`;
- request submission uses `ISessionContext.AccountId` as the renter account id;
- request submission uses the owner account id from the game details API response;
- Desktop shows success only after the API confirms the request was created.

Backend side effect behavior:

- Desktop should not create chat messages directly for this flow.
- Desktop should not create notifications directly for this flow.
- Desktop should rely on the API behavior from Tasks 5 and 6:

```text
POST api/requests
-> request created
-> chat rental request message created
-> owner notification created
```

## Required API And Shared Contracts

Task 9 should consume the final Task 4 games API through Shared clients.

Expected games behavior:

```text
GET api/games
POST api/games/search
GET api/games/{gameId}
GET api/games/{gameId}/image, if image URLs point there
```

Filter cards need:

- game id;
- game name;
- price;
- city or location;
- minimum player count;
- maximum player count;
- image display value;
- owner display name if shown on the card;
- owner account id if needed for navigation/state.

Game Details needs:

- game id;
- game name;
- description;
- price;
- city or location;
- minimum player count;
- maximum player count;
- image display value;
- owner account id;
- owner display name;
- active/deactivated state.

Task 9 should consume the final Task 5 request/availability API through Shared clients.

Expected request behavior:

```text
GET  api/requests/games/{gameId}/booked-dates
GET  api/requests/games/{gameId}/availability
POST api/requests
```

Request creation should send:

- game id;
- renter account id from `ISessionContext.AccountId`;
- owner account id from game details;
- start date;
- end date.

Request creation should handle API errors such as:

```text
invalid_date_range
game_not_found
owner_cannot_rent
dates_unavailable
```

## UI Validation Rules

Desktop should validate obvious input before calling the API:

- user must be logged in before submitting a request;
- start date must be selected;
- end date must be selected;
- end date must not be before start date;
- past dates must not be selectable;
- unavailable booked dates should not be selectable when they are known;
- owner cannot rent own game when the current account id equals the game owner account id.

The API remains the source of truth. Desktop validation is for user experience only. If Desktop validation passes but the API rejects the request, Desktop must show the API failure in a friendly way.

Friendly error display should cover:

- not logged in;
- owner cannot rent own game;
- invalid date range;
- unavailable dates;
- game not found;
- API unavailable or timeout;
- empty game list;
- no search results.

## Current Problems This Task Addresses

The current Desktop renter-side flow has these problems:

- Filter/Game Details come from old project 2 and are not fully unified with account identity.
- Filter/chat/rental still use static or old integer user IDs in places.
- Game request flow uses old booking services instead of the final `api/requests` lifecycle.
- Game Details uses `BookingDTO` instead of the final game details DTO.
- Confirm booking calls `bookingService.AddBooking`, which does not match the final request lifecycle.
- Filter uses old search/filter services instead of the final Shared API client direction.
- The final renter-side flow must use API data, not dummy data or local database behavior.
- Desktop currently risks creating behavior that does not trigger the backend chat and notification side effects.

## Coordination With Other Tasks

Coordinate with Task 4 for:

- final games/search route names;
- game list DTO;
- game details DTO;
- image URL or image bytes decision;
- owner account id on game details;
- active/deactivated game behavior;
- search/filter criteria supported by the API.

Coordinate with Task 5 for:

- final request creation DTO;
- date range validation rules;
- booked-date route;
- availability route;
- API error codes;
- whether same-day rental is allowed;
- whether pending requests block availability.

Coordinate with Task 6 for:

- confirming that request creation creates the owner chat message;
- confirming that request creation creates the owner notification;
- deciding what Desktop should do after successful request submission;
- confirming that Desktop does not need to manually create chat/notification state.

Coordinate with Task 7 for:

- local API URL;
- Swagger smoke status of games and request routes;
- backend availability during Desktop integration;
- required seed games/users/date ranges from the separate DB seed/setup owner.

Coordinate with Task 8 for:

- one active Desktop startup path;
- Filter as first screen;
- API client registration in Desktop;
- final `ISessionContext` usage;
- login-required routing behavior;
- current account id and optional legacy/PAM user id availability.

Coordinate with Task 10 for:

- what happens after request submission;
- whether successful request submission navigates to Chat, stays on details, or returns to Filter;
- how Chat/Notifications display the backend side effects.

## Implementation Hints

Use dependency injection and Shared API clients from Task 8. Do not manually create repositories, API services, `AppDbContext`, or raw `HttpClient` instances inside these view models.

Likely final dependencies for renter-side view models:

- `BoardGames.Shared.ProxyServices.IGameService`;
- `BoardGames.Shared.ProxyServices.IRequestService`;
- `BoardGames.Desktop.Services.ISessionContext`;
- `BoardGames.Desktop.Services.ICurrentUserContext`, if the final Desktop pattern prefers it;
- `BoardGames.Desktop.Services.RequestErrorMapper`;
- a Desktop-owned image helper if image URL/byte rendering needs adaptation.

Move the renter-side pages away from:

- `App.BookingService`;
- `App.SearchAndFilterService`;
- `InterfaceBookingService`;
- `InterfaceSearchAndFilterService`;
- `BookingDTO` as the final Game Details model;
- old `GameDTO` as the final filter card model;
- `BoardGames.Data.Enums.SessionContext`;
- hardcoded or static integer users.

If the existing `DiscoveryView`, `FilteredSearchView`, `GameDetailsView`, and `ConfirmBookingView` are kept, adapt their view models to the final API contracts instead of creating a second parallel UI flow.

If the confirmation page remains, it should be a UI confirmation step only:

```text
Game Details selected dates
-> ConfirmBookingView displays summary
-> Confirm button calls IRequestService.CreateRequestAsync
```

It must not call old direct booking or rental creation behavior.

Use API availability for the calendar. `GetBookedDatesAsync` can mark unavailable dates, and `CheckAvailabilityAsync` can verify the final selected range before request submission.

Do not calculate final business availability in Desktop. Desktop may prevent obviously invalid input, but the API must remain the final decision-maker.

Do not add new API routes from Desktop. If a route or Shared client method is missing, document the dependency for the Task 4, Task 5, or Shared owner instead of bypassing the architecture.

The application may not build perfectly at the start of this task. Do not try to fix the whole solution just to make this task look complete. Fix task-related Desktop/Shared errors only and document unrelated blockers clearly.

## Expected Output

This task should produce:

- working Desktop Filter/Discovery page backed by API data;
- working Desktop search/filter behavior backed by the final games/search contract;
- working Desktop Game Details page backed by real API game details;
- availability display using request/booked-date API data;
- renter-side date selection validation;
- rental request submission through `IRequestService.CreateRequestAsync`;
- friendly UI messages for validation and API errors;
- no hardcoded renter/owner users in this flow;
- no direct database/repository access from this flow;
- clear notes for Task 10 about post-request navigation and chat/notification expectations;
- clear notes for the Shared owner if a game search/details client method or DTO adjustment is required.

## What Counts As Done

Anonymous users can open Desktop and browse/filter active games from the API.

Anonymous users can open Game Details but cannot submit a rental request.

Logged-in users can open Game Details and select a valid date range.

Unavailable dates or unavailable selected ranges are shown clearly.

Logged-in users can submit a rental request through the API.

The request uses the logged-in session account id, not a hardcoded integer user id.

Owner-renting-own-game is blocked with a friendly message.

API errors such as invalid date range, unavailable dates, and game not found are mapped to friendly Desktop messages.

Desktop does not manually create chat messages or notifications for the request. Those side effects are produced by the backend from Tasks 5 and 6.

Filter, Game Details, and request submission all use the same unified backend source.

The task owner documents unrelated blockers instead of taking ownership of the whole solution build.

## Do Not Touch

Do not implement global Desktop routing work owned by the routing person.

Do not take over Task 8 shell/session/API base URL setup.

Do not implement Chat, Notifications, Dashboard, My Games, Account, Admin, or Games management. Those belong to Task 10.

Do not modify the API business rules unless a small contract issue directly blocks this task and the responsible API owner agrees.

Do not create a second request/rental flow in Desktop.

Do not use old standalone My Requests, Others' Requests, My Rentals, or Others' Rentals pages as the final renter-side workflow.

Do not call repositories, `AppDbContext`, or `.Api` internals from Desktop.

Do not use dummy data or hardcoded users to make the flow appear to work.

Do not create chat messages or notifications from Desktop for this request flow.

Do not write tests unless the lead explicitly changes this task.

Do not try to fix unrelated build errors, unrelated warnings, unrelated Web work, unrelated Shared cleanup, unrelated Data migrations, unrelated seed data, or unrelated Task 10 screens.

## Known Blockers And Assumptions

This task assumes Task 7 has made the local API runnable and has verified the games/request route groups in Swagger.

This task assumes Task 8 has activated the final Desktop shell, local API base URL, Shared API clients, and Desktop session context.

This task assumes Task 4 has finalized the game list/search/details contract and image contract.

This task assumes Task 5 has finalized the request creation, availability, and booked-dates contract.

This task assumes Task 6 has connected request creation to chat and notification side effects, or has documented exactly what remains for Task 10 to consume.

This task assumes routing work is handled separately and should be consumed rather than replaced.

This task assumes DB seed/setup is handled separately. The owner may request seed users, games, and booked date ranges, but should not become the seed owner.

If any of these assumptions are not true, document the blocker and coordinate with the responsible task owner instead of expanding Task 9 into backend setup, routing, seed, or Task 10 work.

The current application may not build at the start of this task. The owner should fix errors directly related to Filter, Game Details, request submission, and required Shared API client contracts only. Unrelated Desktop, Web, Shared, Data, test, routing, seed, or API feature errors should be documented rather than silently taken over.
