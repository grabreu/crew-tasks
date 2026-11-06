# One SQL database per repository, one schema per module

The Azure SQL free tier allows only a limited number of databases, shared by all of the owner's projects. Spending two databases on this one would eat into that quota for projects that still need one, so the rule is one database per repository, divided into schemas by module. The whole API therefore uses a single SQL database. Modules stay isolated inside it: each has its own `DbContext`, its own connection string name (`UsersDb`, `TodosDb`, both pointing at the same database), its own schema (`Users`, `Todos`), and its own `__EFMigrationsHistory` table inside that schema. Each module applies its own migrations at startup.

Modules never reference each other's tables: there are no foreign keys or joins across schemas, and one module refers to another's data only by id (for example `TodoList.OwnerId` is a user id without a foreign key to the Users schema).

**Consequences**: a module can move to its own database by changing its connection string, since nothing ties it to another schema. The trade-off is that the modules share the database's capacity and availability, and that application code must keep the isolation, since the database does not enforce it across schemas. The Users module originally kept its history table in `dbo`; databases created before the change must be recreated.
