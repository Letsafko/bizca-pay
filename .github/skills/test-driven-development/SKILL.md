---
name: test-driven-development
description: Drives development with tests in C# / .NET. Use when implementing any logic, fixing any bug, or changing any behavior. Use when you need to prove that code works, when a bug report arrives, or when you're about to modify existing functionality.
---

# Test-Driven Development (.NET)

## Overview

Write a failing test before writing the code that makes it pass. For bug fixes, reproduce the bug with a test before attempting a fix. Tests are proof — "seems right" is not done. A codebase with good tests is an AI agent's superpower; a codebase without tests is a liability.

## When to Use

- Implementing any new logic or behavior
- Fixing any bug (the Prove-It Pattern)
- Modifying existing functionality
- Adding edge case handling
- Any change that could break existing behavior

**When NOT to use:** Pure configuration changes, documentation updates, or static content changes with no behavioral impact.

## The TDD Cycle

```
    RED                GREEN              REFACTOR
 Write a test    Write minimal code    Clean up the
 that fails  ──→  to make it pass  ──→  implementation  ──→  (repeat)
      │                  │                    │
      ▼                  ▼                    ▼
   Test FAILS        Test PASSES         Tests still PASS
```

### Step 1: RED — Write a Failing Test

Write the test first. It must fail. A test that passes immediately proves nothing.

```csharp
// RED: This test fails because the factory doesn't exist yet
[Fact]
public void Create_WithValidEmail_ReturnsSuccess()
{
    var result = Email.Create("user@example.com");

    result.IsSuccess.Should().BeTrue();
    result.Value.Value.Should().Be("user@example.com");
}
```

### Step 2: GREEN — Make It Pass

Write the minimum code to make the test pass. Don't over-engineer.

### Step 3: REFACTOR — Clean Up

With tests green, improve the code without changing behavior. Run tests after every refactor step.

## The Prove-It Pattern (Bug Fixes)

When a bug is reported, **do not start by trying to fix it.** Start by writing a test that reproduces it.

```
Bug report arrives
       │
       ▼
  Write a test that demonstrates the bug
       │
       ▼
  Test FAILS (confirming the bug exists)
       │
       ▼
  Implement the fix
       │
       ▼
  Test PASSES (proving the fix works)
       │
       ▼
  Run full test suite — no regressions
```

## Test Strategy for .NET Backend

There is no E2E layer for backend developers. The three test levels are:

```
          ╱╲
         ╱  \           Functional Tests (~15%)
        ╱    ╲          Reqnroll BDD — business scenarios in Gherkin
       ╱──────╲
      ╱        ╲        Integration Tests (~15%)
     ╱          ╲       Real DB via Testcontainers, full HTTP stack
    ╱────────────╲
   ╱              ╲     Unit Tests (~70%)
  ╱                ╲    Pure domain logic, isolated, milliseconds each
 ╱──────────────────╲
```

### Unit Tests — `*.UnitTests`

Target: **domain logic in isolation** — value objects, entities, domain services, result chains.

- No database, no HTTP, no filesystem
- No mocks for domain collaborators — compose real objects
- Run in milliseconds; the entire suite should complete in seconds
- Use mocks only at infrastructure boundaries you cannot instantiate (external HTTP clients, email senders)

### Integration Tests — `*.IntegrationTests`

Target: **the full vertical slice** — HTTP request → command handler → repository → PostgreSQL → response.

- Spin up a real PostgreSQL container via **Testcontainers**
- Schema migrated once on container start; tables truncated between tests
- Assert on HTTP response shape, status code, **and** database state
- Non-Gherkin tests use `[Fact]` / `[Theory]` directly against the HTTP client

### Functional Tests — `*.IntegrationTests` (Reqnroll)

Target: **business scenarios expressed in domain language** — the living specification a stakeholder can read.

- Written as Gherkin `.feature` files (`Given / When / Then`)
- Step definitions call the same `HttpClient` and `DbContext` used by integration tests
- Live in the **same project** as integration tests — they share the Testcontainers fixture
- One `.feature` file per aggregate or use-case group
- Scenarios act as acceptance criteria: if the feature file passes, the use case is delivered

```
Decision guide:
─────────────────────────────────────────────
Is it pure domain logic (value objects, entities, Result chains)?
  → Unit test

Does it cross a boundary (HTTP, database, EF Core)?
  → Integration test with Testcontainers

Is it a business scenario described in domain language?
  → Functional test with Reqnroll (.feature file)
─────────────────────────────────────────────
```

### Project Naming Convention

| Purpose | Project name |
|---|---|
| Unit tests | `Bizca.{Service}.UnitTests` |
| Integration + Functional tests | `Bizca.{Service}.IntegrationTests` |

---

## Reqnroll — Feature File Anatomy

### Feature File (`.feature`)

Feature files are written in **Gherkin** and live under `Features/` inside `*.IntegrationTests`.

```gherkin
# Features/Users/CreateUser.feature
Feature: Create user

  Scenario: Successfully creating a user with valid inputs
    Given no user exists with email "john.doe@example.com"
    When I send a POST request to "/users" with:
      | firstName | lastName | email                  |
      | John      | Doe      | john.doe@example.com   |
    Then the response status should be 201
    And the response should contain a user id
    And a user with email "john.doe@example.com" should be persisted in the database

  Scenario: Rejecting a duplicate email
    Given a user already exists with email "existing@example.com"
    When I send a POST request to "/users" with:
      | firstName | lastName | email                    |
      | Jane      | Smith    | existing@example.com     |
    Then the response status should be 409
    And no additional user should be created
```

**Rules:**
- One `Feature:` per file, mapping to one use case or aggregate operation
- Each `Scenario:` is independent — state set up in `Given`, action in `When`, assertions in `Then`
- Use `Background:` for setup shared across all scenarios in a file (e.g., seeding a prerequisite user)
- Prefer concrete values over abstract placeholders — scenarios should read like examples, not templates

### Step Definitions

Step definitions wire Gherkin steps to C# code. They live next to the feature files under `StepDefinitions/`.

State that must be shared **across step definition classes within the same scenario** is stored in `ScenarioContext`. State that must survive **across all scenarios in a feature** is stored in `FeatureContext`. Never use instance fields for cross-step state — Reqnroll may split steps across multiple binding classes.

| Context | Scope | Typical use |
|---|---|---|
| `ScenarioContext` | One scenario | HTTP status code (`int`), deserialized response DTO, created resource ID |
| `FeatureContext` | All scenarios in the feature | Seeded reference data, shared auth token |

```csharp
// StepDefinitions/Users/CreateUserSteps.cs
[Binding]
public sealed class CreateUserSteps
{
    private const string StatusCodeKey = "status_code";
    private const string ResponseDtoKey = "response_dto";

    private readonly HttpClient _client;
    private readonly ApplicationDbContext _db;
    private readonly ScenarioContext _scenario;

    public CreateUserSteps(IntegrationTestFixture fixture, ScenarioContext scenario)
    {
        _client   = fixture.CreateClient();
        _db       = fixture.CreateDbContext();
        _scenario = scenario;
    }

    [Given("no user exists with email {string}")]
    public async Task GivenNoUserWithEmail(string email)
    {
        var count = await _db.Set<User>().CountAsync();
        count.Should().Be(0, "precondition: database must be empty");
    }

    [When("I send a POST request to {string} with:")]
    public async Task WhenISendAPostRequestTo(string path, DataTable table)
    {
        var body = table.CreateInstance<CreateUserRequest>();

        var response = await _client.PostAsJsonAsync(path, body);

		var dto = await response.Content.ReadFromJsonAsync<UserResponse>();
		_scenario.Set(dto, ResponseDtoKey);
    }

    [Then("the response status should be {int}")]
    public void ThenStatusIs(int expectedStatus)
    {
        _scenario.Get<int>(StatusCodeKey).Should().Be(expectedStatus);
    }

    [Then("the response should contain a user id")]
    public void ThenResponseContainsUserId()
    {
        var dto = _scenario.Get<UserResponse>(ResponseDtoKey)!;
        dto.Should().NotBeNull();
        dto.Id.Should().NotBeEmpty();
    }

    [Then("a user with email {string} should be persisted in the database")]
    public async Task ThenUserPersistedWithEmail(string email)
    {
        var user = await _db.Users.SingleOrDefaultAsync(u => u.Email == email);
        user.Should().NotBeNull();
    }
}
```

**Rules:**
- **Never store `HttpResponseMessage` in `ScenarioContext`** — it is a disposable; deserialize it immediately and store the typed DTO
- Store the status code separately (`int`) so `Then` steps can assert on it without re-reading the response
- Deserialize success responses into the domain DTO; deserialize error responses into `ProblemDetails`
- Use `table.CreateInstance<T>()` to deserialize a single-row table into a typed object — never read columns via `row["columnName"]`
- Use `table.CreateSet<T>()` when the table represents a collection of rows
- The type `T` must have property names that match the Gherkin table headers (case-insensitive)
- Use `ScenarioContext.Set<T>(value, key)` / `ScenarioContext.Get<T>(key)` — always with a named key constant to avoid magic strings
- Never store cross-step state in a private instance field — step class instances are not guaranteed to be shared
- `FeatureContext` is injected the same way; use it only for data that truly spans scenarios (e.g., a shared read-only seed record)

### Fixture Sharing (Testcontainers + Reqnroll)

Reqnroll integrates with xUnit via a `[CollectionDefinition]` — the same fixture used by `[Fact]` tests.

```csharp
// IntegrationTestFixture.cs  (already used by [Fact] integration tests)
[CollectionDefinition(nameof(IntegrationTestCollection))]
public sealed class IntegrationTestCollection : ICollectionFixture<IntegrationTestFixture> { }

// Reqnroll hooks into the same collection:
[assembly: CollectionDefinition(nameof(IntegrationTestCollection))]
```

In `reqnroll.json`, bind the Reqnroll test framework to xUnit and point it at the shared fixture:

```json
{
  "bindingCulture": { "language": "en-US" },
  "testThreadCount": 1
}
```

**Key rules:**
- `testThreadCount: 1` prevents concurrent writes to the shared container
- **A `[BeforeScenario]` hook that truncates tables is still required.** Testcontainers manages the *container* lifecycle (start/stop), not the *data* lifecycle. The container starts once and persists for the whole test run — data written by scenario A is still present when scenario B starts. Truncation between scenarios is the only way to guarantee isolation.
- The `HttpClient` is recreated per scenario — do not share response state between steps across scenarios

### Folder Structure

```
Bizca.Users.IntegrationTests/
├── Features/
│   └── Users/
│       ├── CreateUser.feature
│       └── GetUser.feature
├── StepDefinitions/
│   └── Users/
│       ├── CreateUserSteps.cs
│       └── SharedSteps.cs          ← reusable steps (auth, common assertions)
├── Hooks/
│   └── DatabaseResetHook.cs        ← [BeforeScenario] truncate tables
├── IntegrationTestFixture.cs       ← Testcontainers + WebApplicationFactory
└── reqnroll.json
```

## Writing Good Tests

### Test State, Not Interactions

Assert on the *outcome* of an operation, not on which methods were called internally. Interaction-based tests break when you refactor, even if the behavior is unchanged.

```csharp
// Good: tests what the code does (state-based)
[Fact]
public void Create_WithNegativeValue_ReturnsFailure()
{
    var result = UserId.Create(-1);

    result.IsSuccess.Should().BeFalse();
    result.Error.Code.Should().Be("INVALID_USER_ID");
}

// Bad: tests how the code works internally
mockRepository.Verify(r => r.FindAsync(It.IsAny<int>()), Times.Once);
```

### DAMP Over DRY in Tests

In production code, DRY is usually right. In tests, **DAMP (Descriptive And Meaningful Phrases)** is better. Each test should tell a complete story without requiring the reader to trace through shared helpers.

Duplication in tests is acceptable when it makes each test independently understandable. Extract helpers only when they reduce noise, not just repetition.

### Prefer Real Implementations Over Mocks

```
Preference order (most to least preferred):
1. Real implementation  → Highest confidence, catches real bugs
2. Fake                 → In-memory substitute (e.g., in-memory IDateTimeProvider)
3. Stub                 → Returns canned responses
4. Mock (interaction)   → Verifies calls — use sparingly, only at external boundaries
```

Use mocks only when the real implementation is non-deterministic (clocks, random), has uncontrollable side effects (SMTP, SMS), or requires network access not provided by Testcontainers.

Over-mocking creates tests that pass while production fails.

### Arrange-Act-Assert Pattern

Every test follows the same three-phase structure:

```csharp
[Fact]
public void Create_WithEmptyFirstName_ReturnsValidationError()
{
    // Arrange
    const string emptyName = "";

    // Act
    var result = FirstName.Create(emptyName);

    // Assert
    result.IsSuccess.Should().BeFalse();
    result.Error.Type.Should().Be(ErrorType.Problem);
}
```

### One Assertion Per Concept

```csharp
// Good: each test verifies one behavior
[Fact] public void Create_WithEmptyValue_ReturnsFailure() { ... }
[Fact] public void Create_WithValidValue_ReturnsSuccess() { ... }
[Fact] public void Create_WithValueExceedingMaxLength_ReturnsFailure() { ... }

// Also good: multiple assertions on the SAME concept
[Fact]
public void Create_WithValidEmail_ReturnsCorrectValue()
{
    var result = Email.Create("user@example.com");

    // These three assertions all describe the same concept — the success state
    result.IsSuccess.Should().BeTrue();
    result.Value.Should().NotBeNull();
    result.Value.Value.Should().Be("user@example.com");
}
```

### Name Tests Descriptively

Pattern: `{Method}_{Condition}_{ExpectedOutcome}`

```csharp
// Good — reads like a specification
public class EmailTests
{
    [Fact] public void Create_WithValidEmail_ReturnsSuccess() { ... }
    [Fact] public void Create_WithEmptyString_ReturnsFailure() { ... }
    [Fact] public void Create_WithMissingAtSign_ReturnsFailure() { ... }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithNullOrWhitespace_ReturnsFailure(string? value) { ... }
}

// Bad
public class EmailTests
{
    [Fact] public void Test1() { ... }
    [Fact] public void EmailWorks() { ... }
    [Fact] public void CheckEmail() { ... }
}
```

### Use `[Theory]` + `[InlineData]` for Boundary Cases

Boundary conditions should be data-driven, not copy-pasted:

```csharp
[Theory]
[InlineData(0)]
[InlineData(-1)]
[InlineData(int.MinValue)]
public void Create_WithNonPositiveValue_ReturnsFailure(int value)
{
    UserId.Create(value).IsSuccess.Should().BeFalse();
}
```

## Testcontainers — Integration Test Setup

Integration tests (both `[Fact]` and Reqnroll scenarios) spin up a **single real PostgreSQL container** per test collection. The container lifecycle is:

- **Container starts once** per test collection (shared fixture via `ICollectionFixture<T>`)
- **Schema migrated once** on container start via EF Core migrations
- **Database cleaned between each test** — Testcontainers manages the container, not the data; rows written by one test persist until explicitly removed. A `[BeforeScenario]` hook (Reqnroll) or `IAsyncLifetime.InitializeAsync` override (`[Fact]` tests) must truncate tables before each test.

Key principle: both integration and functional tests assert on **HTTP response + database state** together. A test that only checks HTTP 201 without verifying the row was persisted is incomplete.

```
[Fact] or [Scenario]
  → HTTP POST via WebApplicationFactory + HttpClient
  → Assert HTTP status + response body
  → Query DB via ApplicationDbContext
  → Assert persisted state matches expected
```

Reqnroll feature files describe the scenario in business language. The step definitions call the same HTTP client and DB context — no duplication of infrastructure setup.

## Test Anti-Patterns to Avoid

| Anti-Pattern | Problem | Fix |
|---|---|---|
| Testing implementation details | Tests break on refactor even when behavior is correct | Assert on inputs and outputs, not internal calls |
| Flaky tests (timing, non-deterministic data) | Erode trust in the suite | Use `IDateTimeProvider` stub; use fixed test data |
| Mocking the database | Tests pass but EF queries never verified | Use Testcontainers for anything touching EF Core |
| No test isolation | Tests pass individually, fail together | Each test cleans its own DB state (truncate via `[BeforeScenario]` hook) |
| Over-mocking | Tests pass while production breaks | Use real domain objects; mock only external I/O |
| Skipping tests to make the suite pass | Hides failures | Fix the test or delete; never `[Skip]` without a tracked issue |
| Asserting on internal repository calls | Couples tests to infrastructure | Assert on HTTP response and DB state instead |
| Feature files with no step definitions | Silent gaps in coverage | Every scenario in a `.feature` file must have a bound step definition |
| Gherkin as a scripting language | Brittle, hard-to-read scenarios | Steps describe business intent, not UI/API mechanics |
| Instance fields for cross-step state | State invisible to other binding classes | Use `ScenarioContext` (per scenario) or `FeatureContext` (per feature) |
| Magic string keys in ScenarioContext | Typos cause runtime `KeyNotFoundException` | Define keys as `private const string` constants |

## Common Rationalizations

| Rationalization | Reality |
|---|---|
| "I'll write tests after the code works" | You won't. Tests written after the fact test implementation, not behavior. |
| "This domain logic is too simple to test" | Simple code gets complicated. The test documents the invariant forever. |
| "Testcontainers are slow" | A Postgres container starts in ~2 seconds. The confidence is worth it. |
| "I'll mock the DB to keep integration tests fast" | A test that mocks EF Core is a unit test pretending to be an integration test. |
| "I tested it manually with Swagger" | Manual testing doesn't persist. Tomorrow's change breaks it with no signal. |
| "The type system makes tests unnecessary" | Types prevent invalid states. Tests verify that your logic produces correct states. |
| "Feature files are just documentation" | Unbound or unmaintained Gherkin is worse than no documentation — it lies. |

## Red Flags

- Writing code without any corresponding test
- Tests that pass on the first run without any implementation (testing the wrong thing)
- Bug fixes without a reproduction test that failed before the fix
- Integration tests that mock the database
- Test names that don't describe expected behavior (`Test1`, `Works`, `Check`)
- `[Skip]` attributes without a linked issue
- Asserting only on HTTP status code without verifying persisted state
- Single large test asserting on unrelated behaviors
- Reqnroll scenarios with no bound step definitions (pending steps silently pass)
- Feature files describing UI/API mechanics instead of business intent
- Cross-step state stored in instance fields instead of `ScenarioContext`

## Verification

After completing any implementation:

- [ ] Every new behavior has at least one unit test
- [ ] Domain invariants (Value Object `Create`, entity factories) are covered by unit tests
- [ ] Cross-boundary behavior (HTTP → DB) has an integration test
- [ ] Key business scenarios have a corresponding Reqnroll `.feature` file
- [ ] All `.feature` scenarios have bound step definitions (no pending steps)
- [ ] All tests pass: `dotnet test`
- [ ] Bug fixes include a reproduction test that failed before the fix
- [ ] Test names follow `{Method}_{Condition}_{ExpectedOutcome}`
- [ ] No tests were skipped or disabled without a tracked issue
- [ ] Integration tests assert on both HTTP response AND database state
