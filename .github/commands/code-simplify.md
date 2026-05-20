---
description: Simplify code for clarity and maintainability — reduce complexity without changing behavior
---

Invoke the agent-skills:code-simplification skill.

Simplify recently changed code (or the specified scope) while preserving exact behavior:

1. Read CLAUDE.md and study project conventions
2. Identify the target code — recent changes unless a broader scope is specified
3. Understand the code's purpose, callers, edge cases, and test coverage before touching it
4. Scan for simplification opportunities:
   - Deep nesting → guard clauses or extracted helpers
   - Long methods → split by single responsibility
   - Manual `if (result.IsFailure) return result.Error` chains → use `Result<T>` implicit conversions
   - Generic names → descriptive names consistent with domain language
   - Duplicated `IEntityTypeConfiguration<T>` logic → shared base or extension method
   - Dead code → remove after confirming no callers
5. Apply each simplification incrementally — run `dotnet test` after each change
6. Verify: `dotnet build` (zero warnings), `dotnet test` (all green), diff is clean

If tests fail after a simplification, revert that change and reconsider. Use `code-review-and-quality` to review the result.
