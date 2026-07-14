---
description: Implement the next task incrementally — build, test, verify, commit
---

Invoke the agent-skills:incremental-implementation skill alongside agent-skills:test-driven-development.

Pick the next pending task from the plan. For each task:

1. Read the task's acceptance criteria
2. Load relevant context (existing code, patterns, types)
3. Write a failing test for the expected behavior (RED)
   - Domain logic → xUnit `[Fact]`/`[Theory]` with FluentAssertions
   - Persistence → Testcontainers (PostgreSQL) integration test
   - HTTP endpoint → Reqnroll `.feature` + step definitions
4. Implement the minimum code to pass the test (GREEN)
   - Domain first (`Entity<TId>`, `ValueObject`, `Result<T>`)
   - Then Infrastructure (`IEntityTypeConfiguration<T>`, migration)
   - Then API (minimal API endpoint, request/response DTO)
5. Run the full test suite: `dotnet test`
6. Verify the build with zero warnings: `dotnet build` (`TreatWarningsAsErrors=true`)
7. If a new migration is required: `dotnet ef migrations add <MigrationName> --project Bizca.Users.Infrastructure`
8. Commit with a conventional commit message (`feat:`, `fix:`, `refactor:`)
9. Mark the task complete in `tasks/todo.md` and move to the next one

If any step fails, follow the agent-skills:debugging-and-error-recovery skill.
