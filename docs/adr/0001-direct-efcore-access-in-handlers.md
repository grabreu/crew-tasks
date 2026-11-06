# Direct EF Core access in handlers, no repository or query abstraction

Each module's `DbContext` (`UsersDbContext`, `TodosDbContext`) is injected directly into command and query handlers: no `IRepository`, no separate query objects. EF Core's `DbSet<T>` already is the repository and unit-of-work pattern; wrapping it behind another interface would only abstract EF Core to swap it for another EF Core, and no query here is complex enough to justify one (same reasoning as [product-catalog ADR 0003](https://github.com/grabreu/product-catalog/blob/main/docs/adr/0003-direct-efcore-access-in-handlers.md)).

Domain rules live in the entities instead: they have private setters and change through methods (for example `TodoList.Rename`, and the owner membership created together with the list), so a handler cannot put an entity in a state the rules forbid.

**Consequences**: handlers are coupled to EF Core, which is acceptable since there is no plan to swap ORMs, and Vertical Slice already means each handler owns its persistence code. Each module owns its own `DbContext`; a handler never uses another module's.
