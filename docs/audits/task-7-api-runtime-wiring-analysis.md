# Task 7 — API Runtime Wiring & Local Backend Smoke: Codebase Analysis

**Status:** Analysis-only. Captures the state of the repository on the `setup` branch (after Tasks 1-6 merged) before Task 7 implementation work on this branch (`task7`).

**Branch policy (from the upstream brief):** Task 7 is being implemented on `task7`, branched off `setup`. Final merge target is `main` (separate step after Task 7 acceptance).

**Scope of this document:** What is wired today, what is missing, what must be registered, where the broken seams are, and what the backend smoke test will fail on if Task 7 stops at "just wire DI".

---

## 1. One-Paragraph Summary

`BoardGames.Api/Program.cs` only registers `AddControllers()` and Swagger. There is no `AppDbContext`, no repository, no service, no mapper, and no auth handler. As a result every controller in the API has unsatisfiable constructor dependencies and the host cannot resolve them on the first request. Beyond the DI gap, the API does not compile in its current shape because (a) `BoardGames.Shared` has 23 compile errors that pull EF model types (`Address`, `ConversationParticipant`) into Shared DTOs and reference a removed `GameDTO`, and (b) roughly half of the files under `BoardGames.Api/Services` still `using BookingBoardGames.*` against namespaces that no longer exist anywhere in the solution. Task 2 explicitly flagged these as Task 6's quarantine work, and Task 6's `.md` confirms it never did the quarantine. So Task 7's first job is unblocking the build, then DI, then auth, then smoke.

---

## 2. Inputs From Tasks 1–6 (the contracts Task 7 is consuming)

| Task | What it produced that Task 7 must honor |
| --- | --- |
| Task 1 | Final dependency direction is `Desktop → Shared → HTTP → API → Services → Repositories → AppDbContext`. Public identity is `Guid AccountId`; legacy persistence identity is `int PamUserId`. API translates between them. |
| Task 2 | Canonical controllers live under `Controllers/`. V1 duplicates are quarantined under `Legacy/` and excluded via `<Compile Remove="Legacy/**/*.cs" />`. |
| Task 3 | `IAuthService`/`IAccountService`/`IAdminService`/`IAvatarStorageService` are implemented and consumed by the three canonical controllers. `AdminController` carries `[Authorize(Roles = "Admin")]` — Task 7 must make that attribute actually do something. |
| Task 4 | `GamesController` (canonical) injects `IGameService`. `GameService` depends on `InterfaceGamesRepository`, `IRentalRepository`, `GameMapper`, `IRequestService`. |
| Task 5 | `IRequestService`/`IRentalService` made async; `RequestService` now needs `IConversationApiService` as a constructor parameter. State machine errors are typed. |
| Task 6 | New services `IConversationApiService` and `IDashboardService`. Task 6 explicitly lists registrations Task 7 must add. |

---

## 3. Current `Program.cs` Inventory

`BoardGames.Api/Program.cs` registers exactly:

```csharp
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

app.UseHttpsRedirection();
app.UseAuthorization();    // no UseAuthentication() above it, no scheme registered
app.MapControllers();
```

Everything else is missing.

---

## 4. Build Errors (verified by `dotnet build BoardGames.Api`)

The build fails with **23 errors, all in `BoardGames.Shared`**:

| File | Symbol | Root cause |
| --- | --- | --- |
| `Shared/Validators/AddressValidator.cs` | `Address` not found | References the EF model `Address` (lives in `BoardGames.Data.Models`). Shared cannot reference Data per Task 1. |
| `Shared/DTO/ConversationDTO.cs` | `ConversationParticipant` not found | Same problem — DTO holds a collection of EF entities. |
| `Shared/DTO/RequestDTO.cs`, `Shared/DTO/RentalDTO2.cs`, `Shared/ProxyServices/IGameService.cs`, `Shared/ProxyServices/GameService.cs` | `GameDTO` not found | `GameDTO` was deleted; canonical shapes are `GameSummaryDTO`/`GameDetailDTO`. |

Once Shared compiles, the API project's own broken files become the next layer: 18 service files and 2 mappers under `BoardGames.Api/Services` and `BoardGames.Api/Mappers` still `using BookingBoardGames.*`. None of these are referenced by any canonical controller. They will be moved to `BoardGames.Api/Legacy/Services/` and `Legacy/Mappers/` and picked up by the existing `<Compile Remove="Legacy/**/*.cs" />` glob.

---

## 5. Dependency Injection Plan (final list)

### 5.1 EF / database

```csharp
builder.Services.AddDbContext<AppDbContext>(opts =>
    opts.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDbContextFactory<AppDbContext>(opts =>
    opts.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
```

Both needed: repositories split between `AppDbContext` and `IDbContextFactory<AppDbContext>`.

### 5.2 Repositories

```csharp
builder.Services.AddScoped<IAccountRepository,        AccountRepository>();
builder.Services.AddScoped<IFailedLoginRepository,    FailedLoginRepository>();
builder.Services.AddScoped<IConversationRepository,   ConversationRepository>();
builder.Services.AddScoped<INotificationRepository,   NotificationRepository>();
builder.Services.AddScoped<IRequestRepository,        RequestRepository>();
builder.Services.AddScoped<IRentalRepository,         RentalRepository>();
builder.Services.AddScoped<IPaymentRepository,        PaymentRepository>();
builder.Services.AddScoped<IRepositoryPayment,        RepositoryPayment>();
builder.Services.AddScoped<IUserRepository,           UserRepository>();

// GamesRepository implements BOTH IGameRepository and InterfaceGamesRepository.
builder.Services.AddScoped<GamesRepository>();
builder.Services.AddScoped<IGameRepository>(sp => sp.GetRequiredService<GamesRepository>());
builder.Services.AddScoped<InterfaceGamesRepository>(sp => sp.GetRequiredService<GamesRepository>());
```

### 5.3 Mappers

```csharp
builder.Services.AddScoped<AccountProfileMapper>();
builder.Services.AddScoped<GameMapper>();
builder.Services.AddScoped<GameImageMapper>();
builder.Services.AddScoped<NotificationMapper>();
builder.Services.AddScoped<RentalMapper>();
builder.Services.AddScoped<RequestMapper>();
builder.Services.AddScoped<UserMapper>();
```

### 5.4 Avatar storage + business services

```csharp
builder.Services.AddScoped<IAvatarStorageService,   AvatarStorageService>();

builder.Services.AddScoped<IAuthService,            AuthService>();
builder.Services.AddScoped<IAccountService,         AccountService>();
builder.Services.AddScoped<IAdminService,           AdminService>();
builder.Services.AddScoped<IUserService,            UserService>();
builder.Services.AddScoped<IGameService,            GameService>();
builder.Services.AddScoped<IRentalService,          RentalService>();
builder.Services.AddScoped<IRequestService,         RequestService>();
builder.Services.AddScoped<INotificationService,    NotificationService>();
builder.Services.AddScoped<IConversationApiService, ConversationApiService>();
builder.Services.AddScoped<IDashboardService,       DashboardService>();
```

---

## 6. Authentication & Authorization

Cookie auth (mirrors `BoardGames.Web/Program2.cs`). The login flow:

1. `AuthController.Login` calls `IAuthService.LoginAsync`.
2. On success, controller builds `ClaimsPrincipal` with `NameIdentifier = Id`, `Name = Username`, `Role = primary role`, `PamUserId` claim.
3. Controller calls `HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal)`.
4. Returns the `AccountProfileDTO` body so Desktop can populate its session.

`app.UseAuthentication()` must come before `app.UseAuthorization()` in the pipeline.

---

## 7. Configuration

`appsettings.json` gains:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=MergedBoardGamesDb;Trusted_Connection=True;TrustServerCertificate=True;"
},
"AvatarStoragePath": "wwwroot/avatars"
```

`launchSettings.json` already exposes `http://localhost:5018`.

---

## 8. Backend Smoke Checklist (the deliverable that gates Task 8/9/10)

| # | Step | Route |
| --- | --- | --- |
| 1 | login | `POST /api/auth/login` |
| 2 | list/filter games | `GET /api/games`, `POST /api/games/search` |
| 3 | game details | `GET /api/games/{gameId:int}` |
| 4 | create rental request | `POST /api/requests` |
| 5 | verify conversation | `GET /api/Conversation/user/{ownerAccountId:guid}` |
| 6 | verify owner notification | `GET /api/notifications/user/{ownerAccountId:guid}` |
| 7 | approve/decline request | `PUT /api/requests/{requestId:int}/approve` (or `/deny`) |
| 8 | verify renter notification | `GET /api/notifications/user/{renterAccountId:guid}` |
| 9 | verify rental state | `GET /api/rentals/renter/{renterAccountId:guid}` |
| 10 | verify payment history shape | `GET /api/payments/user/{accountId:guid}/history` |

**Pre-flight:** `dotnet build` is clean; API starts on `http://localhost:5018`; Swagger lists all canonical groups; `GET /api/admin/accounts` returns 403 for non-admin and 200 for admin (proves the `[Authorize(Roles="Admin")]` middleware fires).

---

## 9. Risks & Open Items

1. The Shared compile errors are technically Task 1's scope. Task 7 absorbs them because they block the build. Smallest blast radius: replace EF model refs in 5 Shared files with DTO equivalents (or with int identifiers where the field is consumed only by id).
2. `PaymentsController` still injects repositories directly — a known Task 6 follow-up. Works for smoke. Not Task 7's fix.
3. `AvatarStorageService` reads `IWebHostEnvironment.WebRootPath`; add `wwwroot/avatars` to the API project.
4. Migration must be run once before the first request (`dotnet ef database update --project BoardGames.Data --startup-project BoardGames.Api`). Cooperate with the DB-seed person for demo passwords.

---

## 10. Execution Order On Task7 Branch

1. Unblock Shared compile (5 files).
2. Quarantine BookingBoardGames-importing files (move 18+ files under `Legacy/`).
3. Add `ConnectionStrings:DefaultConnection` to `appsettings.json` and `appsettings.Development.json`.
4. Rewrite `Program.cs` per §5.
5. Add cookie auth scheme; update `AuthController.Login` to sign in.
6. `dotnet build` → green.
7. Commit on `task7` branch.
8. (Out-of-process) `dotnet ef database update` and walk the §8 smoke checklist.
9. Open merge into `main`.
