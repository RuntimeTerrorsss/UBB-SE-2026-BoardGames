# Task 1: Architecture, Boundary, And Identity Contract

**Purpose:** Accepted contract for the `.Desktop + .Api` department before API cleanup and Desktop integration work continues.

**Status:** Proposed for lead acceptance.

## Final Dependency Direction

The final dependency direction is:

```text
BoardGames.Desktop -> BoardGames.Shared -> HTTP API
BoardGames.Web     -> BoardGames.Shared -> HTTP API
BoardGames.Api     -> BoardGames.Shared + BoardGames.Data
BoardGames.Data    -> database only
```

Inside the API, the final direction is:

```text
Controller -> Service -> Repository -> AppDbContext -> Database
```

## Forbidden Final Dependencies

The final application must not contain:

- `BoardGames.Desktop -> BoardGames.Api`;
- `BoardGames.Desktop -> BoardGames.Data`;
- `BoardGames.Desktop -> AppDbContext`;
- `BoardGames.Desktop -> repository interfaces or implementations`;
- `BoardGames.Shared -> BoardGames.Data`;
- `BoardGames.Data -> BoardGames.Shared`;
- API controllers injecting repositories directly.

Temporary violations must be listed in `docs/audits/task-1-boundary-violation-inventory.md` and removed by the task that owns that area.

## Project Ownership

| Project | Owns | Must Not Own |
| --- | --- | --- |
| `BoardGames.Desktop` | WinUI pages, viewmodels, navigation, local session state, local API-client configuration | API controllers, API services, repositories, `AppDbContext`, migrations, seed data |
| `BoardGames.Shared` | DTOs, API-client interfaces, API-client implementations, API response wrappers, transport enums | EF models, repository interfaces, repository implementations, `AppDbContext` |
| `BoardGames.Api` | Controllers, business services, mappers, auth logic, runtime/DI configuration | WinUI pages, Desktop navigation, direct UI state |
| `BoardGames.Data` | EF models, repositories, migrations, `AppDbContext`, persistence enums | Shared DTOs, Desktop session, HTTP clients |
| `ServerCommunication` | UDP message transport for optional local notification server | API business logic, database access |
| `NotificationServer` | Optional local notification process | Domain persistence, API route ownership |

## Shared/Data Ownership Rule

`.Shared` owns transport shapes. `.Data` owns persistence shapes. The API maps between them.

DTOs that Desktop/Web receive from API belong in `.Shared`.

EF entities and repository contracts belong in `.Data`.

Transport enums used by Desktop/Web/API contracts belong in `.Shared`. EF-only enums may stay in `.Data`. If both are needed, API maps between them.

## Identity Contract

The public identity sent to Desktop/Web is:

```text
AccountId: Guid
```

The legacy/internal identity used by old chat/rental/payment tables is:

```text
PamUserId: int
```

Desktop should primarily use `AccountId`. If an active Desktop feature still needs `PamUserId`, the login/profile response must provide it explicitly. Desktop must not guess it and must not use static Alice/Bob/Carol IDs in the final flow.

## Login/Profile Session Fields

The login/profile response needed by Desktop is:

```text
AccountId: Guid
PamUserId: int?
Username: string
DisplayName: string
Email: string
Role: string
AvatarUrl: string?
IsSuspended: bool
IsLocked: bool
Country: string?
City: string?
StreetName: string?
StreetNumber: string?
```

## Desktop Session Contract

Desktop should have one session source used by:

- Filter;
- Game Details;
- Games/My Games/Admin Games;
- Chat;
- Notifications;
- Dashboard/payment history;
- Account;
- Admin.

The final Desktop session stores:

```text
IsLoggedIn
AccountId
PamUserId, only while legacy chat/rental/payment code still needs it
Username
DisplayName
Email
Role
AvatarUrl
Account status
Profile fields required by Account page
```

The final Desktop session must replace:

- `BoardGames.Data.Enums.SessionContext`;
- hardcoded Alice/Bob/Carol switching;
- separate old project 1 and old project 2 session concepts.

## Handoff To Later Tasks

Task 2 uses this document to clean duplicate API controllers/services.

Tasks 3-6 use this document to expose service-layer APIs with stable DTO and identity rules.

Task 7 uses this document when wiring the local API runtime.

Task 8 uses this document to create the final Desktop startup, API-client configuration, and session implementation.

Tasks 9-10 use this document to make feature screens consume the same session and API-client contracts.

## What Counts As Accepted

The lead accepts this document as the rule for the department.

No later task should need to guess whether to use DTOs, EF models, `AccountId`, `PamUserId`, repositories, API services, or API clients.
