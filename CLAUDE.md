# crew-tasks

## Repository

Backend for collaborative to-do lists shared by small groups (family, school group, small team), synced in real time for everyone viewing a list. Not a project-management tool, no boards. Backend only by design: no web frontend, the API reference (Scalar) is the interface. Scope stays lean: lists, items, members, and share-link invites; no comments, attachments, reminders, or labels.

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

Modular monolith: one project per module (plus a `.Contracts` project for what other modules may reference), hosted by `CrewTasks.Api`. Modules talk through events in `*.Contracts`, never by referencing each other's internals.

- `CrewTasks.slnx` - solution.
- `src/CrewTasks.Api/` - host: `Program.cs`, `ValidationBehavior` (FluentValidation in the Mediator pipeline), `GlobalExceptionHandler`, OpenAPI transformers (Scalar).
- `src/CrewTasks.Users/` - Users module (ASP.NET Core Identity, JWT, email-code sign-in).
  - `Features/` - one file per slice: request/command, validator, handler, and endpoint colocated.
  - `Data/` - `UsersDbContext` (schema `Users`), configurations, EF Core migrations, design-time factory.
  - `Identity/`, `Jwt/` - `ApplicationUser`, token issuing.
- `src/CrewTasks.Users.Contracts/` - integration events published by Users (`EmailSignInStartedEvent`).
- `src/CrewTasks.Todos/` - Todos module: to-do lists, their memberships (owner/member), and their items.
  - `Features/` - one file per slice, grouped by entity: `TodoLists/` (create, get one, get all, rename, delete) and `TodoItems/` (create, get one, get all, change title, complete, reopen, delete).
  - `Entities/` - `TodoList`, `TodoListMembership`, and `TodoItem` (private setters, changed through methods).
  - `Data/` - `TodosDbContext` (schema `Todos`), configurations, EF Core migrations, design-time factory.
- `src/CrewTasks.Todos.Contracts/` - integration events published by Todos (none yet).
- `src/CrewTasks.Notifications/` - Notifications module: handles Users events and sends email through an in-memory queue and a background worker (MailKit/SMTP).
- `src/CrewTasks.AppHost/` - .NET Aspire host for local development: SQL Server container, MailPit, and the API with its configuration.
- `src/CrewTasks.ServiceDefaults/` - Aspire service defaults (OpenTelemetry, health checks, resilience).
- `src/CrewTasks.Api/Dockerfile` - API image (built from the repo root as context).
- `infra/` - Bicep for Azure: `main.bicep`, `modules/sql-database.bicep`, `modules/container-app.bicep` (Container App using a system-assigned identity to reach SQL and ACR; the Container Apps environment and ACR are referenced as existing).
- `.github/` - `ci.yml` (format check, build/test, Docker build), `cd.yml` (push image to ACR, update the Container App in the `prod` environment), `dependabot.yml` (actions, NuGet, Docker).

Commands, queries and events go through `Mediator` (source generator), results use `Desfecho`; packages are versioned centrally in `Directory.Packages.props`.

All modules share one SQL database: each has its own `DbContext` (connection strings `UsersDb`, `TodosDb`), its own schema, and its own `__EFMigrationsHistory` table inside that schema; there are no foreign keys across schemas.

Auth issues short-lived JWT access tokens only (`AccessTokenExpirationMinutes`, 15 in `appsettings.json`); there is no refresh token by design, the client signs in again with a new email code.

Each module exposes `Add<Module>ModuleServices` and `Map<Module>ModuleEndpoints` extension methods that `Program.cs` calls. Options (`Jwt`, `Email`) are validated on start, so the API does not boot without them.

### Validation

Run from the repo root: `dotnet restore`, `dotnet build --no-restore -c Release`, `dotnet format --no-restore --verify-no-changes --severity info` before considering a change done.

### Open Questions

- TODO: terminal client to demonstrate real-time sync (not started; tech to define).
- TODO: no SignalR yet; lists, memberships, and items exist (milestone 1 of the project's idea draft is done). Next is milestone 2: optimistic concurrency on item mutations.
