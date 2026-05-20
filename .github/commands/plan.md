---
description: Break work into small verifiable tasks with acceptance criteria and dependency ordering
---

Invoke the agent-skills:planning-and-task-breakdown skill.

Read the existing spec (`SPEC.md` or equivalent) and the relevant codebase sections. Then:

1. Enter plan mode — read only, no code changes
2. Identify the DDD dependency graph: `SharedKernel` → `Domain` → `Infrastructure` → `API` → `Tests`
3. Slice work vertically (one complete path per task: Value Object + Entity + EF config + endpoint + test)
4. Write tasks with acceptance criteria and verification steps (`dotnet build`, `dotnet test`, `dotnet ef migrations`)
5. Note which tasks require a new EF Core migration
6. Add checkpoints between phases
7. Present the plan for human review

Save the plan to `tasks/plan.md` and the task list to `tasks/todo.md`.
