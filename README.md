# crew-tasks

[![License](https://img.shields.io/github/license/grabreu/crew-tasks?style=flat-square)](LICENSE)

A backend for collaborative to-do lists shared by small groups (family, a school group, a small team), synced in real time for everyone looking at a list. Not a project-management tool, no Trello-style boards.

Backend only, on purpose: there is no web frontend. The interface is the API reference (Scalar); a terminal client to show the real-time sync is planned (TODO, not started).

Status: early stage. The API has sign-in (email code and Google), current user, and onboarding; no to-do features or SignalR yet.

## Tech stack

.NET (ASP.NET Core), modular monolith (Users, Notifications) with Vertical Slice Architecture, .NET Aspire for local orchestration · EF Core + SQL Server · planned: SignalR

## Development

Requires the .NET SDK version in `global.json` and a container runtime (Docker), since the AppHost starts SQL Server and MailPit containers.

```bash
dotnet restore
dotnet run --project src/CrewTasks.AppHost
```

The AppHost starts SQL Server (persistent, with a data volume), MailPit (catches the sign-in code emails), and the API, and opens the Aspire dashboard. It prompts for the `google-client-id` parameter; `jwt-secret-key` is generated and persisted. In Development the API applies EF Core migrations on startup and serves the API reference (Scalar) at `/scalar`; find its URL in the dashboard.

Running `CrewTasks.Api` alone also works if you provide the `UsersDb` connection string, `Jwt:SecretKey`, `Authentication:Google:ClientId`, and `Email:Host` (configuration is validated on start).

## Endpoints

| Method | Route | Auth | Purpose |
| --- | --- | --- | --- |
| POST | `/auth/email/sign-in` | anonymous | Sends a 6-digit sign-in code by email (creates the user if new). |
| POST | `/auth/email/sign-in/complete` | anonymous | Exchanges email + code for an access token. |
| POST | `/auth/google/sign-in` | anonymous | Exchanges a Google ID token for an access token. |
| GET | `/users/me` | bearer | Current user. |
| POST | `/users/me/onboarding` | bearer | Sets the display name once. |

## Other commands

`dotnet build --no-restore` (build), `dotnet format --no-restore --verify-no-changes --severity info` (formatting check).

EF Core migrations live in `src/CrewTasks.Users/Data/Migrations`; the design-time factory targets `(localdb)\mssqllocaldb`.

## License

Licensed under the [MIT License](LICENSE).
