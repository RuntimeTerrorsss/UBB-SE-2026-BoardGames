# Task 4 Audit: Games, Filter, and Search Backend APIs

## Important Notes for Task 7 (API Runtime Wiring)

During the implementation of the canonical unified Games API, certain routing and identity elements were temporarily stubbed because the underlying authentication session mechanism (Task 3) and dependency injection wiring (Task 7) are not yet complete. 

Please address the following when integrating Task 4 code into the final backend:

### 1. Identity Placeholders

The newly unified `GamesController` uses placeholders for user identity in operations that require authentication:

*   **Create Game (`POST api/games`)**: The `GameCreateDTO` accepts an `OwnerAccountId` which is currently being passed to the service layer to establish game ownership. In the final implementation, the API should extract this `OwnerAccountId` directly from the user's authenticated session/token, rather than relying on it being sent in the request body.
*   **Update Game (`PUT api/games/{gameId}`)**: The endpoint currently accepts `requestingAccountId` and `isAdmin` as query parameters to enforce ownership/authorization. These must be replaced with session-derived identity checks (e.g., retrieving `AccountId` from the token and checking user roles).
*   **Delete Game (`DELETE api/games/{gameId}`)**: Similar to update, `requestingAccountId` and `isAdmin` are passed as query parameters. Task 7 should refactor this to use the auth context.

### 2. Omitted Feed Endpoints

The legacy games controller had feed-specific endpoints (`GET feed/tonight` and `GET feed/remaining`) that relied on a specific UI flow using raw integer user IDs. 

**Decision**: These have been intentionally omitted from the new canonical `GamesController`. If the Desktop or Web flows still require feed-like data, they should be serviced using the new robust search/filter endpoint (`POST api/games/search`), which now accepts detailed criteria including availability ranges.

### 3. DI Registrations

Ensure the following are registered when setting up the API runtime:
```csharp
builder.Services.AddScoped<InterfaceGamesRepository, GamesRepository>();
builder.Services.AddScoped<IRentalRepository, RentalRepository>();
builder.Services.AddScoped<IGameService, GameService>();
builder.Services.AddScoped<GameMapper>();
builder.Services.AddScoped<UserMapper>();
```
