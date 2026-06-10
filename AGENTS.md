# AGENTS.md

Guidance for all AI coding agents (Claude Code, Cursor, GitHub Copilot, etc.) working in this repository.

---

## Solution Overview

**Bizca** is a **.NET 10 microservices backend** built with Domain-Driven Design (DDD).
Active microservices: **Users** (`microservices/user/`), **OpenID** (`security/Bizca.OpenId.Server/`)

**Stack:** ASP.NET Core Minimal API · EF Core + PostgreSQL · .NET Aspire · Keycloak · xUnit · FluentAssertions · Reqnroll · Testcontainers · Moq · AutoFixture · Bogus

### Architecture

```
Bizca.Sdk.SharedKernel  →  Bizca.Users.Domain  →  Bizca.Users.Infrastructure  →  Bizca.Users.Api
                                                                                         ↑
                                                                          Bizca.User.IntegrationTests
```

| Project | Responsibility |
|---|---|
| `Bizca.Sdk.SharedKernel` | Base types: `Entity<TId>`, `ValueObject`, `Result<T>`, `Error`, `DomainEvent`, `IValueObject<T,TRaw>`, `IVersionedEntity` |
| `Bizca.Sdk.Abstractions` | Pipeline decorators (`ValidationDecorator`, `LoggingDecorator`), `IRequest`, `IRequestHandler` |
| `Bizca.Sdk.Api` | MinimalApi extensions, OpenAPI/Scalar configuration, OpenID middleware, endpoint mapping |
| `Bizca.Users.Domain` | Entities, Value Objects, domain enums, domain events — pure C#, no infrastructure |
| `Bizca.Users.Infrastructure` | EF Core `ApplicationDbContext`, `IEntityTypeConfiguration<T>`, repositories, options, time provider |
| `Bizca.Users.Api` | ASP.NET Core Minimal API endpoints, DI wiring (`Program.cs`), startup migration |
| `Bizca.Users.Aspire` | .NET Aspire service defaults for Users microservice |
| `Bizca.Users.UnitTests` | Unit tests — domain behaviour, Value Objects, entity factories — no DB, no HTTP |
| `Bizca.User.IntegrationTests` | Integration (Testcontainers) + functional (Reqnroll + WebApplicationFactory) |
| `Bizca.OpenId.ApiModels` | Request/Response DTOs for token, refresh, logout endpoints |
| `Bizca.OpenId.Application` | Use cases (token exchange, refresh, logout), validation, abstractions |
| `Bizca.OpenId.Infrastructure` | Keycloak HTTP client, JWKS cache, JWT validation, DI registration |
| `Bizca.OpenId.Server` | Authentication microservice entry point — endpoints, `Program.cs` |
| `Bizca.Services.AppHost` | .NET Aspire orchestration (Keycloak, PostgreSQL, OpenID Server, Users API) |

### Layer Rules

- Domain references **only** SharedKernel — no EF Core, no HTTP, no DI.
- Infrastructure references Domain + SharedKernel — never API.
- API references Infrastructure for DI wiring only.
- `Bizca.Users.UnitTests` references **Domain only** — never Infrastructure or API.
- `Bizca.User.IntegrationTests` references API (full stack via `WebApplicationFactory`).

### Unit Test Patterns

Unit tests live in `Bizca.Users.UnitTests/` — domain behaviour only, no DB/HTTP/DI.

**Mandatory category trait on every test class:**
```csharp
[Trait("Category", "Unit")]
public sealed class ChannelValueTests { }
```

**Naming pattern** — describe behaviour, never implementation:
```
[Scenario]_[Condition]_[ExpectedOutcome]

✓ AValidChannelValue_IsAccepted
✓ ABlankChannelValue_IsRejected_WithAnExplicitErrorCode
✗ Create_WithValidValue_ReturnsSuccess  // wrong - describes method
```

**What to test:**
- Value Object validation → `Result.IsSuccess` / `Result.IsFailure` + correct `ErrorType` + error code
- Entity creation → observable properties (`Active`, `Status`, `ExternalUserId`)
- Domain rules → `Result.IsFailure` (never thrown exceptions)

**Test structure:**
- Use FluentAssertions — never `Assert.Equal`
- Use `[Theory]` + `[InlineData]` for multiple inputs
- One concept per test — passing and rejection are separate tests
- Never assert on `private` fields or internal state

**Example:**
```csharp
[Theory]
[InlineData("alice@example.com")]
public void AValidChannelValue_IsAccepted(string raw)
{
    var result = ChannelValue.Create(raw);

    result.IsSuccess.Should().BeTrue();
    result.Value.Value.Should().Be(raw);
}
```

---

## Agent Resources

All agent resources live under `.github/`:

| Resource type | Location | Purpose |
|---|---|---|
| **Skills** | `.github/skills/<name>/SKILL.md` | Step-by-step workflows with entry/exit criteria — the *how* |
| **Personas** | `.github/agents/<role>.md` | Specialist roles with a fixed perspective and output format — the *who* |
| **Commands** | `.github/commands/<name>.md` | User-facing entry points that compose personas and skills — the *when* |
| **References** | `.github/references/<name>.md` | Authoritative checklists and pattern catalogs consulted inside skills |

### Core Rule

> **If a task matches a skill, invoke it. Do not implement directly.**

Skills are mandatory hops — always check the intent → skill table below before writing any code.

---

## Skills Catalog

All 28 skills are located in `.github/skills/`. Load the full `SKILL.md` with `read_file` before following a skill's workflow.

### Domain & Design

| Trigger | Skill |
|---|---|
| New aggregate root or child entity | `create-entity` |
| New typed wrapper around a primitive (Id, Email, Code…) | `create-value-object` |
| Operation or factory that can fail with a typed error | `result-error-handling` |
| Domain state change that must notify other parts of the system | `domain-event` |
| New enum must be persisted as a referential/lookup table | `enum-referential-data` |
| Designing REST endpoints, DTOs, or contracts between layers | `api-and-interface-design` |
| Idea needs to be refined before writing a spec | `idea-refine` |
| Writing a spec before coding a new feature | `spec-driven-development` |

### Infrastructure & Data

| Trigger | Skill |
|---|---|
| Mapping an entity to a DB table, columns, relationships, indexes | `efcore-entity-configuration` |
| Adding, applying, or removing an EF Core migration | `ef-migration` |
| New infrastructure service needs settings from `appsettings.json` | `options-configuration` |

### Implementation & Quality

| Trigger | Skill |
|---|---|
| Breaking work into ordered tasks with acceptance criteria | `planning-and-task-breakdown` |
| Implementing one task incrementally (RED→GREEN→REFACTOR) | `incremental-implementation` |
| Writing tests, TDD cycle, or reproducing a bug | `test-driven-development` |
| Code review before merge | `code-review-and-quality` |
| Refactoring code for clarity without changing behavior | `code-simplification` |
| API response time, EF Core N+1, async bottlenecks | `performance-optimization` |
| Build breaks, tests fail, or unexpected behavior | `debugging-and-error-recovery` |
| High-stakes decision — production, security, irreversible ops | `doubt-driven-development` |
| Using an unfamiliar framework/library — need authoritative source | `source-driven-development` |

### Security, Ops & Delivery

| Trigger | Skill |
|---|---|
| Vulnerability check, auth/authz, secrets, hardening | `security-and-hardening` |
| CI pipeline setup, quality gates, test runners in GitHub Actions | `ci-cd-and-automation` |
| Preparing for production deployment, health checks, rollback plan | `shipping-and-launch` |
| Writing or fixing a commit message | `conventional-commits` |
| Branching, merging, conflict resolution, versioning strategy | `git-workflow-and-versioning` |
| Architectural decision, ADR, CHANGELOG, public API doc | `documentation-and-adrs` |

### Agent & Session Management

| Trigger | Skill |
|---|---|
| Starting a new session or unsure which skill applies | `using-agent-skills` |
| Agent output quality degrades or context is stale | `context-engineering` |

---

## Personas (Agents)

Three specialist personas live in `.github/agents/`. Each adopts a single perspective and produces a structured report.

| Persona | Role | Best for |
|---|---|---|
| `code-reviewer` | Senior Staff Engineer | Five-axis review before merge |
| `security-auditor` | Security Engineer | Vulnerability detection, OWASP-style audit |
| `test-engineer` | QA Engineer | Test strategy, coverage analysis, Prove-It pattern |

**Rule:** personas do not invoke other personas. Composition is the job of commands or the user.
See `.github/agents/README.md` for the full decision matrix.

---

## Commands

Seven slash commands live in `.github/commands/`. Each command composes one or more personas with the appropriate skills.

| Command | What it does | Personas invoked |
|---|---|---|
| `/spec` | Writes a spec before any code — loads `spec-driven-development` | — |
| `/plan` | Breaks work into DDD-ordered tasks → `tasks/plan.md` — loads `planning-and-task-breakdown` | — |
| `/build` | RED→GREEN→REFACTOR→commit — loads `incremental-implementation` + `test-driven-development` | — |
| `/test` | TDD cycle or Prove-It bug pattern — loads `test-driven-development` | `test-engineer` |
| `/review` | Multi-axis review before merge — loads `code-review-and-quality` | `code-reviewer` |
| `/code-simplify` | Reduces complexity without changing behavior — loads `code-simplification` | — |
| `/ship` | Pre-launch gate (parallel fan-out) — loads `shipping-and-launch` | `code-reviewer` + `security-auditor` + `test-engineer` |

### `/ship` — Parallel Fan-Out

`/ship` is the only endorsed multi-persona orchestration pattern: three agents run **in parallel**, each producing an independent report, then a merge step produces a go/no-go decision.

```
                    ┌─→ code-reviewer    ─┐
/ship → fan out  ───┼─→ security-auditor ─┤→ merge → go/no-go + rollback plan
                    └─→ test-engineer    ─┘
```

A **Critical** finding from any agent is a NO-GO by default.

---

## References

Authoritative checklists and patterns consulted inside skills — do not inline their content:

| File | When to read it |
|---|---|
| `.github/references/orchestration-patterns.md` | Before adding a new command or persona that coordinates others |
| `.github/references/security-checklist.md` | Inside `security-and-hardening` skill or `security-auditor` persona |
| `.github/references/performance-checklist.md` | Inside `performance-optimization` skill |
| `.github/references/testing-patterns.md` | Inside `test-driven-development` skill or `test-engineer` persona |
| `.github/references/accessibility-checklist.md` | Inside API design reviews |

---

## Lifecycle → Command Mapping

The full feature lifecycle follows a user-driven sequential pipeline. The **user is the orchestrator** — no agent should automate the hand-off between steps.

```
DEFINE   →  /spec
PLAN     →  /plan
BUILD    →  /build
VERIFY   →  /test  (or direct test-engineer invocation)
REVIEW   →  /review
SHIP     →  /ship
```

---

## Intent → Skill Quick Reference

When the task matches one of these intents, load the skill **before** writing any code.

| User intent | Load skill |
|---|---|
| "Create a new entity / aggregate" | `create-entity` |
| "Create a new value object / typed ID" | `create-value-object` |
| "Handle a domain error / Result pattern" | `result-error-handling` |
| "Raise or handle a domain event" | `domain-event` |
| "Persist an enum in a lookup table" | `enum-referential-data` |
| "Design an endpoint / DTO / contract" | `api-and-interface-design` |
| "Map entity to database" | `efcore-entity-configuration` |
| "Add / apply / rollback a migration" | `ef-migration` |
| "Configure options from appsettings" | `options-configuration` |
| "Break a feature into tasks" | `planning-and-task-breakdown` |
| "Implement a task step by step" | `incremental-implementation` |
| "Write tests / TDD / reproduce a bug" | `test-driven-development` |
| "Review code before merging" | `code-review-and-quality` |
| "Simplify / refactor code" | `code-simplification` |
| "Fix a slow query / N+1 / async issue" | `performance-optimization` |
| "Debug a failure / broken build" | `debugging-and-error-recovery` |
| "Audit security / OWASP / CVE" | `security-and-hardening` |
| "Write a commit message" | `conventional-commits` |
| "Branching / merging / versioning" | `git-workflow-and-versioning` |
| "Write an ADR / update the CHANGELOG" | `documentation-and-adrs` |
| "Set up or fix a CI pipeline" | `ci-cd-and-automation` |
| "Prepare for production deployment" | `shipping-and-launch` |
| "Verify an uncertain decision" | `doubt-driven-development` |
| "Use a framework I'm unfamiliar with" | `source-driven-development` |
| "Refine an idea before specifying it" | `idea-refine` |

---

## Orchestration Rules

1. **Personas do not invoke other personas.** The user (or a slash command) is the orchestrator.
2. **Use endorsed patterns only** — see `.github/references/orchestration-patterns.md`:
   - Direct invocation (cheapest, the default)
   - Single-persona slash command (`/review`, `/test`, `/code-simplify`)
   - Parallel fan-out with merge (`/ship`)
   - Sequential pipeline driven by the user (`/spec` → `/plan` → `/build` → …)
   - Research isolation (built-in `Explore` subagent on Claude Code)
3. **Anti-patterns to avoid**: router persona, persona-calls-persona, sequential orchestrator that paraphrases, deep persona trees.

---

## Non-Negotiable Boundaries

### Always

- `private` constructor + `static Create` factory on every entity and value object
- `Result<T>` for domain failures — never `throw` for expected error conditions
- Error codes as `const string` in `SCREAMING_SNAKE_CASE`
- `await` all the way — never `.Result`, `.Wait()`, `async void`
- `CancellationToken` on every `async` method
- `internal sealed` on every `IEntityTypeConfiguration<T>`
- `HasDatabaseName("ix_...")` on every index
- `.AddAuditingProperties().IgnoreAuditingProperties()` on every entity configuration
- Zero warnings before committing (`TreatWarningsAsErrors = true`)

### Never

- Public or `internal` constructor on an entity or value object
- `public set` on any entity property
- `IConfiguration` injected into Domain or Application layer
- Unbounded `ToListAsync()` — always paginate
- `DbContext` mocked in tests — use Testcontainers
- `HttpResponseMessage` stored in `ScenarioContext` — deserialize immediately
- `row["column"]` in Reqnroll `DataTable` — use `CreateInstance<T>()`
- Secrets in `appsettings.json`
- String interpolation in `FromSqlRaw` / `ExecuteSqlRaw`
- Enum values starting at `0` in referential tables
- `new HttpClient()` — always use `IHttpClientFactory`
- `Version=` attributes in `.csproj` — all NuGet versions are centralized in `Directory.Packages.props`

### Ask First

- New NuGet package → add version to `Directory.Packages.props` only
- Breaking EF schema change → migration review required
- Domain event dispatch → application layer design must be agreed first

---

## Compiler Constraints

| Setting | Value |
|---|---|
| `Nullable` | `enable` — annotate all nullable references |
| `TreatWarningsAsErrors` | `true` — warnings are build failures |
| `TargetFramework` | `net10.0` |

---

## Quick Commands

```powershell
dotnet build bizca.slnx                                           # zero warnings required
dotnet test bizca.slnx --filter "Category=Unit"                   # unit tests only (no Docker)
dotnet test bizca.slnx --filter "Category!=Unit"                  # integration + functional (Docker required)
dotnet list bizca.slnx package --vulnerable --include-transitive  # CVE scan
```

### Test Categories

| Category | Project | Needs Docker |
|---|---|---|
| Unit — domain behaviour | `Bizca.Users.UnitTests` | No |
| Integration — EF Core + real PostgreSQL | `Bizca.User.IntegrationTests` | Yes (Testcontainers) |
| Functional — HTTP endpoints via Reqnroll | `Bizca.User.IntegrationTests` | Yes (Testcontainers) |

---

## Local Development

### .NET Aspire Orchestration (Recommended)

Run all services (Keycloak + PostgreSQL + OpenID Server + Users API) with a single command:

```powershell
dotnet run --project microservices/Bizca.Services.AppHost/Bizca.Services.AppHost.csproj
```

**Accesses:**
- **Aspire Dashboard**: `https://localhost:17000` or `http://localhost:15000` — view all service logs, metrics, endpoints
- **Keycloak**: `http://localhost:8080` — admin/admin
- **OpenID Server, Users API**: ports shown in Aspire Dashboard (dynamic)

**Prerequisites:**
- Docker Desktop running
- .NET Aspire workload: `dotnet workload install aspire`

**Services orchestrated:**
- Keycloak (authentication server, versioned at 25.0.6)
- PostgreSQL (Users database with PgWeb UI)
- Bizca.OpenId.Server (token exchange, JWT validation, Keycloak integration)
- Bizca.Users.Api (user management)

**Data persistence:**
- PostgreSQL: anonymous volume (cleared on stop) — prevents corruption in dev
- Keycloak: bind mount to `./keycloak-data/` — realm configuration persists

### EF Core Migrations

**Create a migration** (from `microservices/user/src/Bizca.Users.Api/`):
```powershell
dotnet ef migrations add {MigrationName} `
  --project ..\Bizca.Users.Infrastructure\Bizca.Users.Infrastructure.csproj `
  --startup-project .\Bizca.Users.Api.csproj `
  --context ApplicationDbContext
```

**Apply migrations** (automatic in Aspire, manual command):
```powershell
dotnet ef database update `
  --project ..\Bizca.Users.Infrastructure\Bizca.Users.Infrastructure.csproj `
  --startup-project .\Bizca.Users.Api.csproj `
  --context ApplicationDbContext
```

**Best practices:**
- Use PascalCase names: `AddUserEmailColumn`, `CreateOrdersTable`
- Never remove migrations deployed to shared environments
- Coordinate with team before rolling back shared database migrations
- Review generated SQL: `dotnet ef migrations script {from} {to} --output migration.sql`

---

## Anti-Rationalization

The following thoughts are traps — ignore them:

- *"This is too small for a skill"*
- *"I can just quickly implement this without loading the skill"*
- *"I'll gather context first, then decide if a skill applies"*

Correct behavior: **check the intent → skill table first, every time.**
