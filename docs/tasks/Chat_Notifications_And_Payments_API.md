# Task 6: Chat, Notifications, And Payments API

**Workflow source:** Section 2, API Cleanup  
**Type:** Parallel API feature lane, high risk  
**Can start after:** Task 1 and Task 2 route/service decisions  
**Can run in parallel with:** Tasks 3, 4, 5, but must coordinate strongly with Task 5  
**Suggested owner:** backend worker comfortable with cross-feature workflows  
**Primary project area:** `BoardGames.Api`  
**Secondary coordination area:** `BoardGames.Shared`, `BoardGames.Desktop`, `BoardGames.Data`

## What This Task Is About

This task connects the rental request lifecycle to user-visible chat, notifications, and dashboard/payment data.

Task 5 owns the request/rental lifecycle itself. Task 6 owns what happens around that lifecycle so the user can actually see it:

```text
request created
-> chat message appears
-> owner notification appears

request accepted or declined
-> chat message is finalized
-> renter notification appears
-> payment/dashboard history can use real data
```

This task is very important for proving that the two old projects are now one application. The final app must not have one request/rental flow for old project 1 notifications and another request/rental flow for old project 2 chat.

The final behavior must be one unified event flow:

```text
Game Details request
-> canonical request from Task 5
-> chat rental request message
-> notification linked to that same request
-> approval/decline updates the same chat/request/notification/payment history
```

## Canonical Direction For Chat, Notifications, And Payments

Task 6 must connect the final request/rental lifecycle from Task 5 to chat, notifications, and dashboard/payment history.

The final workflow is not the old project 1 notification flow based on standalone My Requests / Others' Requests / My Rentals / Others' Rentals pages.

The final workflow is the project 2-style flow:

```text
User opens Game Details
-> user selects dates
-> user creates rental request
-> owner receives a chat rental request message
-> owner receives a notification about the request
-> owner accepts or declines
-> renter receives notification about the result
-> rental/payment/dashboard state uses the same request/rental identifiers
```

The important rule is that request/rental events must produce both chat state and notification state from the same source of truth.

For example:

```text
request created
-> create or find renter-owner conversation
-> create rental request chat message
-> create owner notification linked to the same request

request accepted
-> mark/finalize rental request chat message
-> create renter notification linked to the same request/rental
-> expose payment/dashboard data from the confirmed rental

request declined
-> mark/finalize rental request chat message
-> create renter notification linked to the same request
```

The project must not keep one notification behavior for old project 1 requests/rentals and another notification behavior for project 2 chat rental requests. Notifications must follow the final request lifecycle from Task 5.

Useful old API behavior should not be deleted only for cleanup. If an old controller, service, endpoint, or repository method contains logic still needed by chat, notifications, or payments, that behavior must be moved into the canonical service/API path or explicitly assigned to another task.

The goal is to remove duplicate final flows, not to destroy useful endpoint behavior.

Bad result:

```text
Old request notifications still depend on project 1 request pages,
while chat rental requests use a different project 2 flow.
```

Good result:

```text
One request event creates one chat message and one notification path,
and every UI reads that same request/rental state.
```

## Related Audit Context

Helpful audit documents exist in `docs/audits`. They explain how this task relates to the other tasks in the `.Desktop + .Api` task set and should be read before implementation:

- `docs/audits/desktop-api-final-workflow.md`
- `docs/audits/desktop-api-contract-audit.md`
- `docs/audits/desktop-api-department-task-breakdown.md`
- `docs/audits/desktop-api-10-task-assignment-plan.md`

This task corresponds to Task 6 in the 10-task plan. It belongs to the parallel API feature lane after Task 1 and Task 2. It should not redo the architecture boundary work from Task 1 or the duplicate route ownership decisions from Task 2.

## Where This Fits In The Workflow

The intended department workflow is:

```text
Task 1 -> Task 2 -> Tasks 3 / 4 / 5 / 6 -> Task 7 -> Tasks 8 / 9 / 10
```

Task 6 can run in parallel with:

- Task 3, Auth / Account / Admin API;
- Task 4, Games / Filter / Search API;
- Task 5, Request / Rental Lifecycle API.

Task 6 must coordinate strongly with Task 5. Task 5 decides the request/rental truth and state transitions. Task 6 decides the chat, notification, payment, and dashboard side effects that make those state transitions visible to users.

Task 6 depends on Task 3 for the final account identity and session user contract. It depends on Task 4 for game names, owners, prices, and other game details needed in messages, notifications, and dashboard rows. It depends on Task 5 for request ids, rental ids, request status, approval/decline rules, and availability outcomes.

Routing work and DB seed/setup work are assumed to be handled by separate owners. This task may document which seeded conversations, notifications, payments, users, games, requests, and rentals are needed, but it should not become the routing task or the seed task.

## Main Goal

Transform the current state from this:

```text
chat uses old integer user ids
notifications use account identity
payments and dashboard still rely on old payment/rental concepts
conversation controller exposes repository behavior directly
payment controller exposes repository behavior directly
request/rental events do not have one clear side-effect rule
old project 1 request notifications and old project 2 chat requests can drift apart
```

Into this:

```text
one canonical conversation API
one canonical notification API
one canonical payment/dashboard API contract
request-created creates chat and notification side effects
request-accepted/request-declined finalizes chat and creates renter notification
payment/dashboard history reads real request/rental/payment data
chat, notifications, payments, requests, and rentals share the same identifiers
old duplicate flows are quarantined only after useful behavior is preserved
```

## Current State From The Codebase

The active notification route already exists in:

- `BoardGamesApp/BoardGames.Api/Controllers/NotificationsController.cs`
- `BoardGamesApp/BoardGames.Api/Services/INotificationService.cs`
- `BoardGamesApp/BoardGames.Api/Services/NotificationService.cs`
- `BoardGamesApp/BoardGames.Api/Mappers/NotificationMapper.cs`

The notification controller uses the service layer and exposes:

```text
GET api/notifications/user/{accountId}
GET api/notifications/{notificationId}
PUT api/notifications/{notificationId}
DELETE api/notifications/{notificationId}
```

The notification service already supports:

- loading notifications for an account id;
- creating notifications for an account id;
- updating and deleting notifications;
- deleting notifications linked to a request id;
- preserving `RelatedRequestId` on notification DTOs.

This is close to the final identity direction, because it uses `Guid` account ids. Task 6 should preserve this service-layer direction.

The active conversation route currently exists in:

- `BoardGamesApp/BoardGames.Api/Controllers/ConversationController.cs`
- `BoardGamesApp/BoardGames.Api/Services/IConversationService.cs`
- `BoardGamesApp/BoardGames.Api/Services/ConversationService.cs`
- `BoardGamesApp/BoardGames.Api/Services/IConversationNotifier.cs`
- `BoardGamesApp/BoardGames.Api/Services/ConversationNotifier.cs`

The conversation controller currently exposes repository-shaped behavior directly through `IConversationRepository`, not through `IConversationService`. It also uses old `BookingBoardGames.*` namespaces and integer user ids.

Current conversation routes include:

```text
GET api/conversation/user/{userId}
GET api/conversation/{id}
GET api/conversation/{id}/participants
POST api/conversation
POST api/conversation/messages
PUT api/conversation/messages
POST api/conversation/readreceipt
POST api/conversation/rental/finalize/{messageId}
POST api/conversation/cash/{parentMessageId}/{paymentId}
```

This behavior is useful, especially conversation lookup, message sending, rental request messages, rental request finalization, read receipts, and cash agreement messages. However, the final API should not leave repository-based conversation controllers as the active route owner if Task 2 chose service-layer APIs as the final direction.

The active payment route currently exists in:

- `BoardGamesApp/BoardGames.Api/Controllers/PaymentsController.cs`
- `BoardGamesApp/BoardGames.Api/Services/IServicePayment.cs`
- `BoardGamesApp/BoardGames.Api/Services/ServicePayment.cs`
- `BoardGamesApp/BoardGames.Api/Services/IPaymentService.cs`
- `BoardGamesApp/BoardGames.Api/Services/PaymentService.cs`
- `BoardGamesApp/BoardGames.Api/Services/ICardPaymentService.cs`
- `BoardGamesApp/BoardGames.Api/Services/CardPaymentService.cs`
- `BoardGamesApp/BoardGames.Api/Services/ICashPaymentService.cs`
- `BoardGamesApp/BoardGames.Api/Services/CashPaymentService.cs`

The payment controller currently injects repositories directly and exposes:

```text
GET api/payments
GET api/payments/{id}
GET api/payments/history
GET api/payments/history/{id}
POST api/payments
PUT api/payments/{id}
DELETE api/payments/{id}
```

The payment service layer contains useful dashboard/payment-history behavior, but it still depends on old namespaces, old integer session context, and old integer user ids. It also builds payment history by combining payment, rental, and rental request message data.

This task should preserve useful payment/dashboard behavior while moving the final API toward one service-layer route contract that uses the final identity decision from Task 3.

Shared DTOs and clients involved in this area include:

- `BoardGamesApp/BoardGames.Shared/DTO/ConversationDTO.cs`
- `BoardGamesApp/BoardGames.Shared/DTO/MessageDTO.cs`
- `BoardGamesApp/BoardGames.Shared/DTO/MessageType.cs`
- `BoardGamesApp/BoardGames.Shared/DTO/NotificationDTO.cs`
- `BoardGamesApp/BoardGames.Shared/DTO/NotificationType.cs`
- `BoardGamesApp/BoardGames.Shared/DTO/PaymentDTO.cs`
- `BoardGamesApp/BoardGames.Shared/DTO/CardPaymentDTO.cs`
- `BoardGamesApp/BoardGames.Shared/DTO/CashPaymentDTO.cs`
- `BoardGamesApp/BoardGames.Shared/DTO/IncomingNotification.cs`
- `BoardGamesApp/BoardGames.Shared/ProxyServices/INotificationService.cs`
- `BoardGamesApp/BoardGames.Shared/ProxyServices/NotificationService.cs`

Important Shared client detail: notification already has a newer `ProxyServices` API client, but conversation and payment still appear as old `ProxyRepositories` clients:

- `BoardGamesApp/BoardGames.Shared/ProxyRepositories/ConversationAPIProxy.cs`
- `BoardGamesApp/BoardGames.Shared/ProxyRepositories/PaymentAPIProxy.cs`
- `BoardGamesApp/BoardGames.Shared/ProxyRepositories/RepositoryPaymentAPIProxy.cs`

Task 6 should define the final API contract that a Shared service-client can call. The actual broad Shared client cleanup may require coordination with the Shared department, but the final API routes and DTO needs must be clear here.

Data repositories and models involved include:

- `BoardGamesApp/BoardGames.Data/Models/Conversation.cs`
- `BoardGamesApp/BoardGames.Data/Models/ConversationParticipant.cs`
- `BoardGamesApp/BoardGames.Data/Models/Message.cs`
- `BoardGamesApp/BoardGames.Data/Models/Notification.cs`
- `BoardGamesApp/BoardGames.Data/Models/Payment.cs`
- `BoardGamesApp/BoardGames.Data/Repositories/IConversationRepository.cs`
- `BoardGamesApp/BoardGames.Data/Repositories/ConversationRepository.cs`
- `BoardGamesApp/BoardGames.Data/Repositories/INotificationRepository.cs`
- `BoardGamesApp/BoardGames.Data/Repositories/NotificationRepository.cs`
- `BoardGamesApp/BoardGames.Data/Repositories/IPaymentRepository.cs`
- `BoardGamesApp/BoardGames.Data/Repositories/PaymentRepository.cs`
- `BoardGamesApp/BoardGames.Data/Repositories/IRepositoryPayment.cs`
- `BoardGamesApp/BoardGames.Data/Repositories/RepositoryPayment.cs`

Desktop screens and view models that will consume this task later include:

- `BoardGamesApp/BoardGames.Desktop/Views/ChatViews/ChatPageView.xaml`
- `BoardGamesApp/BoardGames.Desktop/Views/ChatViews/ChatView.xaml`
- `BoardGamesApp/BoardGames.Desktop/Views/NotificationsPage.xaml`
- `BoardGamesApp/BoardGames.Desktop/Views/DashboardView.xaml`
- `BoardGamesApp/BoardGames.Desktop/Views/PaymentHistoryView.xaml`
- `BoardGamesApp/BoardGames.Desktop/Views/CardPaymentPage.xaml`
- `BoardGamesApp/BoardGames.Desktop/Views/CashPaymentPage.xaml`
- matching view models under `BoardGamesApp/BoardGames.Desktop/ViewModels`

Desktop integration belongs mostly to Task 10. Task 6 should provide the API contract that Task 10 can consume.

## Owned Route Areas

This task owns the final implementation direction for these route areas:

```text
api/conversations
api/notifications
api/payments
api/payment-history, if this is the chosen final dashboard route
```

The final route table should cover:

- list conversations for the logged-in account;
- get one conversation;
- create or find conversation between renter and owner;
- send normal chat message;
- create rental request chat message from a Task 5 request;
- update or finalize rental request message after accept/decline;
- send read receipts if still part of the final chat UI;
- create cash agreement message if still part of the final payment flow;
- list notifications for the logged-in account;
- mark notification as read/update notification if kept;
- delete notification if kept;
- list payment/dashboard history for the logged-in account;
- get receipt/payment details if kept;
- create card/cash payment records if the final payment flow includes them.

If route names remain singular, such as `api/conversation`, the owner must record whether that is final or compatibility-only. The preferred final route name should be consistent with the other route groups.

## Required Behavior

The canonical chat API must support:

- getting conversations for the logged-in user;
- creating or finding a conversation between renter and owner;
- sending and updating normal messages if chat supports free text;
- creating a rental request message linked to the request id from Task 5;
- finalizing a rental request message when the request is accepted or declined;
- preserving request id and payment id links on message DTOs;
- using the final identity contract from Task 3 instead of hardcoded integer users where possible.

The canonical notification API must support:

- loading notifications for the logged-in account;
- creating owner notification when a request is created;
- creating renter notification when a request is accepted;
- creating renter notification when a request is declined;
- linking request-related notifications to the same request id from Task 5;
- updating or deleting notifications if kept in final UI;
- not relying on old project 1 standalone request/rental pages as the source of notification truth.

The canonical payment/dashboard API must support:

- exposing payment/dashboard history based on real backend data;
- showing request/rental/payment status from real request, rental, payment, and message records;
- linking payment history rows to the confirmed rental or request/rental identifiers from Task 5;
- preserving card/cash payment behavior if it is part of the final app;
- replacing dummy-only dashboard/payment history paths.

## Required Contract With Task 5

Task 5 and Task 6 must agree on one side-effect contract.

At minimum, Task 5 should expose or trigger enough data for Task 6 to know:

- request id;
- rental id after approval;
- renter account id;
- renter legacy/PAM user id if conversation still needs integer participants temporarily;
- owner account id;
- owner legacy/PAM user id if conversation still needs integer participants temporarily;
- game id;
- game name;
- date range;
- request status;
- approval or decline event.

Task 6 should define what happens for each event:

```text
request created
-> create/find renter-owner conversation
-> create rental request chat message
-> create owner notification

request accepted
-> finalize rental request chat message as accepted
-> create renter notification
-> expose confirmed rental/payment/dashboard data

request declined
-> finalize rental request chat message as declined
-> create renter notification
```

If Task 5 currently creates notifications directly, Task 6 must decide whether that remains acceptable or whether notification side effects should move behind a clearer event/side-effect service. The important result is not the exact class name. The important result is that request/rental state, chat messages, and notifications cannot drift into separate flows.

## Current Problems This Task Addresses

- Chat still uses old integer user IDs in places.
- Notifications use account identity and are still influenced by old project 1 request/rental logic.
- Rental request creation must produce both chat state and notification state from the same final workflow.
- A request sent in chat must automatically create the matching notification.
- Accepting or declining a request must update/finalize the chat message and create the matching renter notification.
- Dashboard/payment history must stop being dummy-only and must read from real request/rental/payment data.
- Conversation, notification, and payment APIs are not yet part of one unified flow.
- Useful API logic must be preserved, but duplicate old flows must be quarantined or merged into the canonical services.
- Conversation and payment controllers still expose repositories directly.
- Conversation/payment code still imports old namespaces and uses old integer session/user concepts.
- Shared has notification API clients in the newer service-client style, but conversation/payment still have repository-shaped API proxies.

## Coordination With Other Tasks

Coordinate with Task 3 for:

- final account id format;
- mapping between account id and legacy/PAM user id if chat still needs integer user ids temporarily;
- logged-in user identity for notification, conversation, and payment queries;
- admin access if admins can inspect other users' notifications, conversations, or payments.

Coordinate with Task 4 for:

- game name;
- game owner;
- game price;
- game image or summary if chat/payment/dashboard displays it.

Coordinate with Task 5 for:

- request creation event;
- request approval event;
- request decline event;
- request id and rental id ownership;
- request/rental status rules;
- whether denied/cancelled requests remain queryable for chat and dashboard history.

Coordinate with Task 7 for:

- dependency injection registrations for conversation, notification, payment, mapper, repository, and notifier services;
- local backend smoke flow;
- required seeded data for conversations, notifications, payments, requests, rentals, users, and games.

Coordinate with Task 10 for:

- Desktop Chat API contract;
- Desktop Notifications API contract;
- Desktop Dashboard/Payment History API contract;
- error states and empty states that Desktop should display.

## Implementation Hints

Use the service-layer notification path as the likely base for final notifications:

- `NotificationsController` should continue to call `INotificationService`;
- `NotificationService` should continue to own notification creation and retrieval;
- request-related notifications should keep a request id link through `RelatedRequestId`.

Do not let old project 1 request/rental pages remain the final source of notifications. Notifications should follow the canonical request lifecycle from Task 5.

Preserve useful conversation behavior, but move it toward a service-layer controller:

- conversation lookup;
- find-or-create conversation;
- send message;
- rental request message creation;
- rental request finalization;
- cash agreement message if kept;
- read receipts if kept.

Do not leave a repository-injecting conversation controller as the final design if Task 2 chose service-layer APIs.

Preserve useful payment/dashboard behavior, but move it toward a final API contract:

- payment history from real data;
- filtered/sorted/paged history if still needed by Desktop;
- card/cash payment behavior if kept;
- receipt paths if kept;
- confirmed rental rows even when payment does not exist yet.

Do not keep payment history tied to old static `SessionContext` or hardcoded integer users as the final design.

Be careful with identity. The current conversation and payment code still uses integer users in several places, while notifications use account ids. The final API should use Task 3 identity decisions and should only use legacy integer ids as a compatibility bridge when required.

Be careful with side effects. The task owner should not delete existing endpoint behavior just because the files are messy. Useful behavior must be moved, preserved, assigned to another task, or explicitly marked legacy.

The application may not build perfectly at the start of this task. Do not try to fix the whole solution just to make this task look complete. Fix task-related errors only and document unrelated blockers clearly.

## Expected Output

This task should produce:

- canonical conversation API;
- canonical notification API;
- canonical payment/dashboard API contract;
- side-effect rules for request events;
- clear ownership of request-created, request-accepted, and request-declined behavior;
- clear mapping between chat messages, notifications, requests, rentals, and payments;
- clear decision about old chat/payment/notification endpoints that are legacy, compatibility-only, or final active;
- clear decision about account id versus legacy integer user id in conversation/payment routes;
- notes for Task 7 about required service, mapper, repository, and notifier registrations;
- notes for Task 10 about which routes Desktop should call.

Side-effect rules:

```text
request created
-> owner chat rental request message
-> owner notification

request accepted
-> chat rental request message finalized as accepted
-> renter notification
-> confirmed rental/payment/dashboard data available

request declined
-> chat rental request message finalized as declined
-> renter notification
```

## What Counts As Done

Owner can see rental request in chat.

Owner can see rental request notification.

Renter can see accepted/declined notification.

Dashboard/payment history reads real API data.

Task 10 can build Desktop Chat, Notifications, and Dashboard against this contract.

There is no second active notification flow based on old project 1 request/rental pages.

Useful old endpoint logic is either preserved in the canonical API path, assigned to another task, or explicitly quarantined as legacy.

Conversation, notification, and payment/dashboard routes have clear final ownership.

Request/rental identifiers from Task 5 are used consistently by chat messages, notifications, and payment/dashboard rows.

Unrelated build errors or unrelated merge problems are documented instead of silently taken over.

## Do Not Touch

Do not implement Desktop routing.

Do not implement Web pages.

Do not take over Task 3 auth/account/admin behavior.

Do not take over Task 4 games/filter/search behavior beyond the game data needed for messages, notifications, and payment/dashboard rows.

Do not take over Task 5 request/rental state transition rules. Task 6 consumes those events and attaches side effects.

Do not take over Task 7 runtime startup wiring, except to document required registrations.

Do not take over DB seed/setup work.

Do not revive deprecated duplicate controllers, services, repositories, or old notification/request/rental flows as final behavior.

Do not delete useful old endpoint behavior only to make the API look cleaner. Preserve it in the canonical path, assign it to another task, or document why it is legacy.

Do not keep project 1 notification behavior and project 2 chat rental request behavior as separate active systems.

Do not move business logic into Desktop views, Web views, or Shared API clients.

Do not use hardcoded users or hardcoded ids to make the workflow appear to work.

Do not write tests unless the lead explicitly changes this task.

Do not try to fix unrelated build errors across the entire solution.

## Known Blockers And Assumptions

This task assumes Task 1 has prepared the dependency direction and identity/session contract.

This task assumes Task 2 has decided the canonical controller/service ownership and legacy quarantine direction.

This task assumes Task 3 defines how chat/payment code should move from old integer users to the final account identity, or how legacy/PAM ids are bridged temporarily.

This task assumes Task 4 provides the final game details needed for message, notification, and payment/dashboard content.

This task assumes Task 5 provides the final request/rental lifecycle and event points that chat, notifications, and payments attach to.

This task assumes routing work is handled separately and should be consumed later by Desktop tasks.

This task assumes DB seed/setup is handled separately before final integration.

If any of those assumptions are not true, document the blocker and coordinate with the responsible task owner instead of expanding this task into setup work.

The current application may not build at the start of this task. The owner should fix chat/notification/payment-specific errors only and should not take ownership of unrelated Desktop, Web, Shared, Data, test, routing, or seed problems.
