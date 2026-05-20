---
description: Run TDD workflow — write failing tests, implement, verify. For bugs, use the Prove-It pattern.
---

Invoke the agent-skills:test-driven-development skill.

For new features:
1. Determine the right test level:
   - Domain logic (Value Objects, Entity factories) → xUnit `[Fact]`/`[Theory]` + FluentAssertions
   - EF Core persistence → Testcontainers (PostgreSQL) integration test
   - HTTP endpoints → Reqnroll `.feature` + step definitions + `WebApplicationFactory`
2. Write tests that describe the expected behavior (they should **FAIL**)
3. Implement the code to make them pass (Domain → Infrastructure → API order)
4. Refactor while keeping tests green: `dotnet test`

For bug fixes (Prove-It pattern):
1. Write a test that reproduces the bug (must **FAIL**)
2. Confirm it fails: `dotnet test --filter "FullyQualifiedName~TestName"`
3. Implement the fix
4. Confirm the test passes
5. Run the full suite for regressions: `dotnet test`

Reqnroll reminders:
- Share state between steps via `ScenarioContext` with `private const string` keys
- Deserialize HTTP responses immediately; store `int` status code + typed DTO — never `HttpResponseMessage`
- Use `table.CreateInstance<T>()` / `table.CreateSet<T>()` — never `row["column"]`
- Use `[BeforeScenario]` to truncate tables (Testcontainers manages container lifecycle, not data lifecycle)
