# crew-tasks

## Repository

Backend for collaborative to-do lists shared by small groups (family, school group, small team). Not a project-management tool, no boards. Backend only by design: no web frontend, the API reference (Scalar) is the interface. Scope stays lean: lists, items, members, and share-link invites; no comments, attachments, reminders, or labels.

Read `README.md` before making changes: it documents the project pitch, tech stack, and how to run it. Significant, hard-to-reverse decisions are recorded in `docs/adr/`: check it before revisiting one, and add an entry when making a new one.

## General Rules

- Keep changes scoped to the requested change.
- Prefer existing patterns over introducing new abstractions.
- Do not add dependencies unless they are necessary.
- Do not fill gaps with assumptions when the user hasn't given the information: ask, or mark it as pending.
- Do not claim a validation command passed unless it was actually run.
- Code, comments, commit messages, and documentation are always written in English.
- Do not use em dashes; use a comma, colon, semicolon, parentheses, or a separate sentence instead.

## Git

- Do not create or switch branches unless explicitly requested.
- Do not create commits unless explicitly requested.
- Do not push unless explicitly requested.
- Keep commits focused on the requested change.
- Commit messages follow [Conventional Commits](https://www.conventionalcommits.org/) (`type: summary`).

## Documentation

### Audience

Future-you revisiting this months later, or someone browsing the portfolio to see how it works. Not onboarding material; keep it concise and skimmable.

### Content Rules

- State facts concisely. Avoid unnecessary explanations or trailing rationale.
- Do not document information that is already obvious from the repository structure or configuration.
- Do not invent features, API shapes, or future direction: mark undecided things as TODO.
- Document a capability only after it is implemented and verified.
- Use proper Markdown headings (`##`, `###`), not bold text as headings.

---

## Project-Specific Guidelines

### Source

.NET (ASP.NET Core) API, Vertical Slice Architecture: one slice per use case, with command/query + handler + endpoint colocated.

Modular monolith: one project per module (plus a `.Contracts` project for what other modules may reference), hosted by `CrewTasks.Api`. Modules talk through events and lookup interfaces in `*.Contracts` (ADR 0007), never by referencing each other's internals.

- `CrewTasks.slnx` - solution.
- `src/CrewTasks.Api/` - host: `Program.cs`, `ValidationBehavior` (FluentValidation in the Mediator pipeline), exception handlers (`DbConcurrencyExceptionHandler`, then the fallback `GlobalExceptionHandler`), OpenAPI transformers (Scalar).
- `src/CrewTasks.Users/` - Users module (ASP.NET Core Identity, JWT, email-code sign-in).
  - `Features/` - one file per slice: request/command, validator, handler, and endpoint colocated.
  - `Data/` - `UsersDbContext` (schema `Users`), configurations, EF Core migrations, design-time factory.
  - `Identity/`, `Jwt/` - `ApplicationUser`, token issuing.
- `src/CrewTasks.Users.Contracts/` - integration events published by Users (`EmailSignInStartedEvent`) and `IUserDirectory`, which resolves user ids to display name and email (implemented in `Users/Identity/UserDirectory.cs`; used by Todos for member names).
- `src/CrewTasks.Todos/` - Todos module: to-do lists, their memberships (owner/member), and their items.
  - `Features/` - one file per slice, grouped by entity: `TodoLists/` (create, get one, get all, rename, delete), `TodoItems/` (create, get one, get all, change title, complete, reopen, delete), `TodoListInvites/` (create, get all, revoke, accept), and `TodoListMembers/` (get all, remove).
  - `Entities/` - `TodoList`, `TodoListMembership`, `TodoListInvite`, and `TodoItem` (private setters, changed through methods).
  - `Data/` - `TodosDbContext` (schema `Todos`), configurations, EF Core migrations, design-time factory.
- `src/CrewTasks.Todos.Contracts/` - integration events published by Todos (`TodoListMemberJoinedEvent`, published when someone joins a list through an invite).
- `src/CrewTasks.Notifications/` - Notifications module: handles Users and Todos events and sends email through an in-memory queue and a background worker (MailKit/SMTP). The member-joined email looks up the owner's email through `IUserDirectory` and never fails the request that triggered it (errors are logged).
- `src/CrewTasks.AppHost/` - .NET Aspire host for local development: SQL Server container, MailPit, and the API with its configuration.
- `src/CrewTasks.ServiceDefaults/` - Aspire service defaults (OpenTelemetry, health checks, resilience).
- `src/CrewTasks.Api/Dockerfile` - API image (built from the repo root as context).
- `infra/` - Bicep for Azure: `main.bicep`, `modules/sql-database.bicep`, `modules/container-app.bicep` (Container App using a system-assigned identity to reach SQL and ACR; the Container Apps environment and ACR are referenced as existing).
- `.github/` - `ci.yml` (format check, build and `dotnet test` (no test projects yet), Docker build), `cd.yml` (push image to ACR, update the Container App in the `prod` environment), `dependabot.yml` (actions, NuGet, Docker).

Commands, queries and events go through `Mediator` (source generator), results use `Desfecho`; packages are versioned centrally in `Directory.Packages.props`.

All modules share one SQL database: each has its own `DbContext` (connection strings `UsersDb`, `TodosDb`), its own schema, and its own `__EFMigrationsHistory` table inside that schema; there are no foreign keys across schemas.

Todo items are a flat resource (`/todo-items`, addressed by their own id); the list id travels in the body on creation and as a filter on listing. Access to a list is checked inside each handler, by querying through the user's membership; a user who is not a member gets 404. Invites are nested under the list (`/todo-lists/{id}/invites`) and only the owner manages them (403 for members); an invite is a random token valid for 7 days, usable by several people until revoked, and accepting it (`/todo-list-invites/{token}/accept`) adds a `Member` membership, idempotently; unusable tokens get 404. Leaving and removing share one slice (`DELETE /todo-lists/{id}/members/{userId}`): a member removes themselves, the owner removes anyone else (403 for members), the owner cannot leave (409), and a removed user can rejoin through an active invite.

Item mutations (change title, complete, reopen, delete) require the item's `rowVersion` (SQL Server `rowversion`) in the `If-Match` header: the handler compares it with the loaded value and responds 409 when stale, and `DbConcurrencyExceptionHandler` maps a `DbUpdateConcurrencyException` (a race between load and save) to 409 ahead of the fallback `GlobalExceptionHandler`.

Auth issues short-lived JWT access tokens only (`AccessTokenExpirationMinutes`, 15 in `appsettings.json`); there is no refresh token by design, the client signs in again with a new email code.

Each module exposes `Add<Module>ModuleServices` and `Map<Module>ModuleEndpoints` extension methods that `Program.cs` calls. Options (`Jwt`, `Email`) are validated on start, so the API does not boot without them.

### Validation

Run from the repo root: `dotnet restore`, `dotnet build --no-restore -c Release`, `dotnet format --no-restore --verify-no-changes --severity info` before considering a change done.
