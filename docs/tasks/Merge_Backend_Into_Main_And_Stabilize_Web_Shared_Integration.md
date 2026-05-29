# Merge Validated Backend Into Main And Stabilize Web/Shared Integration

**Workflow source:** final branch integration after the Desktop/API and Web/Shared task lanes  
**Type:** merge and stabilization task  
**Can start after:** `backend` branch is buildable and `main` contains the latest Web/Shared work  
**Can run in parallel with:** limited investigation only; final contract and conflict decisions must be coordinated  
**Suggested owner:** integration lead or senior developer comfortable with `.Api`, `.Desktop`, `.Shared`, and `.Web`  
**Primary project areas:** `BoardGames.Api`, `BoardGames.Shared`, `BoardGames.Web`, `BoardGames.Desktop`  
**Secondary coordination areas:** `BoardGames.Data`, solution/project configuration, local runtime configuration

## What This Task Is About

This task merges the validated `backend` branch into `main` and turns the result into one coherent application.

The `backend` branch is expected to contain the buildable `.Api` and `.Desktop` implementation from the completed Desktop/API task set, including Tasks 1 through 10 under `docs/tasks`.

The `main` branch is expected to contain important `.Web` and `.Shared` work that must be preserved where it is compatible with the final architecture and backend contracts.

The important detail is that buildable does not automatically mean correct.

Before `backend` becomes the integration baseline, the implementer must inspect it against the task and audit documents, run the important backend/Desktop workflows where practical, and record what is confirmed working, what is partially implemented, and what is blocked by environment, seed data, routing, or missing contracts.

After that validation, merge `backend` into `main`, resolve Git and semantic conflicts, align Web/Shared with the verified backend/API contracts, and confirm that previously completed behavior still works after the merge.

This is a merge and stabilization task. It is not a whole-application rewrite, not a new feature task, and not permission to clean up every unrelated problem in the solution.

## Global Application Direction

The final application must behave like one system with one backend, one database source, one identity model, and consistent Desktop/Web behavior.

Expected dependency direction:

```text
Desktop -> Shared DTOs/API clients -> API -> Services -> Data -> Database
Web     -> Shared DTOs/API clients -> API -> Services -> Data -> Database
```

The final result must respect these rules:

- `.Api` is the backend surface used by both client applications.
- `.Api` exposes service-layer behavior, not repositories directly from final controllers.
- `.Desktop` and `.Web` call backend functionality through Shared/API-client contracts.
- `.Shared` owns transport DTOs, API-client interfaces/implementations, and shared client-facing enums.
- `.Desktop` and `.Web` must not directly access `AppDbContext`, repositories, or duplicate backend business rules.
- Equivalent Desktop and Web workflows must use the same backend behavior and same authenticated user identity.
- Account, games, filter, rental requests, chat, notifications, dashboard, admin, and payment/history must feel like one merged application, not two old projects placed beside each other.

## Related Audit Context

Helpful audit documents exist in `docs/audits`. They explain the accepted Desktop/API workflow, contracts, dependency order, and merge direction. Review the documents that exist in the repository before making merge decisions:

- `docs/audits/desktop-api-final-workflow.md`
- `docs/audits/desktop-api-contract-audit.md`
- `docs/audits/desktop-api-department-task-breakdown.md`
- `docs/audits/desktop-api-10-task-assignment-plan.md`
- `docs/audits/task-1-architecture-boundary-identity-contract.md`
- `docs/audits/task-1-boundary-violation-inventory.md`
- `docs/audits/task-2-api-duplicate-cleanup-decisions.md`
- `docs/audits/task-4-games-filter-search-backend-apis.md`
- `docs/audits/task-7-api-runtime-wiring-analysis.md`

Also inspect the completed task files in `docs/tasks`, especially Tasks 1 through 10. These files describe what the backend/Desktop branch is supposed to preserve.

The audit and task files are context for this merge. They do not mean this task owns every feature again. They are used to decide which behavior is canonical, which branch should win a conflict, and which defects belong to this integration task.

## Branch Context

### `backend`

The `backend` branch should be treated as the candidate backend/Desktop baseline, not as automatically verified truth.

Inspect and verify:

- whether `.Api` builds and starts locally;
- whether Swagger exposes the expected route groups;
- whether `.Desktop` builds and starts;
- whether `.Desktop` connects to `.Api` through the configured local API URL;
- whether login populates the final Desktop session;
- whether Tasks 1 through 10 behavior works where practical;
- whether the backend uses one identity model and one database source;
- whether any buildable behavior is still functionally wrong;
- whether verification is blocked by seed data, local database setup, routing, environment configuration, or unavailable external services.

When `backend` behavior matches the documented expectations and is confirmed working, preserve it during the merge.

When `backend` builds but a documented completed backend/Desktop workflow is broken, fix only the scoped defect that belongs to the backend/Desktop task area or document the blocker if the required fix belongs outside this task.

Do not describe an unverified backend/Desktop workflow as working.

### `main`

The `main` branch should be treated as the source of current Web/Shared work that must be preserved where compatible with the final contracts.

Inspect:

- `.Web` controllers, views, models, infrastructure services, authentication/session behavior, and API usage;
- `.Shared` DTOs, enums, request/response shapes, API-client interfaces, and API-client implementations;
- Web functionality that already works or is nearly complete;
- Shared contracts that Web depends on;
- differences between `main` Shared contracts and verified backend behavior from `backend`;
- code that bypasses API, duplicates backend business logic, uses invalid contracts, or assumes a different identity model.

Valid Web/Shared functionality must be aligned with the merged backend rather than removed unnecessarily.

## Current Codebase Areas To Inspect

Before resolving conflicts, inspect the current files and folders that exist in the repository. Do not assume file names or namespaces from memory.

Important project files:

- `BoardGamesApp/BoardGames.Api/BoardGames.Api.csproj`
- `BoardGamesApp/BoardGames.Desktop/BoardGames.Desktop.csproj`
- `BoardGamesApp/BoardGames.Shared/BoardGames.Shared.csproj`
- `BoardGamesApp/BoardGames.Web/BoardGames.Web.csproj`
- the solution file used by the team

Important implementation areas:

- `BoardGamesApp/BoardGames.Api/Controllers`
- `BoardGamesApp/BoardGames.Api/Services`
- `BoardGamesApp/BoardGames.Api/Program.cs`
- `BoardGamesApp/BoardGames.Shared/DTO`
- `BoardGamesApp/BoardGames.Shared/ProxyServices`
- `BoardGamesApp/BoardGames.Desktop/Views`
- `BoardGamesApp/BoardGames.Desktop/ViewModels`
- `BoardGamesApp/BoardGames.Desktop/Services`
- `BoardGamesApp/BoardGames.Web/Controllers`
- `BoardGamesApp/BoardGames.Web/Views`
- `BoardGamesApp/BoardGames.Web/Models`
- `BoardGamesApp/BoardGames.Web/Infrastructure`
- `BoardGamesApp/BoardGames.Data` only where API compatibility or migrations directly require it

Current architecture should move toward:

```text
BoardGames.Api     -> BoardGames.Data + BoardGames.Shared
BoardGames.Desktop -> BoardGames.Shared
BoardGames.Web     -> BoardGames.Shared
BoardGames.Shared  -> no BoardGames.Data
```

Temporary references such as notification-server support may exist only if they were already accepted by the task/audit documents. Do not add forbidden references to make a conflict disappear.

## Ownership Boundaries

### This Task Owns

- inspecting the task/audit documents before merge decisions;
- validating the important `backend` branch workflows before using it as baseline;
- merging `backend` into `main`;
- resolving direct Git conflicts in the owned application area;
- resolving semantic conflicts where files compile but contracts, routes, identity, or behavior disagree;
- selecting one canonical Shared/API contract shape when branches disagree;
- adapting `.Web` to the verified backend/API contracts;
- preserving verified `.Desktop` behavior after integration;
- correcting build failures directly caused by or directly blocking the merge;
- documenting blockers outside the task scope.

### This Task Depends On

- the buildable `backend` branch;
- the latest `main` branch;
- accepted behavior from `docs/tasks`;
- architecture and contract guidance from `docs/audits`;
- existing routing and database seed/setup work where those areas were assigned elsewhere;
- local database/environment configuration needed for runtime verification.

### External Ownership

This task does not own:

- unrelated feature development;
- whole-solution redesign;
- unrelated routing redesign;
- new seed data creation unless a direct compatibility correction is required;
- unrelated database population work;
- broad warning cleanup;
- broad test repair;
- unrelated project cleanup;
- feature expansion beyond documented or already existing Web/Shared behavior.

When an external dependency prevents verification, document it as a blocker instead of creating an unsupported workaround.

## Required Work Order

Complete this task in the following order.

## Phase 1: Review Documentation And Build The Expected-Behavior Checklist

Before merging branches or editing implementation code, inspect the relevant task and audit documentation.

Identify:

- which backend/Desktop workflows are expected to be complete;
- accepted API routes and service behavior;
- required Shared DTO and API-client contracts;
- expected user identity/session behavior;
- how rental requests, chat, notifications, dashboard/payment, account, and admin should connect;
- which work is already externally owned;
- which old duplicate workflows must not be revived.

Create a working checklist of behavior that must be validated on `backend` before the merge and rechecked after the merge.

## Phase 2: Inspect And Validate `backend` Before Merge

Before merging `backend` into `main`, inspect and verify the candidate backend/Desktop baseline.

### Required Build Inspection

Restore and build the relevant projects on `backend`.

Record:

- commands used;
- projects built;
- build outcome;
- important warnings or failures that affect validation;
- unrelated failures that should be documented instead of fixed.

At minimum, inspect or build:

```text
BoardGames.Shared
BoardGames.Data
BoardGames.Api
BoardGames.Desktop
```

Build success is required evidence, but it is not enough to prove runtime functionality.

### Required Code Inspection

Inspect the backend/Desktop implementation related to Tasks 1 through 10:

- API controllers and route groups;
- API service interfaces and implementations;
- repository/data interaction required by API behavior;
- DTOs and request/response contracts;
- Shared API-client interfaces and implementations;
- Desktop app startup, shell, windows, session, services, pages, and view models;
- dependency injection registrations;
- local runtime configuration;
- authentication and user identity handling.

The goal is not to rewrite `backend`. The goal is to understand which behavior is actually safe to preserve as the integration baseline.

### Required Functional Verification

Run or manually inspect the workflows documented by Tasks 1 through 10 where environment and seed data allow it.

At minimum, verify the applicable parts of this flow:

```text
API starts locally
-> Swagger exposes final route groups
-> Desktop starts and connects to API
-> login creates one authenticated session
-> game list/filter loads real data
-> game details loads real data
-> rental request can be created
-> request/rental state can be inspected
-> chat/message state exists where implemented
-> owner notification exists where implemented
-> owner approval/decline works where implemented
-> renter notification/result state exists where implemented
-> account/dashboard/payment/admin behavior works where implemented
```

For each inspected workflow, record whether it is:

- confirmed working;
- fixed as part of scoped backend/Desktop validation;
- partially implemented according to existing task scope;
- blocked by an external dependency;
- broken outside this task scope.

### Scoped Corrections Before Merge

When `backend` builds but a documented completed `.Api` or `.Desktop` behavior is broken, fix it only when:

- it directly belongs to Tasks 1 through 10;
- it directly prevents establishing the backend/Desktop integration baseline;
- it can be corrected without expanding into unrelated feature or architecture work.

Do not repair unrelated Web, test, seed, routing, or warning problems during this pre-merge validation.

## Phase 3: Establish The Verified Integration Baseline

After reviewing documentation and validating `backend`, define the behavior that must be preserved during integration.

The merge source of truth is:

1. accepted architecture and workflow requirements documented under `docs/audits`;
2. completed feature expectations documented under `docs/tasks`;
3. `.Api` and `.Desktop` behavior from `backend` confirmed during pre-merge verification;
4. compatible `.Shared` and `.Web` functionality from `main`;
5. minimal scoped compatibility corrections needed where existing implementations do not align.

A buildable backend implementation that contradicts documented completed workflow must not override the documented requirement only because it compiles.

A Web or Shared implementation that conflicts with verified backend behavior should be adapted to the canonical merged contracts unless the backend implementation is shown to be incorrect against the documented expectations.

## Phase 4: Merge `backend` Into `main`

Merge `backend` into `main`.

If the team allows it, perform this work on a temporary integration branch created from `main`, then merge the final result into `main` after verification. If the lead requires direct work on `main`, keep the same validation and conflict-resolution process.

Resolve:

- direct Git conflicts;
- project reference and package conflicts;
- namespace conflicts;
- duplicate DTO and API-client conflicts;
- route and endpoint conflicts;
- identity/session conflicts;
- semantic conflicts where code compiles but calls the wrong route, uses the wrong DTO, or preserves the wrong old workflow.

A conflict is not resolved simply because conflict markers are removed. The final selected implementation must compile and respect the accepted architecture.

## Conflict Resolution Requirements

### Solution And Project Reference Conflicts

Review conflicts involving:

- solution project membership;
- `.csproj` project references;
- package references;
- target frameworks and platform settings;
- assembly and namespace differences;
- API-client dependencies;
- startup/bootstrap configuration;
- dependency injection registration.

The final structure must support:

```text
Desktop -> Shared DTOs/API clients -> API
Web     -> Shared DTOs/API clients -> API
```

Do not retain project references that violate the intended architecture merely because one branch had them.

### `.Shared` Contract Conflicts

Review every conflict or incompatibility involving:

- DTOs;
- request models;
- response models;
- API-client interfaces;
- API-client implementations;
- auth/session models;
- account/user identity representations;
- enums and shared status values;
- game, rental, request, chat, notification, payment/history, dashboard, or admin transport models.

For each affected contract:

- compare it with task/audit expectations;
- compare it with verified API behavior;
- compare it with existing Desktop usage;
- compare it with existing Web usage;
- choose one canonical final contract;
- update dependent Web or Desktop usage only as required;
- avoid creating duplicate Web-only and Desktop-only contracts for the same backend concept.

If the backend route or DTO needed for an existing workflow is genuinely missing or contradictory, document the dependency instead of inventing an undocumented workaround.

### `.Api` Conflicts

The API must remain the single backend behavior source for both clients.

For `.Api` conflicts:

- preserve verified routes and service behavior from `backend`;
- preserve business logic in the API service layer;
- keep legacy/quarantined files out of final active routes unless documentation says otherwise;
- make only scoped changes needed for correct Web/Shared compatibility or a validated backend defect;
- do not revive deprecated duplicate controllers or services;
- do not add unsupported endpoints only because a Web page expects a route that was never part of the accepted contract;
- ensure dependency injection and startup configuration support the merged clients.

### `.Desktop` Conflicts

Desktop must continue to work for behavior validated before the merge.

For `.Desktop` conflicts:

- preserve verified API-client usage;
- preserve verified authentication/session behavior;
- preserve validated completed workflows from Tasks 8 through 10;
- adapt Shared references only when the canonical final contract requires it;
- rerun important verified workflows after the merge;
- avoid replacing working Desktop behavior with incomplete alternatives from another branch.

### `.Web` Integration Conflicts

Web must be made compatible with the final Shared contracts and verified backend behavior.

Inspect and stabilize Web areas involving:

- authentication and authenticated identity;
- API base address/client setup;
- games listing, search, filtering, and details;
- rental request submission and state;
- chat and notifications where implemented;
- account, dashboard, payment/history, and admin functionality where implemented;
- form validation;
- API error handling;
- authorization-dependent navigation or UI behavior.

Web must:

- call the merged API for backend behavior;
- use canonical Shared DTOs and API clients where that is the accepted pattern;
- use the authenticated user identity from the final application flow;
- show understandable validation or error feedback;
- preserve compatible functionality already implemented on `main`.

Web must not:

- access the database directly for UI workflows;
- instantiate repositories or `AppDbContext` from controllers/views for final UI behavior;
- duplicate business rules already owned by `.Api`;
- use hardcoded users or dummy state to make workflows appear functional;
- introduce substitute local-only routes because a contract mismatch was not resolved.

## Required Final Architecture

### `.Api`

Owns:

- controllers and exposed routes;
- backend service-layer workflows;
- backend validation and permission rules;
- data/repository access through the accepted backend architecture;
- authenticated identity use in business operations.

### `.Shared`

Owns:

- DTOs used by client/API communication;
- shared request and response shapes;
- API-client interfaces and reusable client implementations where that is the project pattern;
- client-facing enums and shared transport concepts.

### `.Desktop`

Owns:

- Desktop UI behavior;
- Desktop presentation state;
- calling the backend through Shared/API-client contracts;
- displaying backend results and failures to the user.

### `.Web`

Owns:

- Web UI behavior;
- Web presentation state;
- calling the backend through Shared/API-client contracts;
- displaying backend results and failures to the user.

### `.Data`

May only be changed when a direct compatibility issue blocks a verified API workflow or required migration/runtime behavior. This task must not become unrelated data-layer cleanup or seed-data work.

## Expected Behavior After The Task

After this task:

- `backend` has been inspected before being used as the merge baseline;
- important documented backend/Desktop workflows have been validated or clearly recorded as blocked;
- `backend` has been merged into `main`;
- direct and semantic merge conflicts in the owned integration area have been resolved;
- `.Api`, `.Desktop`, `.Shared`, and `.Web` build together unless a clearly unrelated external blocker is documented;
- `.Api` acts as the shared backend for Desktop and Web;
- `.Desktop` retains the behavior confirmed working before the merge;
- `.Shared` provides coherent transport contracts and API-client integration for both clients;
- `.Web` uses the final API/Shared contract model and preserves compatible work from `main`;
- equivalent Desktop and Web workflows use the same backend and same user identity model;
- users receive validation or friendly error messages instead of raw exceptions or misleading success states.

## Required Verification After Merge

After resolving conflicts, rerun build and functional verification.

### Build Verification

Verify:

- dependencies restore successfully;
- `BoardGames.Shared` builds;
- `BoardGames.Data` builds if API runtime or migrations depend on it;
- `BoardGames.Api` builds;
- `BoardGames.Desktop` builds;
- `BoardGames.Web` builds;
- the owned merged application area builds together.

Fix errors directly caused by or directly blocking this merge task.

Document unrelated failures instead of expanding the task to solve unrelated solution problems.

### Backend/Desktop Regression Verification

Repeat the backend/Desktop workflows that were confirmed working before the merge.

At minimum, recheck the applicable validated flow:

```text
API startup
-> Desktop startup and API connectivity
-> login/authenticated user identity
-> game list and filtering
-> game details
-> rental request creation
-> rental request visibility/status
-> chat and notifications where implemented
-> approval/decline and resulting state where implemented
-> account/dashboard/payment/history/admin behavior where included and previously validated
```

Any behavior confirmed working before the merge that fails afterward is a merge regression and belongs to this task when caused by integration.

### Web/Shared Verification

For Web functionality present in `main`, verify where implemented:

- Web starts successfully;
- authentication/session state uses the final identity model;
- pages use final Shared contracts;
- API calls target valid merged endpoints;
- games/filter/details load through API behavior;
- rental workflows use backend behavior;
- chat and notification functionality uses backend contracts where implemented;
- account/dashboard/admin functionality uses backend contracts where implemented;
- failures are surfaced clearly to users.

Where a Web feature was already incomplete independently of the merge, document that state instead of inventing feature expansion.

## Parallel Work Guidance

After the documentation review and verified backend baseline are established, limited parallel work is acceptable where files and contract decisions do not overlap.

Work that may be handled in parallel:

- `.Shared` contract alignment after the canonical contract is decided;
- `.Web` integration against the agreed contract;
- build/configuration corrections for owned projects;
- post-merge Desktop/API regression checking.

The following must not be finalized independently before canonical decisions are made:

- authentication/session integration;
- Shared DTO shapes;
- rental/chat/notification endpoint usage;
- duplicate contract removal;
- project-reference decisions dependent on Shared ownership;
- API behavior changes justified only by Web assumptions.

## Implementation Rules

Apply these rules throughout the task:

- No comments are allowed in code unless an existing task explicitly permits them.
- Follow existing StyleCop and file-structure conventions.
- Use descriptive names.
- Keep edits scoped to this merge and stabilization assignment.
- Do not assume functionality works because it compiles.
- Do not attempt whole-solution cleanup because unrelated problems are discovered.
- Do not add tests unless the lead explicitly asks for tests.
- Fix errors directly caused by, directly exposed by, or directly blocking this task.
- Document unrelated blockers rather than taking ownership of unrelated work.
- Use dependency injection where the project already follows that pattern.
- Controllers and services should receive dependencies through constructors.
- Do not move business logic into Desktop views, Web views, Razor pages, or JavaScript.
- When backend behavior exists, Desktop and Web must call the correct backend/API-client contract and display the result.
- Handle failures clearly through user-facing validation or error messages.
- Respect one backend, one identity model, and one backend data source across Desktop and Web.

## Deliverables

This task must produce:

1. A pre-merge validation record for `backend`, including:
   - documentation inspected;
   - build commands executed and results;
   - workflows inspected;
   - workflows confirmed working;
   - scoped defects corrected before merge;
   - workflows blocked from verification and the reason.
2. A completed merge of `backend` into `main`.
3. Logically resolved merge conflicts in the owned integration area.
4. A coherent final `.Shared` contract and API-client layer used consistently by `.Desktop` and `.Web`.
5. `.Web` integration aligned to verified backend/API behavior.
6. Preserved `.Api` and `.Desktop` behavior for workflows confirmed working before the merge.
7. A merged build verification record for `.Api`, `.Desktop`, `.Shared`, and `.Web`.
8. A post-merge workflow verification record showing:
   - backend/Desktop regression results;
   - Web/Shared integration results;
   - unresolved external blockers;
   - remaining incomplete functionality already outside this task scope.

The validation records may be written in a PR description, a handoff note, or a small tracked markdown file if the team wants repository history for the decisions.

## What Counts As Done

The relevant audit and task documents were inspected before implementation decisions were finalized.

`backend` was inspected and functionally evaluated before being used as the merge baseline.

Buildability of `backend` was confirmed, but compilation alone was not treated as proof of completed behavior.

Relevant backend/Desktop workflows from Tasks 1 through 10 were verified where practical, fixed when directly in scope, or documented as blocked.

`backend` was merged into `main`.

Direct Git conflicts and semantic integration conflicts were resolved logically.

The final project dependency direction follows the accepted architecture.

`.Api`, `.Desktop`, `.Shared`, and `.Web` build together unless an unrelated blocker is clearly documented.

Backend/Desktop functionality confirmed working before integration still works after the merge.

Compatible Web/Shared functionality in `main` works against the merged backend and final Shared contracts.

Desktop and Web use the same backend, same identity model, and coherent equivalent contracts.

No hardcoded user workaround, duplicated UI business logic, obsolete endpoint revival, or unapproved local substitute behavior was added.

The implementer produced clear records of verification results and blockers.

## Do Not Touch

Do not treat `backend` as functionally correct only because it builds.

Do not skip inspection of documented backend/Desktop workflows before merging.

Do not rewrite verified working `.Api` or `.Desktop` behavior without a direct merge or compatibility need.

Do not redesign the entire application architecture.

Do not take ownership of unrelated routing redesign.

Do not take ownership of unrelated database seed/setup work.

Do not rewrite unrelated controllers, services, pages, components, view models, DTOs, repositories, or data access code.

Do not add unrelated features.

Do not add tests unless explicitly required by the lead.

Do not invent missing API routes or DTOs without checking existing contracts and documentation.

Do not revive deprecated duplicate controllers, services, pages, or workflows.

Do not move business logic into Desktop UI code, Web UI code, or JavaScript.

Do not access the database directly from Desktop or Web.

Do not use hardcoded users, dummy state, or fake success behavior.

Do not fix unrelated warnings, unrelated namespace issues, unrelated test failures, or unrelated project build errors simply to claim broader completion.

Do not describe an unverified workflow as working.

If an API route, DTO, identity contract, routing dependency, seed-data dependency, environment problem, or unrelated project failure prevents verification, record it as a blocker instead of introducing an unsupported workaround.

## Known Assumptions And Possible Blockers

### Assumptions

- `backend` is currently buildable.
- `backend` contains the `.Api` and `.Desktop` implementation associated with Tasks 1 through 10.
- `main` contains existing nearly completed `.Web` and `.Shared` implementation.
- Relevant audit and task documents exist and describe accepted behavior.
- Routing work and database seed/setup work are completed or externally owned unless a direct compatibility correction is required by this merge.

### Possible Blockers

Document rather than bypass:

- missing or contradictory audit/task requirements;
- missing API routes required by already documented Web or Desktop behavior;
- incompatible or duplicated DTOs with no documented canonical contract;
- incompatible identity/session assumptions requiring broader architecture ownership;
- unavailable seed data required only for runtime demonstration;
- missing local environment configuration;
- external routing behavior preventing page-level verification;
- unrelated build failures outside the owned project area;
- Web functionality already incomplete independently of this merge.

## Final Acceptance Summary

The merged `main` branch must represent one application rather than separate backend/Desktop and Web/Shared implementations placed beside each other.

The implementer must first establish what actually works on the buildable `backend` branch, then merge it into `main`, preserve verified backend/Desktop behavior, align valid Web/Shared functionality to the same backend contracts, resolve conflicts logically, build the owned application area, and document any external blocker that prevents full verification.
