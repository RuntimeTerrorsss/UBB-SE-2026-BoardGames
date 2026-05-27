# Task 6: Chat / Notifications / Payments API — Decisions And Output

## What Was Done

Task 6 wired the request/rental lifecycle into visible chat messages, and exposed filtered payment history per user. All new API code follows the Controller → Service → Repository boundary rule from Task 1.

## New Files

- `BoardGames.Api/Services/IConversationApiService.cs` — stateless conversation service interface; accepts `Guid accountId`, translates to `PamUserId` internally
- `BoardGames.Api/Services/ConversationApiService.cs` — implementation; uses `IAccountRepository` for Guid → PamUserId, delegates to `IConversationRepository`
- `BoardGames.Api/Services/IDashboardService.cs` — payment history service interface
- `BoardGames.Api/Services/DashboardService.cs` — implementation; filters `HistoryPayment` by the user's `PamUserId`

## Modified Files

### `BoardGames.Data/Repositories/IConversationRepository.cs`
Added: `Task<RentalRequestMessage?> FindRentalRequestMessageByRequestId(int requestId);`

### `BoardGames.Data/Repositories/ConversationRepository.cs`
Implemented `FindRentalRequestMessageByRequestId` using a `StartsWith("[req:{requestId}]")` prefix search on `RequestContent`.

### `BoardGames.Api/Controllers/ConversationController.cs`
- Removed broken `BookingBoardGames.Data` / `BookingBoardGames.Data.Interfaces` usings
- Replaced direct `IConversationRepository` injection with `IConversationApiService`
- Route `GET user/{userId}` → `GET user/{accountId:guid}`
- Route `POST /rental/finalize/{messageId}` → `POST /rental/finalize/{requestId}` (now works by request id, not message id)
- Local `MessageDto` and `MessageType` records removed — uses `MessageDataTransferObject` and `MessageType` from `BoardGames.Shared.DTO`
- `POST /` now takes `{ SenderAccountId: Guid, ReceiverAccountId: Guid }` instead of int ids

### `BoardGames.Api/Controllers/PaymentsController.cs`
- Added `IDashboardService` injection
- Added `GET api/payments/user/{accountId:guid}/history` — returns `List<PaymentDataTransferObject>` filtered to the logged-in user
- All existing routes retained unchanged

### `BoardGames.Api/Services/IRequestService.cs`
Four methods made async (return `Task<Result<...>>`):
- `CreateRequest`, `ApproveRequest`, `DenyRequest`, `OfferGame`

### `BoardGames.Api/Services/RequestService.cs`
- Injected `IConversationApiService` (new constructor parameter — Task 7 must register it)
- `CreateRequest`: after `requestDataRepository.Add`, calls `AttachRentalRequestMessage`
- `TryApproveOpenRequestAndNotify`: after `ApproveAtomically`, calls `FinalizeRentalRequestMessage(requestId, accepted: true)`
- `DenyRequest`: after `requestDataRepository.Delete`, calls `FinalizeRentalRequestMessage(requestId, accepted: false)`
- Conversation failures are caught and logged as warnings — they do not roll back the request operation

### `BoardGames.Api/Controllers/RequestsController.cs`
- `Create`, `Approve`, `Deny`, `Offer` made async (`async Task<...>`, `await`)

## RentalRequestMessage Prefix Convention

When a request is created, `AttachRentalRequestMessage` stores:
```
RequestContent = "[req:{requestId}] Rental request for {gameName} from {start:d} to {end:d}."
RentalRequestId = 0   (no rental exists yet; FK would violate constraint)
```

`FindRentalRequestMessageByRequestId(requestId)` searches for messages whose `RequestContent` starts with `"[req:{requestId}]"`. This is how approve/deny finalize the correct chat message without a direct FK to the Request table.

When approved, `RentalRequestId` remains 0 — only `IsRequestResolved = true` and `IsRequestAccepted = true` are set.
If Task 6 or Task 10 later needs to store the real rental id in `RentalRequestId`, call `HandleMessageUpdate` after the rental is created.

## Notes For Task 7 — Required DI Registrations

New registrations to add to `Program.cs`:
- `IConversationApiService` → `ConversationApiService` (scoped)
- `IDashboardService` → `DashboardService` (scoped)
- `IConversationRepository` → `ConversationRepository` (scoped, if not already registered)
- `IRepositoryPayment` → `RepositoryPayment` (scoped, if not already registered)

`RequestService` now requires `IConversationApiService` as a constructor parameter — its DI registration must include it.

## Notes For Task 10

`BoardGames.Shared/ProxyRepositories/ConversationAPIProxy.cs` currently calls:
```
GET conversation/user/{userId}   (int userId)
```
After Task 6, the canonical route is:
```
GET api/conversation/user/{accountId:guid}   (Guid accountId)
```
Task 10 must update `ConversationAPIProxy` to pass `Guid accountId` and hit the new route.

`POST api/conversation` now expects `{ SenderAccountId: Guid, ReceiverAccountId: Guid }`.

## Route Summary

| Route | Purpose |
| --- | --- |
| `GET api/conversation/user/{accountId:guid}` | Get all conversations for a user |
| `GET api/conversation/{id}` | Get single conversation by id |
| `POST api/conversation` | Find or create conversation between two users (Guid ids) |
| `POST api/conversation/messages` | Send a message |
| `PUT api/conversation/messages` | Update a message |
| `POST api/conversation/readreceipt` | Mark messages read |
| `POST api/conversation/rental/finalize/{requestId}` | Finalize rental request message by request id |
| `POST api/conversation/cash/{parentMessageId}/{paymentId}` | Create cash agreement message |
| `GET api/payments/user/{accountId:guid}/history` | Payment history for a user (filtered, DTO) |
