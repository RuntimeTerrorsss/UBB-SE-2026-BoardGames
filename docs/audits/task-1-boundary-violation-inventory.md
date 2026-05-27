# Task 1: Boundary Violation Inventory

**Purpose:** Evidence list for the current codebase violations of the Task 1 architecture contract.

**Status:** Inventory only. This document does not assign cleanup ownership to Task 1.

## Project References

| File | Current Reference | Final Status | Owner To Remove Or Replace |
| --- | --- | --- | --- |
| `BoardGamesApp/BoardGames.Desktop/BoardGames.Desktop.csproj` | `BoardGames.Data` | forbidden final dependency | Task 8 / Desktop API-client setup |
| `BoardGamesApp/BoardGames.Desktop/BoardGames.Desktop.csproj` | `BoardGames.Api` | forbidden final dependency | Task 8 / Desktop API-client setup |
| `BoardGamesApp/BoardGames.Desktop/BoardGames.Desktop.csproj` | `BoardGames.Shared` | final allowed dependency | keep |
| `BoardGamesApp/BoardGames.Desktop/BoardGames.Desktop.csproj` | `ServerCommunication` | temporary notification dependency | Task 6 / Task 8 decide final local notification path |
| `BoardGamesApp/BoardGames.Shared/BoardGames.Shared.csproj` | `BoardGames.Data` | forbidden final dependency | Shared/API-client cleanup coordinated through Tasks 2, 7, and 8 |
| `BoardGamesApp/BoardGames.Api/BoardGames.Api.csproj` | `BoardGames.Data` | final allowed dependency | keep |
| `BoardGamesApp/BoardGames.Api/BoardGames.Api.csproj` | `BoardGames.Shared` | final allowed dependency | keep |
| `BoardGamesApp/BoardGames.Web/BoardGames.Web.csproj` | `BoardGames.Shared` | final allowed dependency | keep |
| `BoardGamesApp/NotificationServer/NotificationServer.csproj` | `ServerCommunication` | allowed for optional local notification process | keep if Task 6 keeps local notification server |
| `BoardGamesApp/BoardGames.Tests/BoardGames.Tests.csproj` | `BoardGames.Shared` and `BoardGames.Data` | outside final application runtime | ignore for this task unless tests are repaired later |

## Shared/Data Violations

These files make `BoardGames.Shared` depend on persistence concepts. They explain why adding `BoardGames.Data -> BoardGames.Shared` would create a cycle.

| File | Current Violation | Final Direction |
| --- | --- | --- |
| `BoardGamesApp/BoardGames.Shared/DTO/MessageDTO.cs` | imports `BoardGames.Data.Enums` for `MessageType` | move/duplicate transport enum into Shared and map in API |
| `BoardGamesApp/BoardGames.Shared/ProxyRepositories/UserAPIProxy.cs` | imports `BoardGames.Data.Models` and `BoardGames.Data.Repositories` | replace repository-shaped proxy with Shared API client contract |
| `BoardGamesApp/BoardGames.Shared/ProxyRepositories/ConversationAPIProxy.cs` | imports `BoardGames.Data`, `BoardGames.Data.Enums`, `BoardGames.Data.Models`, and `BoardGames.Data.Repositories` | replace with conversation API client DTOs |
| `BoardGamesApp/BoardGames.Shared/ProxyRepositories/GamesAPIProxy.cs` | imports Data namespaces and implements `InterfaceGamesRepository` | replace with games/search API client; do not expose repository interface from Shared |
| `BoardGamesApp/BoardGames.Shared/ProxyRepositories/RepositoryPaymentAPIProxy.cs` | imports Data namespaces and implements `IRepositoryPayment` | replace with payments/dashboard API client |
| `BoardGamesApp/BoardGames.Shared/ProxyRepositories/PaymentAPIProxy.cs` | imports Data repository namespace | replace with payment API client contract |
| `BoardGamesApp/BoardGames.Shared/ProxyRepositories/RentalAPIProxy.cs` | imports `BookingBoardGames.Data` and repository interfaces | replace with rental/request API client |
| `BoardGamesApp/BoardGames.Shared/ProxyRepositories/Sql/*` | stores repository-style SQL text in Shared | move persistence/query ownership behind API/Data or quarantine with old proxy repositories |

## Desktop API/Data Violations

These files make Desktop depend on Data, API internals, repository interfaces, or database setup. Task 1 only records them. Later Desktop/API-client tasks should remove them from the final runtime path.

| File | Current Violation | Final Direction |
| --- | --- | --- |
| `BoardGamesApp/BoardGames.Desktop/App.xaml.cs` | imports `BoardGames.Data`, `BoardGames.Data.Repositories`, and `BoardGames.Api.Services`; creates `AppDbContext`, repository proxies, and API service objects directly | final Desktop app should configure Shared API clients and one session only |
| `BoardGamesApp/BoardGames.Desktop/DatabaseBootstrap.cs` | initializes database from Desktop | API/DB setup owns database creation, migration, and seed |
| `BoardGamesApp/BoardGames.Desktop/DatabaseConfig.cs` | Desktop owns database connection string resolution | final Desktop should own API URL config, not DB config |
| `BoardGamesApp/BoardGames.Desktop/Navigation/BookingNavigationArguments.cs` | carries `BoardGames.Api.Services.ConversationService` through Desktop navigation | pass DTO/state identifiers, not API service instances |
| `BoardGamesApp/BoardGames.Desktop/ViewModels/CardPaymentViewModel.cs` | imports Data constants/interfaces and API services | payments should use Shared API client and DTOs |
| `BoardGamesApp/BoardGames.Desktop/ViewModels/CashPaymentViewModel.cs` | imports Data repositories and API services | payments should use Shared API client and DTOs |
| `BoardGamesApp/BoardGames.Desktop/ViewModels/ConfirmBookingViewModel.cs` | imports Data enums and API services | booking/request flow should call final request/rental API client |
| `BoardGamesApp/BoardGames.Desktop/ViewModels/ChatPageViewModel.cs` | imports Data and Data repositories | chat should use Shared conversation API client |
| `BoardGamesApp/BoardGames.Desktop/ViewModels/DiscoveryViewModel.cs` | imports old Data enums and old Sharing services | filter/discovery should use final games/search API client |
| `BoardGamesApp/BoardGames.Desktop/ViewModels/FilteredSearchViewModel.cs` | imports old Data enums and old Sharing services | filter/search should use final games/search API client |
| `BoardGamesApp/BoardGames.Desktop/ViewModels/GameDetailsViewModel.cs` | imports old Data enums, old DTOs, old mappers, and old services | game details/rental entry should use final games and requests API clients |
| `BoardGamesApp/BoardGames.Desktop/ViewModels/PaymentHistoryViewModel.cs` | imports old Data constants/enums and old Sharing services | dashboard/payment history should use final payments/dashboard API client |
| `BoardGamesApp/BoardGames.Desktop/Views/ConfirmBookingView.xaml.cs` | uses `BookingBoardGames.Data.Enum.SessionContext` directly | use final Desktop session context |
| `BoardGamesApp/BoardGames.Desktop/Views/DashboardView.xaml.cs` | uses old `SessionContext` integer user flow | use final Desktop session context |
| `BoardGamesApp/BoardGames.Desktop/Views/DiscoveryView.xaml.cs` | uses static hardcoded logged-in user switching | use final login/session identity |
| `BoardGamesApp/BoardGames.Desktop/Views/ChatViews/*` | several files use old integer `SessionContext`/user id flow | Task 10 should consume the final session and conversation API contract |

## API Controller Boundary Violations

These controllers inject repositories directly. The final API must expose service-layer operations instead.

| File | Current Violation | Final Direction |
| --- | --- | --- |
| `BoardGamesApp/BoardGames.Api/Controllers/UsersController.cs` | injects `IUserRepository` | route should call canonical user/account service |
| `BoardGamesApp/BoardGames.Api/Controllers/GamesController.cs` | injects `InterfaceGamesRepository` | route should call canonical game/search service |
| `BoardGamesApp/BoardGames.Api/Controllers/RentalsController.cs` | injects `IRentalRepository`, `IConversationRepository`, and `InterfaceGamesRepository` | route should call request/rental lifecycle service |
| `BoardGamesApp/BoardGames.Api/Controllers/ConversationController.cs` | injects `IConversationRepository` | route should call conversation/chat service |
| `BoardGamesApp/BoardGames.Api/Controllers/PaymentsController.cs` | injects `IPaymentRepository` and `IRepositoryPayment` | route should call payments/dashboard service |

## Namespace Drift

The current merge still contains old namespace families:

- `BookingBoardGames.*`;
- `BoardRentAndProperty.*`;
- `BoardGames.*`.

The final application does not need to rename every namespace immediately, but workers must not use old namespace names to decide which code is canonical. Canonical ownership should come from the Task 2 route/service decisions and the Task 1 dependency contract.

## Identity/Session Split

Current identity is split between:

- `Guid` account identity in account/profile/admin/game-management flow;
- `int` user identity in old filter/chat/rental/payment flow;
- static hardcoded user switching in old Desktop screens.

Current evidence:

- `BoardGamesApp/BoardGames.Shared/DTO/AccountProfileDTO.cs` exposes account/profile data with a `Guid Id`.
- `BoardGamesApp/BoardGames.Data/Models/User.cs` contains both `Guid Id` and `int PamUserId`.
- `BoardGamesApp/BoardGames.Data/Enums/SessionContext.cs` stores an integer `UserId`.
- `BoardGamesApp/BoardGames.Desktop/Views/DiscoveryView.xaml.cs` switches between hardcoded users.
- `BoardGamesApp/BoardGames.Desktop/Views/ChatViews/ChatPageView.xaml.cs` initializes chat by integer user id.
- `BoardGamesApp/BoardGames.Shared/DTO/BookingDTO.cs` still exposes `int UserId`.
- newer request/rental DTOs such as `CreateRequestDTO.cs` and `CreateRentalDTO.cs` use `Guid RenterAccountId` and `Guid OwnerAccountId`.

Final rule:

- public identity is `Guid AccountId`;
- legacy/internal identity is `int PamUserId`;
- API translates between account id and legacy user id where needed;
- Desktop stores both only if the active legacy flow still needs the integer id;
- Desktop must not guess or hardcode the integer id.

## Cleanup Ownership

Task 1 documents the violations. It does not remove every violation.

Task 2 owns duplicate API cleanup and the final API route/service ownership table.

Task 3 owns auth/account/admin API behavior and the final login/profile fields.

Tasks 4-6 own feature API contracts that must follow this boundary.

Task 7 owns API runtime wiring.

Task 8 owns Desktop startup, API-client configuration, and final session implementation.

Tasks 9-10 own Desktop feature integration against the final API/session contracts.

The Shared/API-client cleanup owner must remove Shared proxy repositories that implement Data repository interfaces before the final build/demo path is accepted.
