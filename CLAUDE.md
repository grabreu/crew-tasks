# crew-tasks

## Repository

Backend for collaborative to-do lists shared by small groups (family, school group, small team), synced in real time for everyone viewing a list. Not a project-management tool, no boards. Backend only by design: no web frontend, the API reference (Scalar) is the interface.

Read `README.md` before making changes: it documents the project pitch, tech stack, and how to run it. Significant, hard-to-reverse decisions are recorded in `docs/adr/` (created with the first one): check it before revisiting one, and add an entry when making a new one.

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
- `src/CrewTasks.Api/` - host: `Program.cs`, `ValidationBehavior` (FluentValidation in the Mediator pipeline), `GlobalExceptionHandler`, OpenAPI transformers (Scalar in Development).
- `src/CrewTasks.Users/` - Users module (ASP.NET Core Identity, JWT, email-code sign-in).
  - `Features/` - one file per slice: request/command, validator, handler, and endpoint colocated.
  - `Data/` - `UsersDbContext` (schema `Users`), configurations, EF Core migrations, design-time factory.
  - `Identity/`, `Jwt/` - `ApplicationUser`, token issuing.
- `src/CrewTasks.Users.Contracts/` - integration events published by Users (`EmailSignInStartedEvent`).
- `src/CrewTasks.Notifications/` - Notifications module: handles Users events and sends email through an in-memory queue and a background worker (MailKit/SMTP).
- `src/CrewTasks.AppHost/` - .NET Aspire host for local development: SQL Server container, MailPit, and the API with its configuration.
- `src/CrewTasks.ServiceDefaults/` - Aspire service defaults (OpenTelemetry, health checks, resilience).
- `.github/` - `dependabot.yml` (actions, NuGet).

Commands, queries and events go through `Mediator` (source generator), results use `Desfecho`; packages are versioned centrally in `Directory.Packages.props`.

Each module exposes `Add<Module>ModuleServices` and `Map<Module>ModuleEndpoints` extension methods that `Program.cs` calls. Options (`Jwt`, `Email`) are validated on start, so the API does not boot without them.

### Validation

Run from the repo root: `dotnet restore`, `dotnet build --no-restore -c Release`, `dotnet format --no-restore --verify-no-changes --severity info` before considering a change done.

### Open Questions

- TODO: scope is meant to stay lean: lists, items, members, and share-link invites; no comments, attachments, reminders, or labels. Confirm before building.
- TODO: terminal client to demonstrate real-time sync (not started; tech to define).
- TODO: finish auth: rotated refresh tokens (only short-lived access tokens are issued today). Remaining work of milestone 0 (Foundation); see the project's idea draft for the milestone order.
- TODO: no to-do list features or SignalR yet.
