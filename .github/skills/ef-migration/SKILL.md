---
name: ef-migration
description: Guides agents through adding, applying, and managing EF Core migrations in this solution. Use when the domain model or entity configuration changes and the database schema must be updated.
---

# EF Core Migration

## Overview
Migrations live in `Bizca.{Service}.Infrastructure/Context/Migrations/`. They are generated from `Bizca.{Service}.Infrastructure` (migrations project) with `Bizca.{Service}.Api` as the startup project. In development (`Development` or `Local` environments), migrations are auto-applied at startup via `ApplyMigrationsAsync`.

## When to Use
- A new entity configuration was added or an existing one changed column names, types, relationships, or indexes.
- A new seed (`HasData`) was added to a referential configuration.
- NOT for data-only scripts → create a separate SQL script in `database-build/scripts/`.

## Steps

### 1. Add a migration (from repo root)
```powershell
dotnet ef migrations add <MigrationName> `
  --project microservices/user/src/Bizca.Users.Infrastructure/Bizca.Users.Infrastructure.csproj `
  --startup-project microservices/user/src/Bizca.Users.Api/Bizca.Users.Api.csproj `
  --context Bizca.Users.Infrastructure.Context.ApplicationDbContext
```
Replace `<MigrationName>` with a descriptive PascalCase name (e.g. `AddOrderTable`).

### 2. Review the generated migration
Check the generated file in `Context/Migrations/`. Confirm:
- Only expected changes are included.
- No accidental drops of unrelated tables/columns.

### 3. Apply migrations locally
**Via Aspire (recommended):** Run the `Bizca.Users.AppHost` project — it starts Postgres + the API, which calls `MigrateAsync()` on startup automatically.

**Direct `dotnet ef` apply:**
```powershell
dotnet ef database update `
  --project microservices/user/src/Bizca.Users.Infrastructure/Bizca.Users.Infrastructure.csproj `
  --startup-project microservices/user/src/Bizca.Users.Api/Bizca.Users.Api.csproj `
  --context Bizca.Users.Infrastructure.Context.ApplicationDbContext
```

### 4. Auto-apply on startup
In `Program.cs`, `ApplyMigrationsAsync()` runs when the environment is `Development` or `Local`:
```csharp
if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Local"))
{
    await app.ApplyMigrationsAsync();
}
```
This calls `context.Database.MigrateAsync()` — do **not** remove this check for non-local environments.

### 5. Rollback a migration (development only)
```powershell
dotnet ef migrations remove `
  --project microservices/user/src/Bizca.Users.Infrastructure/Bizca.Users.Infrastructure.csproj `
  --startup-project microservices/user/src/Bizca.Users.Api/Bizca.Users.Api.csproj
```

## Common Rationalizations
| Rationalization | Reality |
|---|---|
| "I'll add the migration later" | EF model and database must stay in sync; a pending migration breaks startup in Development. |
| "I can run `add` from the Infrastructure project directory" | The `--startup-project` flag is required; running without it fails because the DbContext has no connection string without the API's `appsettings`. |
| "I don't need to review the generated SQL" | Auto-generated migrations sometimes produce destructive SQL (e.g. column renames as drop+add). Always review. |

## Red Flags
- `ApplicationDbContextModelSnapshot.cs` not updated after adding a migration.
- Migration references entities that are not registered in `OnModelCreating`.
- `dotnet ef` command run from a subdirectory without correct `--project` / `--startup-project` paths.

## Verification
- [ ] Migration file exists in `Context/Migrations/` with correct timestamp prefix.
- [ ] `ApplicationDbContextModelSnapshot.cs` updated.
- [ ] Migration SQL reviewed — no unintended drops.
- [ ] Applied to the local database without errors.
- [ ] Build passes with `TreatWarningsAsErrors=true`.

