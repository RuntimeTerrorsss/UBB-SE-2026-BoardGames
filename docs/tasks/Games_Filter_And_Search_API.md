# Task 4: Games, Filter, And Search API

**Workflow source:** Section 2, API Cleanup  
**Type:** Parallel API feature lane  
**Can start after:** Task 1 and Task 2 route/service decisions  
**Can run in parallel with:** Tasks 3, 5, 6  
**Suggested owner:** backend worker comfortable with game/search data  
**Primary project area:** `BoardGames.Api`  
**Secondary coordination area:** `BoardGames.Shared`, `BoardGames.Desktop`, `BoardGames.Data`

## What This Task Is About

This task creates one canonical games API for the merged application.

The final API must support both old project flows:

- Games / My Games / Admin Games from the first project;
- Filter / Discovery / Game Details from the second project.

The goal is not to keep two separate game APIs side by side. The goal is to merge the useful behavior from both old versions into one understandable backend surface.

After this task, Desktop and Web should know which games routes to call for listing, filtering, searching, game details, owner games, admin game management, create, edit, and delete/deactivate.

## Related Audit Context

Before starting, read the helpful audit documents from `docs/audits`.

The most important one for this task is:

- `docs/audits/desktop-api-10-task-assignment-plan.md`

This task belongs to the parallel API feature lane after the setup decisions from Task 1 and Task 2. It should not redo those setup tasks. If the canonical API route/service decisions are missing, this task is blocked and must coordinate with Task 2 instead of inventing a separate route structure.

## Where This Fits In The Workflow

The intended workflow is:

```text
Task 1 -> Task 2 -> Tasks 3 / 4 / 5 / 6 -> Task 7 -> Tasks 8 / 9 / 10
```

Task 4 can start after Task 1 and Task 2 have decided the common API structure and duplicate cleanup direction.

Task 4 can run in parallel with:

- Task 3, Auth / Account / Admin API;
- Task 5, Rentals / Requests / Availability API;
- Task 6, Chat / Notifications / Dashboard API.

Task 4 provides the game API contract needed later by:

- Task 9, Desktop Filter / Game Details / Rental Entry;
- Task 10, Desktop Games / My Games / Admin Games.

## Main Goal

Transform the current state from this:

```text
two game controllers
two GameDTO concepts
filter and my-games using different data paths
image represented differently in different flows
some routes exposing repository behavior directly
```

Into this:

```text
one canonical games controller
one canonical games/search service contract
one game list/details DTO decision
one game image contract
clear authorization rules for standard users and admins
filter, discovery, my-games, and admin games using the same backend source
```

## Current State From The Codebase

The API currently has duplicate game controller concepts.

`BoardGames.Api/Controllers/GamesController.cs` contains filter/discovery-style behavior. It exposes routes for listing games, filtering games, searching by filter criteria, game price, and discovery feeds. It currently injects a games repository directly, which does not match the final requirement that API controllers expose the service layer.

`BoardGames.Api/Controllers/GamesController2.cs` contains my-games/admin-style behavior. It exposes routes for all games, game details, owner games, active owner games, available renter games, create, update, and delete. This is closer to the required controller-to-service structure, but it does not preserve all filter/discovery behavior from the other controller.

The shared DTO layer also has duplicate game DTO concepts.

One game DTO is shaped for filter cards and discovery:

```text
GameId
Name
Image as string
Price
City
MinimumPlayerNumber
MaximumPlayerNumber
```

Another game DTO is shaped for my-games/create/edit:

```text
Id
Owner
Name
Price
MinimumPlayerNumber
MaximumPlayerNumber
Description
Image as byte[]
IsActive
```

This task must not randomly delete one side. It must define the final game DTO shape or split the contract intentionally into list, details, and mutation DTOs.

The current image handling is inconsistent. Some flows expect an image URL string, some flows expect raw image bytes, and some mapping code provides hardcoded image URLs or placeholder images. The final API must define how game images are returned and how game images are provided during create/edit.

## Owned Route Areas

This task owns the final decision and implementation direction for these route areas:

```text
api/games
api/games/{gameId}
api/games/owner/{ownerAccountId}
api/games/search or the chosen final filter route
```

The final route table should cover:

- list all active games for filter/discovery;
- list owner games for My Games;
- list active owner games if still needed;
- list all games for admin;
- get one game by id;
- create game;
- update game;
- delete or deactivate game;
- search/filter games;
- expose game price if Task 5 or Task 6 still needs a separate price endpoint.

If old feed endpoints are still required by Desktop/Web, decide whether they stay as explicit routes or become part of the unified search/filter route.

## Required Behavior

The canonical games API must support:

- listing all active games for anonymous and logged-in filter pages;
- listing games owned by one account for My Games;
- allowing admins to list and manage all games;
- loading complete game details by id;
- creating a game for the current owner;
- editing a game;
- deleting or deactivating a game;
- searching/filtering by name;
- searching/filtering by city or location;
- searching/filtering by price range or maximum price;
- searching/filtering by minimum and maximum player count;
- searching/filtering by availability once Task 5 defines the final availability contract;
- sorting by supported fields such as price, location, or other agreed fields;
- returning game images in one consistent way.

## Required Contract With Desktop And Web

The Desktop and Web layers must be able to use the same API contract.

For Filter and Discovery, the API must return enough information to render a game card:

- game id;
- game name;
- price;
- city or location;
- minimum player count;
- maximum player count;
- image display value;
- owner summary if needed by the card or details navigation;
- availability summary if Task 5 provides it.

For Game Details, the API must return enough information to open the rental/request flow:

- game id;
- game name;
- description;
- price;
- location;
- owner account/user information needed for chat/request flow;
- image display value;
- active/deactivated state;
- availability data or the route needed to fetch it from Task 5.

For My Games and Admin Games, the API must return enough information to manage games:

- owner account id;
- active/deactivated state;
- create/edit fields;
- image field;
- information needed to decide whether edit/delete buttons should be available.

The API must enforce authorization. Desktop hiding a button is not enough.

Standard users should manage only their own games. Admin users should be able to manage all games according to the final admin rules from Task 3.

## Concrete Responsibilities

Use the canonical controller and service direction decided by Task 2.

Merge the useful behavior from both old games controller versions into one final route surface.

Merge or replace the duplicate game DTO concepts with a clear final contract. It is acceptable to define separate DTOs for list, details, create, and update if that makes the API cleaner.

Decide the final game image contract:

- whether list/detail responses return image URLs, image bytes, or both;
- how create/edit sends image data;
- what placeholder behavior is used when a game has no image;
- whether image upload/remove remains part of the game API or is assigned to another route.

Make sure the final controller calls a service, not a repository directly.

Make sure the final game service uses the final data access layer behind the service boundary.

Preserve behavior that old Desktop/Web flows still need:

- filter/search;
- discovery feed if still used;
- game price lookup if still needed by rental/payment flows;
- owner games;
- available games for renter if still needed;
- active games for owner if still needed;
- create/edit/delete behavior.

Remove ambiguity from the old duplicated API artifacts by documenting which ones are:

- final active;
- temporary compatibility;
- legacy/quarantined;
- removed or excluded later.

## Current Problems This Task Addresses

The current codebase has:

- duplicate game controllers;
- duplicate `GameDTO` concepts;
- filter and my-games flows using different old data paths;
- game image data represented differently across flows;
- route ownership split between old project versions;
- controller code that still depends directly on repository behavior;
- admin game behavior separate from normal game management;
- old namespace usage mixed into the merged API surface.

This blocks Desktop and Web work because developers cannot safely know which game route or DTO is final.

## Coordination With Other Tasks

Coordinate with Task 3 for account identity, owner account ids, admin role checks, and user/session fields.

Coordinate with Task 5 for rental availability, booked dates, rental request entry, and the meaning of delete versus deactivate when a game already has rentals or requests.

Coordinate with Task 6 if notifications, dashboard, or payment history need game name, price, owner, renter, or image information.

Coordinate with Task 7 for dependency injection and runtime registration. This task should document required service/repository registrations, but Task 7 owns the final application startup wiring.

Coordinate with Task 9 for the Desktop Filter, Discovery, Game Details, and rental-entry screens.

Coordinate with Task 10 for Desktop My Games and Admin Games.

## Implementation Hints

The final controller structure should probably start from the service-layer controller shape, then preserve the useful filter/search behavior from the older filter controller.

Do not keep an API controller that directly injects `InterfaceGamesRepository` as the final design.

Do not leave two `GameDTO` classes with the same final meaning. If multiple DTOs are needed, name them by purpose, for example list, details, create, or update.

Do not keep UI-specific filter methods as part of the API contract. The API should accept clean request/query data and return clean response DTOs.

Be careful with user ids. Some old filter/discovery code uses an integer user id, while account/admin flows use account ids. The final API must use the identity decision from Task 3 and Task 2 instead of inventing a third identity model.

Be careful with game delete. If rentals, requests, conversations, notifications, or payments reference a game, deactivation may be safer than physical delete. The final decision must be coordinated with Task 5.

The application may not build at the moment. Do not try to fix the whole solution just to make this task look complete. Fix task-related errors only and document unrelated blockers clearly.

## Expected Output

This task should produce:

- canonical games/search API contract;
- final route ownership for games/search routes;
- final game list/details/create/update DTO decision;
- final game image decision;
- authorization rules for standard users and admins;
- clear preservation list for useful behavior from both old game APIs;
- clear list of legacy/quarantined game API artifacts;
- notes for Task 7 about required service/repository registrations;
- notes for Task 9 and Task 10 about which routes Desktop should call.

## What Counts As Done

Filter and My Games are designed to read from the same backend source.

The final API has one clear games/search route surface.

Standard users can manage only their own games through the API.

Admins can manage all games through the API according to the final admin rules.

The final game DTO and image contract are clear enough for Desktop and Web workers to implement against without guessing.

Task 9 can build Desktop Filter and Game Details against this route contract.

Task 10 can build Desktop My Games and Admin Games against this route contract.

## Do Not Touch

Do not implement Desktop routing.

Do not implement Web pages.

Do not take over Task 3 account/admin behavior.

Do not take over Task 5 rental/request/business availability behavior beyond the game-side contract.

Do not take over Task 6 chat/notification/dashboard behavior.

Do not take over final runtime startup wiring from Task 7.

Do not write tests unless the lead explicitly changes this task.

Do not try to fix unrelated build errors across the entire solution.

## Known Blockers And Assumptions

This task assumes Task 1 has prepared the solution dependency direction and removed the most important project-reference blockers.

This task assumes Task 2 has decided which duplicate API controller/service structure is canonical.

This task assumes DB seed/setup is handled separately before final integration.

This task assumes routing work is handled separately and should be consumed later by Desktop tasks.

If those assumptions are not true, document the blocker and coordinate with the responsible task owner instead of expanding this task into setup work.
