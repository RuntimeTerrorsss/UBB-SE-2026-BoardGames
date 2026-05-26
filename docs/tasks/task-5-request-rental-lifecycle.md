# Task 5: Request And Rental Lifecycle API — Decisions And Output

## Canonical Decision
Project 2 request/rental lifecycle is canonical. Project 1 standalone screens
(My Requests, Others Requests, My Rentals, Others Rentals) do not define a
second backend flow.

## Route Ownership
- `api/requests` — owned by RequestsController / IRequestService / RequestService
- `api/rentals` — owned by RentalsController / IRentalService / RentalService

## State Transition Rules
- Request created → status: Open, owner receives notification
- Owner approves → confirmed Rental created, request deleted, conflicting requests deleted
- Owner denies → request deleted, renter notified
- Renter cancels → request deleted
- Game deactivated → all Open/OfferPending requests cancelled, renters notified

## Availability Rules
- Start date must be before end date
- Start date must not be in the past
- Dates must be within 1 month from now
- Game must be active
- Owner cannot rent own game
- No overlap with confirmed rentals (48h buffer)
- No overlap with existing open requests (48h buffer)

## Error Codes For Desktop
- `invalid_date_range`
- `game_not_found`
- `owner_cannot_rent`
- `dates_unavailable`
- `request_not_found`
- `request_forbidden`
- `request_not_open`
- `request_transaction_failed`
- `rental_validation_failed`
- `rental_conflict`

## DTO Decision
Canonical rental DTO is `RentalDTO` in `RentalDTO2.cs` (account-based).
`RentalDataTransferObject` in `RentalDTO.cs` is legacy — not used by Task 5.

## Direct Rental Creation
`POST api/rentals` is internal/admin only.
Normal user flow: POST api/requests → owner approves → rental created automatically.

## Denied/Cancelled Request History
Current implementation deletes denied/cancelled requests.
Task 6 must decide if history is needed for chat/payment/dashboard.

## Notes For Task 6
- Request created: attach chat message and owner notification using request.Id
- Request approved: attach renter notification using request.Id and rental.Id
- Request denied: attach renter notification using request.Id
- Confirmed rental id returned by PUT api/requests/{id}/approve as { RentalId: int }

## Notes For Task 7 — Required DI Registrations
- `IRequestRepository` → `RequestRepository`
- `IRentalRepository` → `RentalRepository`
- `IGameRepository` → `GamesRepository`
- `RequestMapper`, `RentalMapper`, `GameMapper`, `UserMapper` — singleton
- `IRequestService` → `RequestService`
- `IRentalService` → `RentalService`
- `INotificationService` → `NotificationService`

## Notes For Task 9 And Task 10
- Game Details date picker → GET api/requests/games/{gameId}/availability
- Booked dates calendar → GET api/requests/games/{gameId}/booked-dates
- Create request → POST api/requests
- Approve/deny/cancel → PUT api/requests/{id}/approve|deny|cancel
- Renter history → GET api/requests/renter/{id} and GET api/rentals/renter/{id}
- Owner history → GET api/requests/owner/{id} and GET api/rentals/owner/{id}

## Fixes Applied In This Task
- Restored IGameRepository interface in IGameRepository2.cs
- GamesRepository now implements IGameRepository
- Added Rental.Renter alias property for Client
- Fixed RentalMapper.ToModel to use Client instead of Renter
- Fixed RentalService.CreateConfirmedRental constructor call
- Fixed RequestService and RentalService to call GetGame() not Get()
- Added owner notification on request creation
- Marked POST api/rentals as internal/admin only

## Unrelated Blockers (Not Task 5)
- NU1202 H.NotifyIcon.WinUI — Desktop package incompatibility, Task 9/10 owner
- GameDTO not found in BoardGames.Shared — Task 4 owner
- Program.cs DI wiring — Task 7 owner