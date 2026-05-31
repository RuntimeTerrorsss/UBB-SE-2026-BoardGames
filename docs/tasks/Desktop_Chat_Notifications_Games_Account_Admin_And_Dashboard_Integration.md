# Task 10: Desktop Chat, Notifications, Games, Account, Admin, And Dashboard Integration

**Workflow source:** Section 5, Desktop Feature Integration  
**Type:** Parallel Desktop feature lane  
**Can start after:** Task 7 and Task 8 session/API foundation  
**Can run in parallel with:** Task 9  
**Suggested owner:** Desktop feature worker or small pair, because this touches several existing screens  
**Primary project area:** `BoardGames.Desktop`  
**Secondary coordination area:** `BoardGames.Shared` API clients and DTOs needed by Chat, Notifications, Games, Account, Admin, and Dashboard

## What This Task Is About

This task integrates the remaining logged-in Desktop sections with the unified API and the unified Desktop session.

It is broader than Task 9, but most screens already exist. The work is not to rebuild all features from zero. The work is to make the existing Desktop screens use the same backend, the same session, and the same final request/rental/chat/notification workflow.

The final Desktop app must not feel like one old account/games application plus another old chat/filter/dashboard application glued together. After this task, logged-in Desktop areas should behave like one application backed by one API and one account identity.

Task 9 owns the renter-side Filter, Game Details, and request submission path. Task 10 owns the remaining logged-in areas:

```text
Chat
Notifications
Games / My Games / Admin Games
Account
Admin
Dashboard / payment history
final visible menu cleanup
```

## Related Audit Context

Helpful audit documents exist in `docs/audits`. They explain how this task relates to the other tasks in the `.Desktop + .Api` task set and should be read before implementation:

- `docs/audits/desktop-api-final-workflow.md`
- `docs/audits/desktop-api-contract-audit.md`
- `docs/audits/desktop-api-department-task-breakdown.md`
- `docs/audits/desktop-api-10-task-assignment-plan.md`
- `docs/audits/task-1-architecture-boundary-identity-contract.md`
- `docs/audits/task-1-boundary-violation-inventory.md`
- `docs/audits/task-7-api-runtime-wiring-analysis.md`

This task corresponds to Task 10 in the 10-task plan. It belongs to the Desktop integration lane after the local backend is runnable and after Task 8 has chosen one Desktop shell, one API base URL, and one session context.

This task should not redo Task 4 games API work, Task 5 request/rental lifecycle API work, Task 6 chat/notification/payment side effects, Task 7 backend runtime wiring, Task 8 shell/session setup, or Task 9 Filter/Game Details/request submission. It should consume those results through Shared API clients and the Desktop session context.

## Where This Fits In The Workflow

The intended department workflow is:

```text
Task 1 -> Task 2 -> Tasks 3 / 4 / 5 / 6 -> Task 7 -> Tasks 8 / 9 / 10
```

Task 10 can begin once:

- Task 7 has the local API running with the final route groups;
- Task 8 has one Desktop shell, one API base URL, and one Desktop session;
- Task 3 has stable auth/account/admin routes;
- Task 4 has stable games routes;
- Task 5 has stable request/rental approve/deny routes;
- Task 6 has stable chat, notification, and payment/dashboard routes.

Task 10 can run in parallel with Task 9 because Task 9 owns the renter-side request entry flow, while Task 10 owns the logged-in shell sections that display, manage, or react to the same backend state.

Routing work and DB seed/setup work are assumed to be handled by separate owners. This task may identify which menu entries, navigation targets, and seed data it needs, but it should not become the routing task or the seed task.

## Main Goal

Transform the current Desktop logged-in areas from this:

```text
menu still exposes old My Requests / Others' Requests / My Rentals / Others' Rentals
chat uses old integer user ids and hardcoded Alice/Bob behavior
notifications are partly wired to account identity but must follow the final request flow
games/account/admin use newer API-client patterns in places
dashboard/payment history still uses old payment services and old integer user state
some views create services directly through App static properties
some view models use old namespaces or old proxy/service patterns
```

Into this:

```text
Chat loads conversations for the logged-in session user
Chat shows rental request messages from the final backend request flow
Notifications load for the logged-in session account
Notifications show request-created and request-result events from the final backend flow
Games uses the final games API and same session role/account
Account uses the final account API and same session
Admin uses the final admin API and is visible only for admin users
Dashboard/payment history reads real API data for the logged-in account
old duplicate request/rental menu paths are not part of the final visible flow
```

## Important Direction For Notifications

This task must make Desktop notifications follow the final rental request workflow.

Notifications must no longer be driven by the old Project 1 standalone request/rental pages:

```text
My Requests
Others' Requests
My Rentals
Others' Rentals
direct create rental/request pages
```

Those pages are not the final business workflow.

The final notification source is the Project 2-style request flow that starts from Game Details and is connected to Chat:

```text
Filter / Game Details
-> user selects rental dates
-> user sends rental request
-> API creates request
-> API creates chat rental request message
-> API creates owner notification
-> owner accepts or declines
-> API updates request/chat state
-> API creates renter notification
```

Desktop must display notifications from this final API flow. Desktop must not create a second notification workflow by reviving old request/rental pages.

Bad result:

```text
Notifications still depend on old Project 1 request/rental pages,
while Chat uses the newer Project 2 rental request flow.
```

Good result:

```text
A rental request made from Game Details appears in Chat and Notifications,
and accepting/declining it updates the same request/chat/notification state.
```

If old Project 1 request/rental screens contain useful UI ideas, those ideas can be reused visually. They must not define the final notification source or final backend workflow.

## Backend Side Effects Are Not Desktop Logic

Task 10 does not implement backend notification side effects.

The backend is expected to create notifications and chat message state from the final request lifecycle from Tasks 5 and 6:

```text
request created
-> owner chat rental request message
-> owner notification

request accepted
-> rental request chat message finalized as accepted
-> renter notification

request declined
-> rental request chat message finalized as declined
-> renter notification
```

Desktop should call the canonical request/chat/notification APIs and then refresh the UI from the API.

If creating, approving, or declining a request does not produce the expected chat or notification state, that is a backend Task 5/6/7 blocker. Do not patch that behavior inside Desktop by manually creating fake notifications or fake chat messages.

## Current State From The Codebase

The current Desktop project has pieces of the final logged-in flow, but they are not consistently active or unified.

Relevant project file:

- `BoardGamesApp/BoardGames.Desktop/BoardGames.Desktop.csproj`

The project currently references:

- `BoardGames.Shared`
- `ServerCommunication`

It does not currently reference `BoardGames.Api` or `BoardGames.Data`, which matches the final dependency direction. However, many older Desktop files still use old services, old session concepts, or old static `App` properties. Some old project 2 pages are currently excluded from compilation in the project file, including chat, dashboard, payment history, and several filter/payment pages. Task 10 should coordinate with Task 8 before changing which Desktop pages are active.

Relevant shell/menu files:

- `BoardGamesApp/BoardGames.Desktop/Views/MenuBarPage.xaml`
- `BoardGamesApp/BoardGames.Desktop/Views/MenuBarPage.xaml.cs`
- `BoardGamesApp/BoardGames.Desktop/ViewModels/MenuBarViewModel.cs`
- `BoardGamesApp/BoardGames.Desktop/ViewModels/AppPage.cs`

Current menu behavior still includes old request/rental entries:

```text
My Requests
My Rentals
Others' Requests
Others' Rentals
```

Those entries are not part of the final visible workflow if they duplicate the new request/chat/notification lifecycle. The final menu should expose the final application areas instead of preserving both old applications.

Relevant session and authorization files from Task 8:

- `BoardGamesApp/BoardGames.Desktop/Services/ISessionContext.cs`
- `BoardGamesApp/BoardGames.Desktop/Services/SessionContext.cs`
- `BoardGamesApp/BoardGames.Desktop/Services/ICurrentUserContext.cs`
- `BoardGamesApp/BoardGames.Desktop/Services/CurrentUserContext.cs`
- `BoardGamesApp/BoardGames.Desktop/Services/IDesktopAuthorizationService.cs`
- `BoardGamesApp/BoardGames.Desktop/Services/DesktopAuthorizationService.cs`

Task 10 should use these as the final session and authorization source. It should not use `BoardGames.Data.Enums.SessionContext`, `SessionContext.GetInstance().UserId`, or hardcoded `MainWindow.loggedInUserAlice` / `MainWindow.loggedInUserBob` as final behavior.

## Current Chat State

Relevant Desktop chat files:

- `BoardGamesApp/BoardGames.Desktop/Views/ChatViews/ChatPageView.xaml`
- `BoardGamesApp/BoardGames.Desktop/Views/ChatViews/ChatPageView.xaml.cs`
- `BoardGamesApp/BoardGames.Desktop/Views/ChatViews/ChatView.xaml`
- `BoardGamesApp/BoardGames.Desktop/Views/ChatViews/LeftPanelView.xaml`
- `BoardGamesApp/BoardGames.Desktop/Views/ChatViews/RightPanelView.xaml`
- `BoardGamesApp/BoardGames.Desktop/ViewModels/ChatPageViewModel.cs`
- `BoardGamesApp/BoardGames.Desktop/ViewModels/ChatViewModel.cs`
- `BoardGamesApp/BoardGames.Desktop/ViewModels/LeftPanelViewModel.cs`
- `BoardGamesApp/BoardGames.Desktop/ViewModels/MessageViewModel.cs`
- `BoardGamesApp/BoardGames.Desktop/ViewModels/ConversationPreviewModel.cs`

The current chat code still follows the old integer-user workflow. It uses:

- `ConversationService`
- `IUserRepository`
- `App.ConversationRepository`
- `App.UserRepository`
- `App.ConversationNotifier`
- `SessionContext.GetInstance().UserId`
- `MainWindow.loggedInUserAlice`
- `MainWindow.loggedInUserBob`

That is not the final Desktop direction.

The final chat Desktop behavior should use the logged-in session from Task 8 and the canonical conversation/request APIs from Tasks 5 and 6. If Shared does not yet expose a conversation API client, this task may add or coordinate the smallest required Shared client contract for the existing backend routes. It should not reintroduce repository-style clients or direct API service references in Desktop.

Relevant backend routes currently exist under:

```text
GET  api/Conversation/user/{accountId}
GET  api/Conversation/{id}
POST api/Conversation
POST api/Conversation/messages
PUT  api/Conversation/messages
POST api/Conversation/readreceipt
POST api/Conversation/rental/finalize/{requestId}
```

Request accept/decline routes also exist under:

```text
PUT api/requests/{requestId}/approve
PUT api/requests/{requestId}/deny
PUT api/requests/{requestId}/cancel
```

For accept/decline, Desktop should prefer the canonical request routes. The backend should then finalize chat state and create notifications.

## Current Notifications State

Relevant Desktop notification files:

- `BoardGamesApp/BoardGames.Desktop/Views/NotificationsPage.xaml`
- `BoardGamesApp/BoardGames.Desktop/Views/NotificationsPage.xaml.cs`
- `BoardGamesApp/BoardGames.Desktop/ViewModels/NotificationsViewModel.cs`
- `BoardGamesApp/BoardGames.Desktop/Services/IDesktopNotificationService.cs`
- `BoardGamesApp/BoardGames.Desktop/Services/DesktopNotificationService.cs`
- `BoardGamesApp/BoardGames.Desktop/Services/IToastNotificationService.cs`
- `BoardGamesApp/BoardGames.Desktop/Services/ToastNotificationService.cs`

The notifications view model already follows the right idea in places. It uses:

- `ICurrentUserContext`
- `IDesktopNotificationService`
- `LoadNotificationsForUserAsync(Guid accountId)`

This is close to the final direction.

However, the current notification service still has namespace and contract drift. `DesktopNotificationService` aliases `BoardGames.ApiClient.INotificationService` and `BoardGames.Utilities.ICurrentUserContext`, while the current Shared API clients live under `BoardGames.Shared.ProxyServices` and the Desktop session context lives under `BoardGames.Desktop.Services`.

The final notification path should use the actual Shared notification client and Task 8 session context.

Relevant backend routes currently exist under:

```text
GET    api/notifications/user/{accountId}
GET    api/notifications/{notificationId}
PUT    api/notifications/{notificationId}
DELETE api/notifications/{notificationId}
```

The current Shared notification client also contains methods for persisting notifications and deleting notifications by request. If the backend does not expose a matching route, do not fake it in Desktop. Coordinate the missing API contract with the backend owner or remove the Desktop dependency on unsupported routes.

## Current Games State

Relevant Desktop game-management files:

- `BoardGamesApp/BoardGames.Desktop/Views/ListingsPage.xaml`
- `BoardGamesApp/BoardGames.Desktop/Views/ListingsPage.xaml.cs`
- `BoardGamesApp/BoardGames.Desktop/Views/CreateGameView.xaml`
- `BoardGamesApp/BoardGames.Desktop/Views/CreateGameView.xaml.cs`
- `BoardGamesApp/BoardGames.Desktop/Views/EditGameView.xaml`
- `BoardGamesApp/BoardGames.Desktop/Views/EditGameView.xaml.cs`
- `BoardGamesApp/BoardGames.Desktop/ViewModels/ListingsViewModel.cs`
- `BoardGamesApp/BoardGames.Desktop/ViewModels/CreateGameViewModel.cs`
- `BoardGamesApp/BoardGames.Desktop/ViewModels/EditGameViewModel.cs`

The `ListingsViewModel` already moves in the final direction. It uses:

- `IGameService`
- `IDesktopAuthorizationService`
- current account id;
- admin role check.

Task 10 should finish this direction and make Games/My Games/Admin Games use the canonical games API from Task 4.

Relevant backend routes currently exist under:

```text
GET    api/games
GET    api/games/{gameId}
GET    api/games/owner/{ownerAccountId}
GET    api/games/owner/{ownerAccountId}/active
GET    api/games/admin
POST   api/games
PUT    api/games/{gameId}
DELETE api/games/{gameId}
POST   api/games/search
```

The current games API still has temporary query/body identity parameters in places. Desktop should use the agreed Task 4/Task 7 contract. If the final API still requires `requestingAccountId` or `isAdmin`, Desktop may pass those from the Task 8 session and authorization service, but it should not hardcode them.

## Current Account And Admin State

Relevant Desktop account/admin files:

- `BoardGamesApp/BoardGames.Desktop/Views/ProfilePage.xaml`
- `BoardGamesApp/BoardGames.Desktop/Views/ProfilePage.xaml.cs`
- `BoardGamesApp/BoardGames.Desktop/ViewModels/ProfileViewModel.cs`
- `BoardGamesApp/BoardGames.Desktop/Views/AdminPage.xaml`
- `BoardGamesApp/BoardGames.Desktop/Views/AdminPage.xaml.cs`
- `BoardGamesApp/BoardGames.Desktop/ViewModels/AdminViewModel.cs`

The admin view model already follows the final direction in places. It uses:

- `IAdminService`
- `IDesktopAuthorizationService`
- admin-only access checks.

The profile view model also uses session fields and API service concepts. However, it currently aliases `BoardGames.ApiClient.IAccountService` and `BoardGames.ApiClient.IAuthService`. The active Shared proxy services live under `BoardGames.Shared.ProxyServices`. Task 10 should align account/admin/profile code with the actual Task 8 API-client registration and session context.

Relevant backend routes currently exist under:

```text
GET    api/accounts/{accountId}
PUT    api/accounts/{accountId}
PUT    api/accounts/{accountId}/password
POST   api/accounts/{accountId}/avatar
DELETE api/accounts/{accountId}/avatar
GET    api/admin/accounts
PUT    api/admin/accounts/{accountId}/suspend
PUT    api/admin/accounts/{accountId}/unsuspend
PUT    api/admin/accounts/{accountId}/reset-password
PUT    api/admin/accounts/{accountId}/unlock
```

Admin visibility must be controlled by the Desktop authorization service and enforced by the API. Hiding the Admin menu in Desktop is useful, but it is not a substitute for API authorization.

## Current Dashboard And Payment History State

Relevant Desktop dashboard/payment files:

- `BoardGamesApp/BoardGames.Desktop/Views/DashboardView.xaml`
- `BoardGamesApp/BoardGames.Desktop/Views/DashboardView.xaml.cs`
- `BoardGamesApp/BoardGames.Desktop/Views/PaymentHistoryView.xaml`
- `BoardGamesApp/BoardGames.Desktop/Views/PaymentHistoryView.xaml.cs`
- `BoardGamesApp/BoardGames.Desktop/ViewModels/PaymentHistoryViewModel.cs`

The current dashboard/payment path still follows old service patterns. It uses:

- `App.ServicePayment`
- old payment service interfaces;
- old integer user state;
- `SessionContext.GetInstance().UserId`;
- old direct navigation from Discovery/old pages.

That is not the final Desktop direction.

Relevant backend route currently exists for user-specific payment history:

```text
GET api/Payments/user/{accountId}/history
```

The current Shared `ProxyServices` folder does not clearly expose a payment/dashboard API client. Task 10 may need to add or coordinate a small Shared API client contract for dashboard/payment history. It should not keep using old local payment services or dummy data as the final flow.

## Concrete Responsibilities

Chat:

- Load conversations for the current Desktop session user.
- Use the Task 8 session account id and optional `PamUserId` only where the API contract requires it.
- Show rental request messages created by the final backend request flow.
- Remove hardcoded Bob/Alice/Carol/static user switching.
- Remove final dependency on `MainWindow.loggedInUserAlice` and `MainWindow.loggedInUserBob`.
- Remove final dependency on `SessionContext.GetInstance().UserId`.
- Support accept/decline/finalize action if the final Desktop UI owns that action.
- When accepting or declining a request, call the canonical request API and refresh chat/notification state from the backend.

Notifications:

- Load notifications for the current Desktop session user.
- Show owner notification when a rental request is created.
- Show renter notification when a request is accepted or declined.
- Use the canonical notification API from Task 6.
- Do not load notifications from old Project 1 request/rental page logic.
- Do not manually create fake Desktop-only notifications for request side effects.
- Support read/delete/update if required by the final API contract.

Games:

- Standard user sees and manages only their own games.
- Admin can see and manage all games.
- Create, edit, delete, or deactivate games through the unified games API.
- Use session account id and role from Task 8.
- Do not use local repositories, `AppDbContext`, or old Data services from Desktop.

Account:

- Profile loads from API.
- Profile updates through API.
- Password/avatar behavior uses API if kept.
- Account page uses the same Desktop session populated by login.
- After profile update, refresh the Desktop session if profile/session fields changed.
- Logout clears the Task 8 session and returns to the agreed shell route.

Admin:

- Admin section is visible only for admin users.
- Admin account operations call the unified API.
- Admin can manage all games through the Games area.
- Non-admin users must not be able to access Admin by direct navigation.
- API-side authorization failures must show friendly Desktop errors.

Dashboard:

- Dashboard/payment history uses real API data.
- Data is user-specific.
- No dummy-only payment/history data remains in the final demo flow.
- Payment history should read by current session account id.
- If payment actions are kept in Desktop, they must use API-backed services and the same request/rental/chat identifiers.

Menu and visible flow:

- Remove old duplicate request/rental navigation from the final visible menu.
- Do not expose My Requests, Others' Requests, My Rentals, and Others' Rentals as separate final workflows if they duplicate the new chat/request/notification lifecycle.
- Keep or reuse only UI pieces that serve the final workflow.
- The visible Desktop navigation should make the application feel like one merged product.

## Required Final User Flow

The flow that proves this task is connected correctly is:

```text
user logs in
-> Desktop session is populated
-> user can open Chat
-> Chat loads conversations for that session user
-> owner sees rental request message from Task 9 request submission
-> owner also sees matching notification
-> owner accepts or declines through the final UI/API path
-> renter sees accepted/declined notification
-> Dashboard/payment history reads real API data for the current session
-> My Games/Admin Games/Account/Admin all use the same session and backend
```

The important part is that Chat and Notifications reflect the same backend request/rental state. If they show different realities, the merge is not complete.

## Expected Output

This task should produce:

- Chat using real session and API;
- Notifications using real session and API;
- Notifications following the final Game Details -> request -> chat -> notification flow;
- Games using real session and API;
- Account using real session and API;
- Admin using real session, role checks, and API;
- Dashboard/payment history using real session and API;
- old duplicate request/rental navigation removed from the final visible flow;
- missing Shared API-client contracts identified or added only where needed for this task;
- clear blockers documented for missing backend routes instead of Desktop workarounds.

## What Counts As Done

Chat loads conversations for the logged-in Desktop session user.

Chat shows rental request messages created by the final backend request flow.

Chat no longer depends on hardcoded Alice/Bob/Carol/static user switching as final behavior.

Owner sees a rental request notification after a renter sends a request.

Renter sees accepted or declined notification after the owner responds.

Chat and Notifications refer to the same backend request/rental state.

Old Project 1 request/rental pages are not the active source of final notifications.

Standard user can manage only their own games.

Admin can manage accounts and all games through API-backed UI.

Account page loads and updates the logged-in account through the API.

Dashboard/payment history reads real backend data for the logged-in account.

Duplicate old request/rental navigation is removed from the final visible Desktop menu.

The application does not visually feel like two old applications stitched together.

## Do Not Touch

Do not implement Task 8 shell/session/API base URL setup.

Do not implement Task 9 Filter, Game Details, or rental request submission.

Do not implement backend request/chat/notification side effects inside Desktop.

Do not revive old My Requests/My Rentals/Others' Requests/Others' Rentals as the final business workflow.

Do not use `AppDbContext`, repositories, or API service classes directly from Desktop.

Do not reintroduce `BoardGames.Desktop -> BoardGames.Api` or `BoardGames.Desktop -> BoardGames.Data` project references.

Do not use hardcoded users to make Chat, Dashboard, Notifications, or Games appear to work.

Do not use dummy payment/dashboard data as the final demo path.

Do not make tests for this task unless the lead explicitly asks for tests.

Do not try to fix the whole solution build. Fix only errors directly related to this task's Desktop feature areas and document unrelated blockers.

## Known Blockers And Assumptions

This task assumes Task 7 has produced a locally runnable API and a stable local API URL.

This task assumes Task 8 has activated one Desktop shell, one API-client configuration, and one Desktop session context.

This task assumes Task 9 creates rental requests through the canonical request API.

This task assumes backend request side effects from Tasks 5 and 6 are active. Desktop should verify them by refreshing Chat and Notifications, not by recreating them locally.

The current Desktop project may not build perfectly before this task starts. The implementer should not repair unrelated Web, API, test, or feature-page problems only to make the full solution green.

The current Shared `ProxyServices` set does not clearly include final conversation and payment/dashboard clients. If those clients are missing, this task may add the smallest required Shared API-client contracts or coordinate with the Shared owner. It should not fall back to repository-shaped proxies.

The current notifications client and Desktop notification service show namespace/route drift. If a Desktop method references a route that the API does not expose, report the contract mismatch and coordinate the fix instead of inventing a Desktop-only workaround.

The final proof of this task is not that every old page is preserved. The final proof is that the remaining logged-in Desktop areas use one session, one backend, and one final workflow.
