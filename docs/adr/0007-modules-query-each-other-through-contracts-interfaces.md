# Modules query each other through interfaces in `*.Contracts`

Events remain how modules react to each other. When a module needs data owned by another one, it calls an interface declared in the owner's `.Contracts` project, implemented inside the owner module and resolved through dependency injection. The call is in-process and synchronous; the caller references only the contracts project, never the other module's internals.

The first one is `IUserDirectory` (`CrewTasks.Users.Contracts`): it resolves user ids to display name and email in one batch call. Todos uses it to put display names in the member list; Notifications is the next intended caller, to find a recipient's email (not implemented yet). Todos only knows `UserId`, and the data lives in the Users schema, so a join is not possible (no foreign keys across schemas, see ADR 0003). The email is returned to in-process callers only; no Todos endpoint exposes it.

Considered and not adopted:
- Copying the name or email into the Todos tables: duplicated data that goes stale when the user changes it.
- Carrying the data in events: it does not help when the recipient is someone other than the actor (for example the list owner when a member joins).
- A Mediator query declared in `*.Contracts`: equivalent, with an extra layer of indirection for a lookup that is not a use case.

**Consequences**: a lookup fails together with its caller, since there is no network between them. Callers must handle ids with no match (a user that no longer exists) and use the batch call instead of one lookup per row. Each new interface of this kind widens what a module exposes, so it belongs in `.Contracts` only when events do not fit.
