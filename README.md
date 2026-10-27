# shared-todo

[![License](https://img.shields.io/github/license/grabreu/shared-todo?style=flat-square)](LICENSE)

A collaborative to-do list for small groups (family, a school group, a small team): a shared list of items, synced in real time for everyone looking at it. Not a project-management tool, no Trello-style boards.

Status: early stage. The API has login (email code and Google), current user, and onboarding; the web app is still a scaffold and no to-do features exist yet.

## Tech stack

.NET (ASP.NET Core), modular monolith with Vertical Slice Architecture, .NET Aspire for local orchestration · React SPA (Vite, TanStack Router, Tailwind) · EF Core + SQL Server · planned: SignalR

## Structure

Monorepo with one folder per app, each with its own setup instructions:

- [apps/api/](apps/api/README.md): ASP.NET Core API.
- [apps/web/](apps/web/README.md): React SPA.

## License

Licensed under the [MIT License](LICENSE).
