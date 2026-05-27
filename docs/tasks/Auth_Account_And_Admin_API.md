# Task 3: Auth, Account, And Admin API

**Workflow source:** Section 2, API Cleanup and Section 3, API Runtime Wiring  
**Type:** Parallel API feature lane  
**Can start after:** Task 1 and Task 2 route/service decisions  
**Can run in parallel with:** Tasks 4, 5, and 6  
**Suggested owner:** backend worker comfortable with auth/account rules  
**Primary project area:** `BoardGames.Api`  
**Secondary coordination area:** `BoardGames.Shared` DTOs and API clients only where auth/account/admin contracts require it  

## What This Task Is About

This task makes the project 1 account system the canonical identity system for the whole merged application.

The merged application must not have two unrelated login systems. The final user account used by Account, Admin, Games, Filter, Rental Requests, Chat, Notifications, and Dashboard must be the same account. This task owns the API side of that identity decision.

The main goal is to make `api/auth`, `api/accounts`, and `api/admin` the stable backend contract for authentication, profile management, and account administration. Desktop and Web should call these APIs through the shared API/client contract. They should not use the old duplicated `api/users/login` and `api/users/register` flow as the final login/register path.

This task is not responsible for Desktop shell/session wiring. It must, however, provide the login/profile data that Task 8 needs to build one Desktop session.

## Related Audit Context

Helpful audit documents exist in `docs/audits`. They explain how this task relates to the rest of the `.Desktop + .Api` task set and should be read before implementation:

- `docs/audits/desktop-api-final-workflow.md`
- `docs/audits/desktop-api-contract-audit.md`
- `docs/audits/desktop-api-department-task-breakdown.md`
- `docs/audits/desktop-api-10-task-assignment-plan.md`

This task corresponds to Task 3 in the 10-task plan. It is one of the four parallel API feature lanes that can start after Task 2 has decided the canonical route/service ownership.

## Where This Fits In The Workflow

The department workflow is:

```text
Task 1
-> Task 2
-> Task 3 / Task 4 / Task 5 / Task 6
-> Task 7
-> Task 8 / Task 9 / Task 10
```

Task 3 starts after Task 1 defines the identity/session contract and Task 2 decides the final API route ownership.

Task 3 can run in parallel with:

- Task 4, Games/Filter/Search API;
- Task 5, Request/Rental Lifecycle API;
- Task 6, Chat/Notifications/Payments API.

This task must coordinate strongly with those tasks because they all depend on the same user identity. Games need owner IDs, rental requests need renter and owner IDs, notifications need recipient account IDs, and chat may still need the legacy integer user ID.

This task does not own routing work and does not own DB seed/setup work. Those are assumed to be handled by other owners before the full task set is assigned. This task may document what demo accounts or roles are required, but it should not become the seed task.

## Main Goal

Transform the auth/account/admin API from this situation:

```text
canonical api/auth exists but is not fully settled
canonical api/accounts exists but DTO names are inconsistent
canonical api/admin exists but admin authorization is not enforced by API yet
old api/users login/register still exists from the other project
Desktop session cannot rely on a complete login response yet
chat/filter may still need a legacy integer user id
Program.cs does not wire the active services yet
```

Into this situation:

```text
api/auth is the final login/register/logout route group
api/accounts is the final profile/password/avatar route group
api/admin is the final account administration route group
login/profile response contains the fields needed by one Desktop session
old api/users login/register is legacy/quarantined, not final
admin-only operations are protected by API rules
auth/account/admin services use repositories behind services
Task 8 can build Desktop auth/session without guessing the contract
```

## Current State From The Codebase

The current API already contains service-layer auth/account/admin controllers:

- `BoardGamesApp/BoardGames.Api/Controllers/AuthController.cs`
- `BoardGamesApp/BoardGames.Api/Controllers/AccountsController.cs`
- `BoardGamesApp/BoardGames.Api/Controllers/AdminController.cs`

The current API already contains service interfaces and implementations:

- `BoardGamesApp/BoardGames.Api/Services/IAuthService.cs`
- `BoardGamesApp/BoardGames.Api/Services/AuthService.cs`
- `BoardGamesApp/BoardGames.Api/Services/IAccountService.cs`
- `BoardGamesApp/BoardGames.Api/Services/AccountService.cs`
- `BoardGamesApp/BoardGames.Api/Services/IAdminService.cs`
- `BoardGamesApp/BoardGames.Api/Services/AdminService.cs`
- `BoardGamesApp/BoardGames.Api/Services/IAvatarStorageService.cs`
- `BoardGamesApp/BoardGames.Api/Services/AvatarStorageService.cs`

Relevant DTO files currently include:

- `BoardGamesApp/BoardGames.Shared/DTO/LoginDTO.cs`
- `BoardGamesApp/BoardGames.Shared/DTO/RegisterDTO.cs`
- `BoardGamesApp/BoardGames.Shared/DTO/AccountProfileDTO.cs`
- `BoardGamesApp/BoardGames.Shared/DTO/RoleDTO.cs`
- `BoardGamesApp/BoardGames.Shared/DTO/ChangePasswordDTO.cs`
- `BoardGamesApp/BoardGames.Shared/DTO/ResetPasswordDTO.cs`
- `BoardGamesApp/BoardGames.Shared/DTO/AvatarUploadResponseDTO.cs`
- `BoardGamesApp/BoardGames.Shared/DTO/UserDTO.cs`

Relevant Shared API client/proxy files currently include:

- `BoardGamesApp/BoardGames.Shared/ProxyServices/IAuthService.cs`
- `BoardGamesApp/BoardGames.Shared/ProxyServices/AuthService.cs`
- `BoardGamesApp/BoardGames.Shared/ProxyServices/IAccountService.cs`
- `BoardGamesApp/BoardGames.Shared/ProxyServices/AccountService.cs`
- `BoardGamesApp/BoardGames.Shared/ProxyServices/IAdminService.cs`
- `BoardGamesApp/BoardGames.Shared/ProxyServices/AdminService.cs`

Relevant data model/repository files currently include:

- `BoardGamesApp/BoardGames.Data/Models/User.cs`
- `BoardGamesApp/BoardGames.Data/Models/Role.cs`
- `BoardGamesApp/BoardGames.Data/Models/UserAccountRole.cs`
- `BoardGamesApp/BoardGames.Data/Models/FailedLoginAttempt.cs`
- `BoardGamesApp/BoardGames.Data/Repositories/IAccountRepository.cs`
- `BoardGamesApp/BoardGames.Data/Repositories/IFailedLoginRepository.cs`

The data model already shows the identity split that this task must respect:

```text
Guid Id        -> public account identity
int PamUserId  -> legacy/internal identity used by older chat/rental tables
```

The current `AuthService.LoginAsync` returns `AccountProfileDataTransferObject`. That response includes the `Guid` account ID, username, display name, email, phone number, avatar URL, country/city/address fields, suspended state, and role. It does not clearly expose `PamUserId`, even though the final merged workflow may still need it for chat/rental compatibility.

The current `AuthController`, `AdminController`, and some service interfaces still import old namespaces such as `BoardRentAndProperty.Api.Utilities` and `BoardRentAndProperty.Contracts.DataTransferObjects`. The final codebase should move toward the accepted project and namespace contract from Task 1.

The current `AccountsController` references `AccountProfileDTO` in its action signature, while the DTO file currently declares `AccountProfileDataTransferObject`. Similar naming confusion appears in Desktop view models. This task should settle the auth/account/admin DTO names or coordinate the decision with the Shared owner.

The old duplicated user route still exists:

- `BoardGamesApp/BoardGames.Api/Controllers/UsersController.cs` exposes old repository-based login/register/balance/address behavior.
- `BoardGamesApp/BoardGames.Api/Controllers/UsersController2.cs` exposes `api/users/except/{excludeAccountId}` through `IUserService`.

The old `api/users/login` and `api/users/register` path should not remain the final login/register route. Any user lookup still needed by chat should be clearly separated from auth.

`BoardGames.Api/Program.cs` currently registers controllers and Swagger only. It does not register `IAuthService`, `IAccountService`, `IAdminService`, repositories, mappers, or auth/security helpers. Full runtime wiring belongs to Task 7, but Task 3 must make the required auth/account/admin dependencies clear.

## Owned Route Groups

This task owns these final route groups:

```text
api/auth
api/accounts
api/admin
```

Required auth routes:

```text
POST /api/auth/login
POST /api/auth/register
POST /api/auth/logout
GET  /api/auth/forgot-password
```

Required account routes:

```text
GET    /api/accounts/{accountId}
PUT    /api/accounts/{accountId}
PUT    /api/accounts/{accountId}/password
POST   /api/accounts/{accountId}/avatar
DELETE /api/accounts/{accountId}/avatar
```

Required admin routes:

```text
GET /api/admin/accounts
PUT /api/admin/accounts/{accountId}/suspend
PUT /api/admin/accounts/{accountId}/unsuspend
PUT /api/admin/accounts/{accountId}/reset-password
PUT /api/admin/accounts/{accountId}/unlock
```

If the team keeps a user lookup route for chat, it should be documented separately from login/register. For example, `api/users/except/{accountId}` may remain as a user lookup route for Task 6, but it should not be treated as the final authentication route group.

## Required Behavior

Authentication behavior:

- login by username or email;
- register a standard user account;
- logout, even if the first local version is stateless;
- forgot password behavior if kept;
- consistent error responses for invalid credentials, suspended account, locked account, duplicate username/email, and validation failures;
- no final login/register behavior through the old `api/users/login` or `api/users/register` route.

Account behavior:

- get profile by account ID;
- update profile fields used by the Account page;
- change password;
- upload avatar if avatar support remains;
- remove avatar if avatar support remains;
- return profile data in the same shape Desktop uses for session and Account page display.

Admin behavior:

- list accounts with paging;
- suspend account;
- unsuspend account;
- unlock locked account;
- reset password;
- enforce admin authorization through the API, not only by hiding the Desktop button;
- return clear errors for non-admin attempts, missing account, invalid reset password, and locked/suspended account cases.

Validation behavior:

- registration should validate required fields;
- registration should validate password and confirm password;
- registration should check duplicate username and duplicate email;
- password rules should be consistent between registration, change password, and admin reset password;
- profile update should validate display name, phone number, email uniqueness, and address fields where applicable.

## Required Contract With Desktop

The login response must provide enough data for one Desktop session.

Required fields:

- account ID;
- legacy user ID if chat/rental flow still needs it;
- username;
- display name;
- email;
- role;
- avatar URL;
- account status;
- locked status if the UI/admin flow needs it;
- profile fields needed by the Account page.

The current `AccountProfileDataTransferObject` covers many of these fields, but it does not clearly include the legacy `PamUserId`. If Task 1 decides that Desktop should temporarily store the legacy integer ID for chat/rental compatibility, this task must either add it to the login/profile DTO contract or provide a clear API/service translation path so Desktop does not guess or hardcode it.

Task 8 will use this contract to build Desktop auth/session. Task 3 must make sure Task 8 does not need to invent session fields.

## Concrete Responsibilities

Confirm that these are the canonical final controllers:

- `AuthController` for `api/auth`;
- `AccountsController` for `api/accounts`;
- `AdminController` for `api/admin`.

Confirm that these are the canonical final service interfaces and services:

- `IAuthService` and `AuthService`;
- `IAccountService` and `AccountService`;
- `IAdminService` and `AdminService`;
- `IAvatarStorageService` and `AvatarStorageService`, if avatar support remains.

Make the final auth/account/admin contract explicit:

- request DTO for login;
- request DTO for register;
- response DTO for login;
- profile DTO;
- change password DTO;
- reset password DTO;
- avatar response DTO;
- API error response format.

Decide the DTO naming and namespace direction for this lane:

- avoid having `AccountProfileDTO` and `AccountProfileDataTransferObject` mean two different things;
- avoid old `BoardRentAndProperty.*` transport namespaces in final API contracts unless Task 1 explicitly keeps them temporarily;
- keep DTOs in Shared according to the accepted Task 1 dependency contract.

Handle the old duplicated auth route:

- mark old `api/users/login` as legacy/quarantined;
- mark old `api/users/register` as legacy/quarantined;
- preserve only user lookup behavior that other tasks still need;
- coordinate with Task 2 before removing, excluding, or renaming old user files.

Define admin authorization:

- decide how the API identifies the current caller as an administrator;
- ensure admin operations are not protected only by Desktop visibility;
- if full auth middleware is deferred to Task 7, document exactly what Task 7 must wire;
- do not leave final admin routes as open operations.

Document dependencies required by Task 7:

- `IAuthService`;
- `IAccountService`;
- `IAdminService`;
- `IAvatarStorageService`;
- `IAccountRepository`;
- `IFailedLoginRepository`;
- `AccountProfileMapper`;
- password/security helpers;
- file/avatar storage configuration.

## Current Problems This Task Addresses

The current code has these auth/account/admin problems:

- duplicate sign-in/sign-up flow exists from old project 2 through `api/users/login` and `api/users/register`;
- account/admin uses `Guid` account identity while chat/filter/rental paths may still need `int PamUserId`;
- the login response does not clearly expose every field needed by the final Desktop session;
- admin API routes currently do not show a final API-side role authorization strategy;
- `Program.cs` does not register auth/account/admin services and repositories yet;
- old namespaces still appear in active auth/account/admin API files;
- DTO naming is inconsistent around `AccountProfileDTO` and `AccountProfileDataTransferObject`;
- password policy is not clearly consistent between register, change password, and admin reset password.

This blocks Desktop because Desktop cannot unify session until the login/profile response is clear and stable.

## Coordination With Other Tasks

Task 2, API Duplicate Cleanup And Legacy Quarantine:

- decides whether the old `UsersController` files are active, compatibility, or legacy;
- decides whether `api/users` is final for user lookup only or fully quarantined;
- prevents this task from accidentally reviving duplicate login/register.

Task 4, Games/Filter/Search API:

- needs the canonical account ID for game owner behavior;
- needs admin role rules for admin all-games behavior.

Task 5, Request/Rental Lifecycle API:

- needs the same renter and owner identity contract;
- needs a clear rule for `Guid AccountId` versus `int PamUserId`.

Task 6, Chat/Notifications/Payments API:

- may need user lookup for conversation participants;
- may need the legacy user ID if chat tables still use `PamUserId`;
- needs notifications to target the same account identity returned by login/profile.

Task 7, API Runtime Wiring And Local Backend Smoke:

- registers this task's final services, repositories, mappers, and auth/security dependencies;
- verifies that `api/auth`, `api/accounts`, and `api/admin` can be constructed and called locally.

Task 8, Desktop App Shell, API Client Config, And Auth Session:

- consumes the login/profile DTO and role names from this task;
- populates the final Desktop session;
- uses the returned role to decide admin navigation.

## Implementation Hints

Use the existing `AuthController`, `AccountsController`, and `AdminController` as the likely canonical route shape because they already expose the correct route groups and call services instead of repositories.

Do not use the old `UsersController.cs` repository-based login/register as the final login/register path. If old user behavior is still useful, move or assign the useful behavior to the canonical auth/account/user lookup service path.

Keep repository access behind services:

```text
AuthController -> IAuthService -> IAccountRepository / IFailedLoginRepository
AccountsController -> IAccountService -> IAccountRepository
AdminController -> IAdminService -> IAccountRepository / IFailedLoginRepository
```

Do not expose repository behavior directly from controllers.

Use the existing `PasswordHasher` and `PasswordValidator` direction, but make the password rules consistent across registration, profile password change, and admin reset password.

If the final app has no server-side token/session yet, be explicit. A stateless local logout may be acceptable for this assignment phase only if Desktop session clearing is handled by Task 8, but admin routes still need a final API-side authorization rule.

If avatar upload remains, keep the file size rule and `IAvatarStorageService` behavior clear. Avatar upload/remove should update the account profile and return a URL that Desktop/Web can display.

Do not add broad new auth infrastructure without checking Task 1 and Task 7. This task owns the auth/account/admin API behavior and contract; Task 7 owns full runtime wiring and local smoke verification.

## Expected Output

This task should produce:

- canonical auth/account/admin API contract;
- final login/profile DTO decision;
- decision about legacy `PamUserId` exposure or translation;
- documented admin authorization rule;
- documented role names used by API and Desktop;
- documented password validation rule;
- documented old `api/users/login` and `api/users/register` legacy/quarantine decision;
- list of services/repositories/mappers Task 7 must register;
- clear note of any DTO or namespace conflicts that need Shared coordination.

The output may be included in the task handoff, pull request description, or a small markdown decision file under `docs/audits` if the team wants repository-tracked decisions.

## What Counts As Done

Desktop can login through `api/auth/login` and receive enough data to populate one session.

Desktop can register through `api/auth/register` if register remains in the final Desktop flow.

Desktop can load and update profile through `api/accounts`.

Desktop can change password and handle logout behavior through the canonical auth/account APIs.

Avatar upload/remove works through `api/accounts` if avatar support remains part of the final app.

Admin account list and admin account actions are available through `api/admin`.

Admin-only operations are blocked for non-admin users by the API-side rule, not only by Desktop button visibility.

Old `api/users/login` and `api/users/register` are not the final login/register flow.

The final login/profile DTO contract is clear enough for Task 8 to build Desktop auth/session without guessing.

Task 7 has a clear dependency list for runtime registration.

The task owner has documented unrelated blockers instead of taking ownership of the whole solution build.

## Do Not Touch

Do not implement Games, Filter, Search, Requests, Rentals, Chat, Notifications, Payments, Dashboard, or Desktop feature wiring.

Do not try to fix the entire solution build. The current application may not build yet because the merge is incomplete and unrelated areas may still contain errors.

Do not write tests for this task unless the lead explicitly changes the task.

Do not take over DB seed/setup work.

Do not take over routing work.

Do not move business logic into Desktop, Web, or Shared API clients.

Do not use hardcoded users, hardcoded roles, or hardcoded account IDs to make the workflow appear to work.

Do not create a second final login/register API path.

Do not redesign the whole Shared/Data boundary inside this task. If a DTO or namespace conflict blocks this feature, coordinate with the Shared owner and document the dependency.

## Known Blockers And Assumptions

This task assumes Task 1 has defined the identity rule, especially whether Desktop needs `Guid AccountId` only or also needs `int PamUserId` temporarily.

This task assumes Task 2 has decided what happens to duplicate `UsersController` and `IUserService` artifacts. If Task 2 is not complete, this task should not delete or revive old user routes independently.

The application may not build at the start of this task. The owner should fix errors directly related to auth/account/admin API files, DTOs, and contracts, but should not fix unrelated Desktop, Web, tests, DB seed, routing, games, rentals, chat, notification, or payment errors.

`Program.cs` full dependency injection wiring is not the main ownership of this task. Task 3 should make the active dependencies clear and may update its own registration needs, but Task 7 owns the final API runtime wiring and local backend smoke.

Admin authorization depends on the final auth mechanism. If token/session middleware is not yet available, the owner must document the dependency for Task 7 instead of pretending Desktop-only visibility is enough.

The final result of this task should make the project 1 account system the one account system for the merged application.
