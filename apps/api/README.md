# api

ASP.NET Core API of [shared-todo](../README.md): modular monolith (Users, Notifications), Vertical Slice Architecture, EF Core + SQL Server, orchestrated locally with .NET Aspire.

## Development

Requires the .NET SDK version in `global.json` and a container runtime (Docker), since the AppHost starts SQL Server and MailPit containers.

```bash
dotnet restore
dotnet run --project src/SharedTodo.AppHost
```

The AppHost starts SQL Server (persistent, with a data volume), MailPit (catches the login code emails), and the API, and opens the Aspire dashboard. It prompts for the `google-client-id` parameter; `jwt-secret-key` is generated and persisted. In Development the API applies EF Core migrations on startup and serves the API reference (Scalar) at `/scalar`; find its URL in the dashboard.

Running `SharedTodo.Api` alone also works if you provide the `UsersDb` connection string, `Jwt:SecretKey`, `Authentication:Google:ClientId`, and `Email:Host` (configuration is validated on start).

## Endpoints

| Method | Route | Auth | Purpose |
| --- | --- | --- | --- |
| POST | `/auth/login/passwordless` | anonymous | Sends a 6-digit login code by email (creates the user if new). |
| POST | `/auth/login/passwordless/complete` | anonymous | Exchanges email + code for an access token. |
| POST | `/auth/login/google` | anonymous | Exchanges a Google ID token for an access token. |
| GET | `/users/me` | bearer | Current user. |
| POST | `/users/me/onboarding` | bearer | Sets the display name once. |

## Other commands

`dotnet build --no-restore` (build), `dotnet format --no-restore --verify-no-changes --severity info` (formatting check).

EF Core migrations live in `src/SharedTodo.Users/Data/Migrations`; the design-time factory targets `(localdb)\mssqllocaldb`.
