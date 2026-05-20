# Bizca — GitHub Copilot Instructions

## Project Overview

Bizca is a **.NET 10 microservices backend** built with DDD (Domain-Driven Design).
Active microservice: **Users** (`microservices/user/`).

**Stack:** ASP.NET Core Minimal API · EF Core + PostgreSQL · .NET Aspire · xUnit · FluentAssertions · Reqnroll · Testcontainers · Moq · AutoFixture · Bogus

---

## Architecture

```
Bizca.Sdk.SharedKernel  →  Bizca.Users.Domain  →  Bizca.Users.Infrastructure  →  Bizca.Users.Api
                                                                                         ↑
                                                                         Bizca.User.IntegrationTests
```

| Project | Responsibility |
|---|---|
| `Bizca.Sdk.SharedKernel` | Base types: `Entity<TId>`, `ValueObject`, `Result<T>`, `Error`, `DomainEvent`, `IValueObject<T,TRaw>`, `IVersionedEntity` |
| `Bizca.Users.Domain` | Entities, Value Objects, domain enums, domain events — pure C#, no infrastructure |
| `Bizca.Users.Infrastructure` | EF Core `ApplicationDbContext`, `IEntityTypeConfiguration<T>`, repositories, options, time provider |
| `Bizca.Users.Api` | ASP.NET Core Minimal API endpoints, DI wiring (`Program.cs`), startup migration |
| `Bizca.Users.AppHost` | .NET Aspire orchestration for local development (Postgres + API) |
| `Bizca.Users.UnitTests` | Unit tests only — domain behaviour, Value Objects, entity factories — no DB, no HTTP |
| `Bizca.User.IntegrationTests` | Integration (Testcontainers) · functional (Reqnroll + WebApplicationFactory) |

**Layer rules:**
- Domain references **only** SharedKernel — no EF Core, no HTTP, no DI.
- Infrastructure references Domain + SharedKernel — never API.
- API references Infrastructure for DI wiring only.
- `Bizca.Users.UnitTests` references **Domain only** — never Infrastructure or API.
- `Bizca.User.IntegrationTests` references API (full stack via `WebApplicationFactory`).

**Test categories:**

| Category | Project | Filter | Needs Docker |
|---|---|---|---|
| Unit — domain behaviour | `Bizca.Users.UnitTests` | `--filter "Category=Unit"` | No |
| Integration — EF Core + real PostgreSQL | `Bizca.User.IntegrationTests` | `--filter "Category!=Unit"` | Yes (Testcontainers) |
| Functional — HTTP endpoints via Reqnroll | `Bizca.User.IntegrationTests` | `--filter "Category!=Unit"` | Yes (Testcontainers) |

- All NuGet versions centralized in `Directory.Packages.props` — **never add `Version=` in `.csproj`**.

---

## Unit Tests

Unit tests live in their **own dedicated project** — `Bizca.Users.UnitTests` — which references **Domain only**. No DB, no HTTP, no DI container.

### Location

```
microservices/user/test/Bizca.Users.UnitTests/
└── Users/
    ├── ValueObjects/
    │   ├── ChannelValueTests.cs
    │   ├── UserIdTests.cs
    │   └── CountryCodeTests.cs
    └── UserTests.cs
```

### Category trait — mandatory on every unit test class

```csharp
[Trait("Category", "Unit")]
public sealed class ChannelValueTests { }
```

### Naming pattern — describe a behaviour, never a method

```
[Scenario]_[Condition]_[ExpectedOutcome]
```

```csharp
// ✓ describes behaviour
public void AValidChannelValue_IsAccepted(string raw) { }
public void ABlankChannelValue_IsRejected_WithAnExplicitErrorCode(string raw) { }
public void ANewUser_IsInactiveWithDraftStatus_UntilConfirmed() { }

// ✗ describes implementation
public void Create_WithValidValue_ReturnsSuccess(string raw) { }
```

### What to test at unit level

**Test observable behaviour — never internal implementation details.**

| Behaviour to verify | Observable outcome |
|---|---|
| A valid channel value is accepted | `Result.IsSuccess` + the wrapped value is preserved |
| A blank channel value is rejected | `Result.IsFailure` + correct `ErrorType` + correct error code |
| A newly created user is inactive by default | `Active == false`, `Status == Draft` |
| A newly created user receives a unique external identity | `ExternalUserId` is set and non-default |
| A domain rule violation is returned, not thrown | `Result.IsFailure` — no exception propagates to the caller |

The **method name under test is irrelevant** — what matters is the scenario and the expected outcome.

### Examples — based on real domain types

```csharp
using Bizca.Sdk.SharedKernel;
using Bizca.Users.Domain.Users.ValueObjects;
using FluentAssertions;
using Xunit;

namespace Bizca.Users.UnitTests.Users.ValueObjects;

[Trait("Category", "Unit")]
public sealed class ChannelValueTests
{
    [Theory]
    [InlineData("alice@example.com")]
    [InlineData("+33612345678")]
    public void AValidChannelValue_IsAccepted(string raw)
    {
        var result = ChannelValue.Create(raw);

        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(raw);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void ABlankChannelValue_IsRejected_WithAnExplicitErrorCode(string raw)
    {
        var result = ChannelValue.Create(raw);

        result.IsSuccess.Should().BeFalse();
        result.Error.Type.Should().Be(ErrorType.Problem);
        result.Error.Code.Should().Be("INVALID_CHANNEL_VALUE");
    }
}
```

```csharp
using Bizca.Users.Domain.Users;
using Bizca.Users.Domain.Users.Models;
using FluentAssertions;
using Xunit;

namespace Bizca.Users.UnitTests.Users;

[Trait("Category", "Unit")]
public sealed class UserTests
{
    [Fact]
    public void ANewUser_IsInactiveWithDraftStatus_UntilConfirmed()
    {
        var profile = new UserProfile(/* ... */);
        var now = DateTimeOffset.UtcNow;

        var user = User.Create(profile, passwordHash: null, securityStamp: null, now);

        user.Active.Should().BeFalse();
        user.Status.Should().Be(Status.Draft);
        user.ExternalUserId.Should().NotBeNull();
        user.CreatedDatetime.Should().BeCloseTo(now, TimeSpan.FromSeconds(1));
    }
}
```

### Rules

- **Test the behaviour, not the method** — the test name describes a business scenario, not a method call
- No `DbContext`, no `HttpClient`, no DI container — pure in-memory
- Always use FluentAssertions — never `Assert.Equal`
- Use `[Theory]` + `[InlineData]` to cover multiple inputs for the same behaviour
- One concept per test — a passing scenario and a rejection scenario are two different behaviours
- Never assert on `private` fields, internal state, or call order — only on `Result<T>`, returned values, and observable properties

---

## Compiler Constraints (non-negotiable)

| Setting | Value |
|---|---|
| `Nullable` | `enable` — annotate all nullable references |
| `TreatWarningsAsErrors` | `true` — warnings are build failures |
| `TargetFramework` | `net10.0` |

---

## Skill Activation Rules

When a task matches one of the triggers below, **load the corresponding skill before writing any code**.

### Domain & Design

| Trigger | Load skill |
|---|---|
| New aggregate root or child entity | `create-entity` |
| New typed wrapper around a primitive (Id, Email, etc.) | `create-value-object` |
| Operation or factory that can fail with a typed error | `result-error-handling` |
| Domain state change that must notify other parts of the system | `domain-event` |
| New enum must be persisted as a referential/lookup table | `enum-referential-data` |
| Designing REST endpoints, DTOs, or contracts between layers | `api-and-interface-design` |
| Idea needs to be refined before writing a spec | `idea-refine` |
| Writing a spec before coding a new feature | `spec-driven-development` |

### Infrastructure & Data

| Trigger | Load skill |
|---|---|
| Mapping an entity to a DB table, columns, relationships, indexes | `efcore-entity-configuration` |
| Adding, applying, or removing an EF Core migration | `ef-migration` |
| New infrastructure service needs settings from `appsettings.json` | `options-configuration` |

### Implementation & Quality

| Trigger | Load skill |
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

| Trigger | Load skill |
|---|---|
| Vulnerability check, auth/authz, secrets, hardening | `security-and-hardening` |
| CI pipeline setup, quality gates, test runners in GitHub Actions | `ci-cd-and-automation` |
| Preparing for production deployment, health checks, rollback plan | `shipping-and-launch` |
| Writing or fixing a commit message | `conventional-commits` |
| Branching, merging, conflict resolution, versioning strategy | `git-workflow-and-versioning` |
| Architectural decision, ADR, CHANGELOG, public API doc | `documentation-and-adrs` |

### Agent & Session Management

| Trigger | Load skill |
|---|---|
| Starting a new session or unsure which skill applies | `using-agent-skills` |
| Agent output quality degrades or context is stale | `context-engineering` |

---

## Agent Activation Rules

When a slash command or user intent matches below, **invoke the corresponding agent**.

| Trigger / Command | Invoke agent(s) |
|---|---|
| `/review` — review current changes | `code-reviewer` |
| `/test` — TDD workflow or Prove-It bug | `test-engineer` |
| `/ship` — pre-launch gate | `code-reviewer` + `security-auditor` + `test-engineer` (parallel fan-out) |
| Security-focused pass, OWASP audit, CVE scan | `security-auditor` |
| Coverage gap analysis, test quality assessment | `test-engineer` |

> `/ship` is a **parallel fan-out**: spawn all three agents in a single turn, merge their reports, then produce a go/no-go with rollback plan. A Critical finding from any agent is a NO-GO by default.

---

## Slash Commands

| Command | What it does |
|---|---|
| `/spec` | Loads `spec-driven-development` — writes a spec before any code |
| `/plan` | Loads `planning-and-task-breakdown` — DDD-ordered tasks → `tasks/plan.md` |
| `/build` | Loads `incremental-implementation` + `test-driven-development` — RED→GREEN→REFACTOR→commit |
| `/test` | Loads `test-driven-development` — TDD or Prove-It pattern |
| `/review` | Loads `code-review-and-quality`, invokes `code-reviewer` agent |
| `/ship` | Loads `shipping-and-launch`, invokes `code-reviewer` + `security-auditor` + `test-engineer` |
| `/code-simplify` | Loads `code-simplification` — reduces complexity without changing behavior |

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
- Zero warnings before committing

### Never
- Public or `internal` constructor on an entity or value object
- `public set` on any entity property
- `IConfiguration` injected into Domain or Application layer
- Unbounded `ToListAsync()` — always paginate
- `DbContext` mocked in tests — use Testcontainers
- `HttpResponseMessage` stored in `ScenarioContext` — deserialize immediately
- `row["column"]` on Reqnroll `DataTable` — use `CreateInstance<T>()`
- Secrets in `appsettings.json`
- String interpolation in `FromSqlRaw` / `ExecuteSqlRaw`
- Enum values starting at `0` in referential tables
- `new HttpClient()` — always use `IHttpClientFactory`

### Ask first
- New NuGet package → add version to `Directory.Packages.props` only
- Breaking EF schema change → migration review required
- Domain event dispatch → application layer design must be agreed first

---

## Quick Commands Reference

```powershell
dotnet build bizca.slnx                                          # zero warnings required
dotnet test bizca.slnx --filter "Category=Unit"                  # unit only
dotnet test bizca.slnx --filter "Category!=Unit"                 # integration + functional
dotnet list bizca.slnx package --vulnerable --include-transitive # CVE scan
```
