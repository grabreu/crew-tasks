# crew-tasks

[![CI](https://github.com/grabreu/crew-tasks/actions/workflows/ci.yml/badge.svg?branch=main)](https://github.com/grabreu/crew-tasks/actions/workflows/ci.yml)
[![CD](https://github.com/grabreu/crew-tasks/actions/workflows/cd.yml/badge.svg?branch=main)](https://github.com/grabreu/crew-tasks/actions/workflows/cd.yml)
[![License](https://img.shields.io/github/license/grabreu/crew-tasks?style=flat-square)](LICENSE)

A backend for collaborative to-do lists shared by small groups (family, a school group, a small team). Not a project-management tool, no Trello-style boards.

_Backend only, on purpose: there is no web frontend. The interface is the API reference (Scalar)._

**[Try it live →](https://crew-tasks.grabreu.dev)**

Status: early stage. To-do lists and items with optimistic concurrency are implemented.

## Tech stack

.NET 10 (ASP.NET Core), modular monolith (Users, Todos, Notifications) with Vertical Slice Architecture · ASP.NET Core Identity + JWT · EF Core + SQL Server · Mediator + FluentValidation + Desfecho · MailKit · .NET Aspire for local orchestration · Scalar

## Features

- **Passwordless sign-in**: a 6-digit code sent by email, exchanged for a short-lived access token; the user is created on first sign-in.
- **Onboarding**: the display name is set once after the first sign-in.
- **Current user**
- **To-do lists**: create, list, rename, and delete lists. The creator is the owner and only the owner can delete; a list you do not belong to responds 404.
- **Members and invites**: the owner creates invite links (a token valid for 7 days, usable by several people until it expires or is revoked), lists the active ones, and revokes them. Anyone signed in can accept a token to join the list as a member (accepting twice has no effect; an unknown, expired, or revoked token responds 404). Any member can list the members (user id, display name, role); the email is never exposed. The owner gets an email when someone joins through an invite (not sent when an already-member accepts again). A member can leave the list and the owner can remove members; the owner cannot leave (409), and a removed person can rejoin through an active invite. Non-owners get 403 on invites and on removing others.
- **To-do items**: create an item in a list, list your items across all your lists (filtered by list or completion), change the title, complete, reopen, and delete. Any member of the list can change its items; an item of a list you do not belong to responds 404.
- **Optimistic concurrency**: every item carries a `rowVersion`. Changing the title, completing, reopening, and deleting require the current value in the `If-Match` header; a stale value responds 409 and the client reloads the item. The three changes that keep the item return the updated item with its new `rowVersion`.

| Method | Route | Auth | Purpose |
| --- | --- | --- | --- |
| POST | `/auth/email/sign-in` | anonymous | Sends the sign-in code by email. |
| POST | `/auth/email/sign-in/complete` | anonymous | Exchanges email + code for an access token. |
| GET | `/users/me` | bearer | Current user. |
| POST | `/users/me/onboarding` | bearer | Sets the display name once. |
| POST | `/todo-lists` | bearer | Creates a list owned by the current user. |
| GET | `/todo-lists` | bearer | Lists the current user's lists, oldest first. |
| GET | `/todo-lists/{todoListId}` | bearer | Gets a list the current user belongs to. |
| PUT | `/todo-lists/{todoListId}` | bearer | Renames a list. |
| DELETE | `/todo-lists/{todoListId}` | bearer | Deletes a list and its items (owner only). |
| GET | `/todo-lists/{todoListId}/members` | bearer | Lists the members of a list (id, display name, role), in join order. |
| DELETE | `/todo-lists/{todoListId}/members/{userId}` | bearer | Leaves the list (own id) or removes a member (owner only); the owner cannot leave. |
| POST | `/todo-lists/{todoListId}/invites` | bearer | Creates an invite token for a list (owner only). |
| GET | `/todo-lists/{todoListId}/invites` | bearer | Lists the active invites of a list (owner only). |
| DELETE | `/todo-lists/{todoListId}/invites/{inviteId}` | bearer | Revokes an invite (owner only). |
| POST | `/todo-list-invites/{token}/accept` | bearer | Joins the list of an invite as a member. |
| POST | `/todo-items` | bearer | Creates an item in a list (`todoListId` in the body). |
| GET | `/todo-items` | bearer | Lists the items of the user's lists, oldest first; optional `todoListId` and `isCompleted` filters. |
| GET | `/todo-items/{todoItemId}` | bearer | Gets an item. |
| PUT | `/todo-items/{todoItemId}/title` | bearer | Changes the title of an item (`If-Match` required). |
| POST | `/todo-items/{todoItemId}/complete` | bearer | Completes an item (`If-Match` required). |
| POST | `/todo-items/{todoItemId}/reopen` | bearer | Reopens a completed item (`If-Match` required). |
| DELETE | `/todo-items/{todoItemId}` | bearer | Deletes an item (`If-Match` required). |

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
