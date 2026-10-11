# shared-todo

## Repository

Collaborative to-do list for small groups (family, school group, small team): a shared list of items, synced in real time for everyone viewing it. Not a project-management tool, no boards.

Read `README.md` before making changes: it documents the project pitch and tech stack. Significant, hard-to-reverse decisions are recorded in `docs/adr/` (created with the first one): check it before revisiting one, and add an entry when making a new one.

This is a monorepo. Each app has its own `CLAUDE.md` with its source layout, validation commands, and open questions; read the one for the app you are changing. The rules in this file apply to both.

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

- `apps/api/` - .NET (ASP.NET Core) API, Vertical Slice Architecture. See `apps/api/CLAUDE.md`.
- `apps/web/` - React SPA, feature-based folders, installable as a minimal PWA. See `apps/web/CLAUDE.md`.
- `.github/` - `dependabot.yml` (actions, NuGet, npm).

Toolchain files (`CLAUDE.md`, `README.md`, tool config, toolchain-specific `.gitignore` patterns) live inside the app folder. The root only holds what spans both apps: `.gitignore` (env and OS files), `.gitattributes`, and `.github/`.

### Validation

Run the commands listed in the `CLAUDE.md` of each app you changed (`apps/api/`, `apps/web/`) before considering a change done.

### Open Questions

- TODO: finish auth, working end-to-end between the SPA and API. The API already has ASP.NET Core Identity with passwordless email-code sign-in, Google sign-in, and JWT access tokens; missing: rotated refresh tokens and the SPA sign-in flow. Remaining work of milestone 0 (Foundation); see the project's idea draft for the full milestone order.
