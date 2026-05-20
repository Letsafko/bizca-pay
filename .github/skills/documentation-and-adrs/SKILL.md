---
name: documentation-and-adrs
description: Records decisions and documentation. Use when making architectural decisions, changing public APIs, shipping features, or when you need to record context that future engineers and agents will need to understand the codebase.
---

# Documentation and ADRs

## Overview

Document decisions, not just code. The most valuable documentation captures the *why* — the context, constraints, and trade-offs that led to a decision. Code shows *what* was built; documentation explains *why it was built this way* and *what alternatives were considered*. This context is essential for future humans and agents working in the codebase.

## When to Use

- Making a significant architectural decision
- Choosing between competing approaches
- Adding or changing a public API
- Shipping a feature that changes user-facing behavior
- Onboarding new team members (or agents) to the project
- When you find yourself explaining the same thing repeatedly

**When NOT to use:** Don't document obvious code. Don't add comments that restate what the code already says. Don't write docs for throwaway prototypes.

## Architecture Decision Records (ADRs)

ADRs capture the reasoning behind significant technical decisions. They're the highest-value documentation you can write.

### When to Write an ADR

- Choosing a framework, library, or major dependency
- Designing a data model or database schema
- Selecting an authentication strategy
- Deciding on an API architecture (REST vs. GraphQL vs. tRPC)
- Choosing between build tools, hosting platforms, or infrastructure
- Any decision that would be expensive to reverse

### ADR Template

Store ADRs in `docs/decisions/` with sequential numbering:

```markdown
# ADR-001: Use EF Core + Npgsql for data persistence

## Status
Accepted

## Date
2025-01-15

## Context
We need a data persistence layer for the User microservice. Key requirements:
- Relational data model (User, Address, UserChannel with relationships)
- ACID transactions for user state changes
- PostgreSQL as the target database (chosen for its JSON support and ecosystem)
- .NET-native tooling for migrations and type safety

## Decision
Use Entity Framework Core 10 with Npgsql provider and `EFCore.NamingConventions` for camelCase column names.

## Alternatives Considered

### Dapper + hand-written SQL
- Pros: Full SQL control, minimal overhead
- Cons: Manual migration management, no type-safe query composition
- Rejected: EF Core migrations reduce schema drift risk; DDD aggregates map cleanly to EF configurations

### Marten (PostgreSQL document DB via .NET)
- Pros: Document model flexibility, powerful querying
- Cons: Document store doesn't fit the relational shape of User + Address + Channels
- Rejected: Our data is inherently relational; aggregate boundaries map better to EF entity configurations

## Consequences
- EF Core migrations are the single source of truth for schema changes
- `UseCamelCaseNamingConvention()` is applied globally — all column names in camelCase; always provide `HasDatabaseName("ix_...")` for explicit index names to avoid the auto-generated `iX_` casing issue
- `UseQueryTrackingBehavior.NoTracking` is set globally — use `AsTracking()` only when explicit tracking is needed
```

### ADR Lifecycle

```
PROPOSED → ACCEPTED → (SUPERSEDED or DEPRECATED)
```

- **Don't delete old ADRs.** They capture historical context.
- When a decision changes, write a new ADR that references and supersedes the old one.

## Inline Documentation

### When to Comment

Comment the *why*, not the *what*:

```csharp
// BAD: Restates the code
// Increment counter by 1
counter += 1;

// GOOD: Explains non-obvious intent
// Rate limit uses a sliding window — reset counter at window boundary,
// not on a fixed schedule, to prevent burst attacks at window edges
if (now - _windowStart > WindowSizeMs)
{
    _counter = 0;
    _windowStart = now;
}
```

### When NOT to Comment

```csharp
// Don't comment self-explanatory code
var isValidAge = person.Age is >= 18 and <= 120;

// Don't leave TODO comments for things you should just do now
// TODO: add error handling  ← Just add it

// Don't leave commented-out code
// var oldImplementation = ...; ← Delete it, git has history
```

### Document Known Gotchas

```csharp
/// <summary>
/// IMPORTANT: UseCamelCaseNamingConvention() generates "iX_" prefixes for auto-named indexes.
/// Always provide an explicit HasDatabaseName("ix_...") to avoid casing inconsistency
/// in PostgreSQL index names. See ADR-001 for context.
/// </summary>
builder.HasIndex(static e => e.Status)
    .HasDatabaseName("ix_user_statusId");
```

## API Documentation

For public REST APIs, prefer XML doc comments for Swagger/OpenAPI auto-generation:

```csharp
/// <summary>Creates a new user.</summary>
/// <param name="request">User creation data (firstName and lastName required)</param>
/// <returns>The created user with server-generated ExternalUserId</returns>
/// <response code="201">User created successfully</response>
/// <response code="422">Validation error — invalid input</response>
[ProducesResponseType<UserResponse>(StatusCodes.Status201Created)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
public static async Task<IResult> CreateUser(
    [FromBody] CreateUserRequest request,
    IUserService service)
{
    var result = await service.CreateAsync(request);
    return result.IsSuccess
        ? Results.Created($"/users/{result.Value.ExternalUserId}", result.Value)
        : result.Error.ToHttpResult();
}
```

Enable Swagger in development:

```csharp
// Program.cs
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

app.UseSwagger();
app.UseSwaggerUI(); // available at /swagger in development
```

## README Structure

Every project should have a README that covers:

```markdown
# Project: Bizca — User Microservice

One-paragraph description of what this microservice does.

## Quick Start
1. Clone the repo
2. Run `bash setup-husky.sh` (one-time git hooks setup)
3. Copy `appsettings.Local.json.example` to `appsettings.Local.json` and fill in values
4. Launch via .NET Aspire: open `Bizca.Users.AppHost` in Rider or run `dotnet run --project microservices/user/src/Bizca.Users.AppHost`

## Commands
| Command | Description |
|---|---|
| `dotnet build` | Build all projects |
| `dotnet test` | Run all tests |
| `dotnet ef migrations add <Name> --project ...Infrastructure --startup-project ...Api` | Create a new EF Core migration |
| `dotnet ef database update --project ...Infrastructure --startup-project ...Api` | Apply pending migrations |

## Architecture
Brief overview: DDD layered architecture (Domain → Infrastructure → API).
See `docs/decisions/` for ADRs on significant choices (EF Core, Aspire, etc.).

## Contributing
Follow the Conventional Commits spec enforced by `commitlint.config.js` + husky.
PR checklist: `dotnet test` passes, `dotnet build --configuration Release` is warning-free.
```

## Changelog Maintenance

For shipped features:

```markdown
# Changelog

## [1.2.0] - 2025-01-20
### Added
- Task sharing: users can share tasks with team members (#123)
- Email notifications for task assignments (#124)

### Fixed
- Duplicate tasks appearing when rapidly clicking create button (#125)

### Changed
- Task list now loads 50 items per page (was 20) for better UX (#126)
```

## Documentation for Agents

Special consideration for AI agent context:

- **CLAUDE.md / rules files** — Document project conventions so agents follow them
- **Spec files** — Keep specs updated so agents build the right thing
- **ADRs** — Help agents understand why past decisions were made (prevents re-deciding)
- **Inline gotchas** — Prevent agents from falling into known traps

## Common Rationalizations

| Rationalization | Reality |
|---|---|
| "The code is self-documenting" | Code shows what. It doesn't show why, what alternatives were rejected, or what constraints apply. |
| "We'll write docs when the API stabilizes" | APIs stabilize faster when you document them. The doc is the first test of the design. |
| "Nobody reads docs" | Agents do. Future engineers do. Your 3-months-later self does. |
| "ADRs are overhead" | A 10-minute ADR prevents a 2-hour debate about the same decision six months later. |
| "Comments get outdated" | Comments on *why* are stable. Comments on *what* get outdated — that's why you only write the former. |

## Red Flags

- Architectural decisions with no written rationale
- Public APIs with no documentation or types
- README that doesn't explain how to run the project
- Commented-out code instead of deletion
- TODO comments that have been there for weeks
- No ADRs in a project with significant architectural choices
- Documentation that restates the code instead of explaining intent

## Verification

After documenting:

- [ ] ADRs exist for all significant architectural decisions
- [ ] README covers quick start, commands, and architecture overview
- [ ] API functions have parameter and return type documentation
- [ ] Known gotchas are documented inline where they matter
- [ ] No commented-out code remains
- [ ] Rules files (CLAUDE.md etc.) are current and accurate
