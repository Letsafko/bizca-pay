---
description: Conduct a five-axis code review — correctness, readability, architecture, security, performance
---

Invoke the agent-skills:code-review-and-quality skill.

Review the current changes (staged or recent commits) across all five axes:

1. **Correctness** — Does it match the spec? `Result<T>` failures returned (not thrown)? `Nullable` annotations respected? `async`/`await` correct?
2. **Readability** — Clear names consistent with domain language? Guard clauses? `Result<T>` implicit conversions used?
3. **Architecture** — Respects layer hierarchy (`SharedKernel → Domain → Infrastructure → API`)? `Entity<TId>` / `ValueObject` / `IEntityTypeConfiguration<T>` patterns followed?
4. **Security** — Input validated at API boundary? Secrets not committed (`appsettings.Local.json` only)? LINQ used (not raw SQL strings)? (Use `security-and-hardening` skill)
5. **Performance** — No EF Core N+1? No unbounded queries? No `.Result`/`.Wait()` blocking? Indexes named with `HasDatabaseName("ix_...")`? (Use `performance-optimization` skill)

Categorize findings as Critical, Important, or Suggestion.
Output a structured review with specific `Project/File.cs:line` references and fix recommendations.
