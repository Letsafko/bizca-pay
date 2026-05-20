---
description: Start spec-driven development — write a structured specification before writing code
---

Invoke the agent-skills:spec-driven-development skill.

Begin by understanding what the user wants to build. Ask clarifying questions about:
1. The objective — which microservice / domain area is affected (`user`, `order`, `notification`, or new)?
2. Core features and acceptance criteria (Gherkin scenarios welcome)
3. Domain model impact: new entities, value objects, domain events, or changes to existing ones?
4. API surface: new endpoints, request/response contracts, HTTP status codes?
5. Known boundaries (what to always do, ask first about, and never do)

Then generate a structured spec covering: objective, domain model changes, API contract, EF Core schema impact (new migration needed?), testing strategy (unit / integration / functional), and boundaries.

Save the spec as `SPEC.md` in the project root and confirm with the user before proceeding.
