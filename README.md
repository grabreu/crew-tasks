# crew-tasks

[![CI](https://github.com/grabreu/crew-tasks/actions/workflows/ci.yml/badge.svg?branch=main)](https://github.com/grabreu/crew-tasks/actions/workflows/ci.yml)
[![CD](https://github.com/grabreu/crew-tasks/actions/workflows/cd.yml/badge.svg?branch=main)](https://github.com/grabreu/crew-tasks/actions/workflows/cd.yml)
[![License](https://img.shields.io/github/license/grabreu/crew-tasks?style=flat-square)](LICENSE)

A backend for collaborative to-do lists shared by small groups (family, a school group, a small team), synced in real time for everyone looking at a list. Not a project-management tool, no Trello-style boards.

_Backend only, on purpose: there is no web frontend. The interface is the API reference (Scalar); a terminal client to show the real-time sync is planned (TODO, not started)._

**[Try it live →](https://crew-tasks.grabreu.dev)**

Status: early stage. To-do list features and SignalR are not implemented yet.

## Tech stack

.NET 10 (ASP.NET Core), modular monolith (Users, Notifications) with Vertical Slice Architecture · ASP.NET Core Identity + JWT · EF Core + SQL Server · Mediator + FluentValidation + Desfecho · MailKit · .NET Aspire for local orchestration · Scalar · planned: SignalR

## Features

- **Passwordless sign-in**: a 6-digit code sent by email, exchanged for a short-lived access token; the user is created on first sign-in.
- **Onboarding**: the display name is set once after the first sign-in.
- **Current user**

| Method | Route | Auth | Purpose |
| --- | --- | --- | --- |
| POST | `/auth/email/sign-in` | anonymous | Sends the sign-in code by email. |
| POST | `/auth/email/sign-in/complete` | anonymous | Exchanges email + code for an access token. |
| GET | `/users/me` | bearer | Current user. |
| POST | `/users/me/onboarding` | bearer | Sets the display name once. |

## Development

Requires the .NET SDK version in `global.json` and a container runtime (Docker), since the AppHost starts SQL Server and MailPit containers.

```bash
dotnet restore
dotnet run --project src/CrewTasks.AppHost
```

The AppHost starts SQL Server, MailPit (catches the sign-in code emails), and the API. Migrations run on startup, and the API reference (Scalar) is served at `/scalar`.

Other commands: `dotnet build --no-restore -c Release` (build), `dotnet format --no-restore --verify-no-changes --severity info` (formatting check, matches CI).

## Deployment

Auto-deployed to Azure Container Apps (Consumption plan) + Azure SQL (serverless, free tier) on every push to `main` via GitHub Actions, authenticated with an OIDC federated credential, so no Azure credential is stored in GitHub. The infrastructure is defined in Bicep (`infra/`). CI checks formatting, build, and the Docker image on every PR.

## License

Licensed under the [MIT License](LICENSE).
