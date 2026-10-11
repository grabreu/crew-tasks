# api

## Repository

.NET (ASP.NET Core) API of shared-todo, Vertical Slice Architecture: one slice per use case, with command/query + handler + endpoint colocated. General rules, Git, and Documentation conventions are in the root `CLAUDE.md`; this file only adds what is specific to `apps/api/`.

---

## Project-Specific Guidelines

### Source

Modular monolith: one project per module (plus a `.Contracts` project for what other modules may reference), hosted by `SharedTodo.Api`. Modules talk through events in `*.Contracts`, never by referencing each other's internals.

- `SharedTodo.slnx` - solution.
- `src/SharedTodo.Api/` - host: `Program.cs`, `ValidationBehavior` (FluentValidation in the Mediator pipeline), `GlobalExceptionHandler`, OpenAPI transformers (Scalar in Development).
- `src/SharedTodo.Users/` - Users module (ASP.NET Core Identity, JWT, Google sign-in).
  - `Features/` - one file per slice: request/command, validator, handler, and endpoint colocated.
  - `Data/` - `UsersDbContext` (schema `Users`), configurations, EF Core migrations, design-time factory.
  - `Identity/`, `Jwt/`, `Google/` - `ApplicationUser`, token issuing, Google ID token validation.
- `src/SharedTodo.Users.Contracts/` - integration events published by Users (`EmailSignInStartedEvent`).
- `src/SharedTodo.Notifications/` - Notifications module: handles Users events and sends email through an in-memory queue and a background worker (MailKit/SMTP).
- `src/SharedTodo.AppHost/` - .NET Aspire host for local development: SQL Server container, MailPit, and the API with its configuration.
- `src/SharedTodo.ServiceDefaults/` - Aspire service defaults (OpenTelemetry, health checks, resilience).

Commands, queries and events go through `Mediator` (source generator), results use `Desfecho`; packages are versioned centrally in `Directory.Packages.props`.

Each module exposes `Add<Module>ModuleServices` and `Map<Module>ModuleEndpoints` extension methods that `Program.cs` calls. Options (`Jwt`, `Authentication:Google`, `Email`) are validated on start, so the API does not boot without them.

### Validation

Run from `apps/api/`: `dotnet restore`, `dotnet build --no-restore -c Release`, `dotnet format --no-restore --verify-no-changes --severity info` before considering a change done.

### Open Questions

- TODO: rotated refresh tokens (only short-lived access tokens are issued today).
- TODO: no to-do list features or SignalR yet.
