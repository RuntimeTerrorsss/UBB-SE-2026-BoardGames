# Task Markdown Writing Rules

This file describes how task markdown files should be written for the merged BoardGames application.

## How To Write A Task Markdown File

Task files are project requirement and research briefs. They are not system prompts, chat answers, commit messages, or vague TODO notes.

Before writing a task file, analyse the current codebase and the relevant audit documents. For the current assignment, always understand the merged-application goal first: the old board-rent/login project and the other team project must behave like one application, with one backend, one database, one user identity, and consistent Desktop/Web behavior.

When writing future task files, assume the routing work and DB seed/setup work are already completed unless the user explicitly says otherwise. Mention routing or seed data only as existing context when it helps explain the assigned task; do not write the task as if the implementer owns routing or DB seed/setup.

Use these audit documents as general context when they exist:

- `docs/audits/desktop-api-final-workflow.md`
- `docs/audits/desktop-api-contract-audit.md`
- `docs/audits/desktop-api-department-task-breakdown.md`
- `docs/audits/desktop-api-10-task-assignment-plan.md`

Write the task for one assigned feature owner or one assigned task package. Do not write instructions as if the implementer owns the whole merge. The task should clearly say what belongs to that owner, what is external, and what should not be touched.

Use the same style as the existing task files: clear, direct, and specific. The document should be detailed enough that an implementer can understand the feature, make a plan, and implement it without needing to ask what the task means.

Each task file should normally contain:

- task name and purpose;
- where this task fits in the global workflow;
- related audit context from `docs/audits`, so the implementer understands how this task connects to the other tasks in the same task set;
- current state of the codebase for this assigned area;
- exact project area, such as `.Api`, `.Desktop`, `.Shared`, `.Web`, or `.Data`;
- relevant files, folders, controllers, pages, view models, services, DTOs, repositories, and endpoints;
- expected behavior after the task is done;
- dependency on other task owners;
- what can be done in parallel;
- what must wait for another task;
- implementation hints based on the current codebase;
- what counts as done for this task;
- what should not be touched;
- known blockers or assumptions.

Implementation hints are strong suggestions based on the codebase, but the implementer should still understand the feature and make good decisions. Do not write task files that encourage blind copy-paste execution.

Do not include explicit code examples unless they are necessary to remove ambiguity. Prefer describing behavior, architecture, data flow, routes, DTOs, permissions, ownership, and UI expectations.

Every task file should include a short section named `Related Audit Context`. That section should mention that helpful markdown documents exist in `docs/audits` and should list the most relevant ones for the task. For this assignment, the default audit references are:

- `docs/audits/desktop-api-final-workflow.md`
- `docs/audits/desktop-api-contract-audit.md`
- `docs/audits/desktop-api-department-task-breakdown.md`
- `docs/audits/desktop-api-10-task-assignment-plan.md`

The section should not tell the implementer to own all audit tasks. It should explain that the audit files are context for understanding dependencies, task order, and how this task relates to the other assigned tasks.

## Current Assignment Context

The current assignment is a merge/stabilization assignment, not a normal single-feature assignment.

The final application should follow this direction:

```text
Desktop -> Shared DTOs/API clients -> API -> Services -> Data -> Database
Web     -> Shared DTOs/API clients -> API -> Services -> Data -> Database
```

The `.Api` project should expose service-layer behavior. The `.Desktop` and `.Web` applications should call the backend through API/client contracts and should not own business logic or database access.

For the `.Desktop + .Api` department, task files should focus as much as possible on:

- `BoardGames.Api`;
- `BoardGames.Desktop`;
- contracts needed from `BoardGames.Shared`;
- data access behavior only when it is required to make the API service work.

Do not make a `.Desktop + .Api` task secretly become a `.Web`, `.Shared`, `.Data`, or whole-solution cleanup task unless the assignment explicitly says so.

## Common Implementation Rules For Every Task

No comments are allowed in code unless the task explicitly permits them.

Follow the existing StyleCop and file-structure conventions.

Use descriptive names. Do not introduce one-character variable names.

Keep edits scoped to the assigned task. Do not modify unnecessary files and do not implement more than the task requires.

The application cannot be assumed to build perfectly at the start of a task. The current codebase is a partial merge and may contain unrelated errors. The implementer must not try to fix the entire solution just to make their task look complete.

Do not make tests for these tasks unless the particular task explicitly asks for tests. The priority is to implement the assigned application behavior and keep the work scoped.

Fix errors that are directly caused by, or directly blocking, the assigned task. Do not fix unrelated build errors, unrelated warnings, unrelated namespace problems, unrelated tests, or unrelated UI flows.

If an unrelated error blocks verification, document it as a blocker instead of taking ownership of that whole area.

Use dependency injection for instantiations where the project already follows that pattern. Controllers and services should receive dependencies through constructors instead of manually creating services, repositories, HTTP clients, or database contexts.

Do not move business logic into Desktop views, Web views, or JavaScript. Business behavior should stay in the API service layer whenever it already exists there or belongs there.

When the API already implements a workflow, Desktop/Web should call the correct API/client endpoint and display the result. They should not duplicate the backend logic.

Handle failures clearly. Users should see validation messages or error alerts, not crashes or raw exceptions.

Respect the final merge goal: the user should not feel that two old applications were glued together. Account, games, filter, rental requests, chat, notifications, dashboard, and admin must use one user identity and one backend data source.

Every task file must clearly describe ownership boundaries:

- what this task owns;
- what this task depends on;
- what another task owner handles;
- what the implementer must not touch;
- what to report as a blocker instead of fixing globally.

## Scope Rules For AI-Assisted Implementation

Task files should be written so that an AI assistant or a human implementer cannot accidentally expand the scope.

Every task should include a section called `Do Not Touch` or `Out Of Scope`.

For the current assignment, common out-of-scope items are:

- do not try to fix the whole build;
- do not rewrite unrelated controllers, pages, services, DTOs, or repositories;
- do not add tests unless explicitly requested;
- do not invent missing API routes or DTOs without checking the contract;
- do not revive deprecated duplicate controllers/pages;
- do not move business logic into UI code;
- do not use hardcoded users to make a workflow appear to work.

If an API route or DTO is missing, the implementer should stop and report the dependency instead of creating a workaround outside the task.

## Prompt Pattern For Creating A Task File

Use this pattern when asking to create a new task markdown file:

```text
Using `docs/task-md-writing-rules.md`, create `<path-to-task-file>.md`.

Before writing it, analyse the current codebase and the relevant audit documents in `docs/audits`.

Write the document in the same style as the existing task files: as a project requirement/research brief, not as a system prompt and not as a chat answer.

Include the common implementation rules from `docs/task-md-writing-rules.md`, but adapt the task-specific parts yourself: current state, files involved, expected final shape, API endpoints, DTOs, Desktop/Web behavior, what counts as done, what should not be touched, links with other task owners, blockers, and ownership boundaries.

Add a `Related Audit Context` section. Mention that `docs/audits` contains helpful markdown files for understanding how this task relates to the other tasks in the assignment, and list the audit files that are relevant for this task.

Mention clearly that the current application may not build yet. The implementer should not try to fix the entire solution. They should implement strictly inside the assigned task area, fix only errors related to that task, and document unrelated blockers.

Do not ask for tests unless the specific task requires tests.

The particular task is:

<write the specific task here>
```
