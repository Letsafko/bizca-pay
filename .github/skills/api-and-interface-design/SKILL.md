---
name: api-and-interface-design
description: Guides stable API and interface design in C# / ASP.NET Core. Use when designing REST endpoints, defining contracts between layers (domain, application, API), creating DTOs/command objects, or establishing module boundaries.
---

# API and Interface Design (C#)

## Overview

Design stable, well-documented interfaces that are hard to misuse. Good interfaces make the right thing easy and the wrong thing hard. This applies to REST endpoints, CQRS command/query contracts, DTO shapes, and any surface where one layer talks to another.

## When to Use

- Designing new ASP.NET Core endpoints
- Defining contracts between API, Application, and Domain layers
- Creating request/response DTOs or command/query objects
- Changing existing public interfaces

## Core Principles

### Hyrum's Law

> With a sufficient number of users of an API, all observable behaviors of your system will be depended on by somebody, regardless of what you promise in the contract.

Every public behavior — including undocumented quirks, error message text, serialization format, and ordering — becomes a de facto contract once consumers depend on it. Design implications:

- **Be intentional about what you expose.** Every public method, property, and HTTP status code is a potential commitment.
- **Serialization behavior is a contract.** Property casing, null vs. omitted fields, date format — all are observable. ASP.NET Core's default camelCase JSON is the contract; deviating locally breaks consumers.
- **Don't leak domain types through the API boundary.** Returning an entity couples consumers to your persistence model. Return DTOs.
- **Plan for deprecation at design time.** If you can't remove it safely, you shouldn't add it carelessly. See `deprecation-and-migration`.

### The One-Version Rule

Avoid forcing consumers to maintain two versions of the same contract simultaneously. Prefer **additive changes** (new optional fields, new endpoints) over parallel versions. When breaking changes are unavoidable, use versioning — not parallel maintenance.

### Contract First

Define request and response shapes **before** writing the handler. The contract is the spec; the implementation follows. Use `sealed record` for all DTOs — immutable, value-equal, serializer-friendly.

The shape answers three questions up front:
1. What does the caller need to provide?
2. What does the server guarantee to return?
3. What errors are possible, and how are they shaped?

If you can't answer all three, the interface isn't ready.

## Layer Responsibility Map

Each layer owns a distinct validation concern. Mixing them creates untestable, leaky boundaries.

| Layer | Owns | Tool |
|---|---|---|
| **API** | Input shape, format, required fields | FluentValidation on commands/queries |
| **Domain** | Business invariants, value constraints | `Result<T>` factories on Value Objects |
| **Infrastructure** | External service response shape | Explicit DTO deserialization + validation |

**Never validate business rules in the API layer.** "Email must be unique" is a domain concern — not a format concern. It belongs in a domain service or repository check, not in a FluentValidation rule.

**Never trust external API responses.** A third-party service can return unexpected types, missing fields, or malicious content. Always deserialize into a typed DTO before using the data in any logic.

See `result-error-handling` for the `Result<T>` / `Error` pattern and `fluent-validation-decorator` for wiring FluentValidation into the command pipeline.

## Error Contract — ProblemDetails (RFC 7807)

All error responses follow the same shape: **ProblemDetails**. One extension method maps every `Error` to its HTTP status. Consistency here means consumers can write one error handler instead of one per endpoint.

| Scenario | HTTP Status | `ErrorType` |
|---|---|---|
| Resource not found | 404 | `ErrorType.NotFound` |
| Duplicate / state conflict | 409 | `ErrorType.Conflict` |
| Domain rule violation | 422 | `ErrorType.Failure` / `ErrorType.Problem` |
| Input validation failure | 400 | `ValidationError` (multiple errors) |
| Server fault | 500 | Unhandled exception → exception middleware |

Never mix error strategies. If some endpoints return `ProblemDetails`, others throw, and others return `null` — consumers can't predict behavior.

## Naming Conventions

| Element | Convention | Example |
|---|---|---|
| REST endpoints | Plural nouns, no verbs | `POST /api/users`, `GET /api/users/{id}` |
| Query string | camelCase | `?sortBy=createdAt&pageSize=20` |
| JSON fields | camelCase (ASP.NET Core default) | `{ "createdAt", "firstName" }` |
| Boolean properties | `Is`/`Has`/`Can` prefix | `IsConfirmed`, `HasAddress` |
| Enum values | `UPPER_SNAKE` | `"IN_PROGRESS"`, `"COMPLETED"` |
| Commands | Verb + Noun | `CreateUserCommand`, `ConfirmChannelCommand` |
| Queries | `Get`/`List` + Noun | `GetUserByIdQuery`, `ListUsersQuery` |

## Key Design Decisions

### Input/Output Separation

Inputs (commands, queries) carry what the caller provides. Outputs (responses) carry what the server guarantees. They are never the same type — even if their fields overlap today.

Conflating them creates pressure to add server-generated fields to the input, or to remove required fields from the output. Keep them separate from the first commit.

### Prefer Addition Over Modification

Adding an optional nullable field is safe — existing consumers ignore it. Renaming, removing, or changing the type of an existing field is a breaking change, regardless of whether it's marked optional. Once a field is shipped, treat it as a commitment.

### Always Paginate List Endpoints

A list endpoint without pagination becomes a problem the moment data grows. Design `PagedResult<T>` from day one with `Page`, `PageSize`, `TotalItems`, `TotalPages`. Adding pagination later is a breaking change for consumers who cache or stream the full result.

### PATCH over PUT for Updates

`PUT` requires the full object — fields not included are overwritten with defaults. `PATCH` accepts a partial object where `null` means "not provided, do not change." Use nullable properties on the request record to model optionality.

### Value Objects Over Primitive IDs

A method taking two `int` parameters — `userId` and `orderId` — **will** be called with them swapped. A method taking `UserId` and `OrderId` makes that a compile error. Use Value Objects for all IDs crossing layer boundaries. See `create-value-object`.

## Common Rationalizations

| Rationalization | Reality |
|---|---|
| "We'll document the API later" | The record types ARE the documentation. Define them first. |
| "We don't need pagination for now" | Adding it later is a breaking change. Add `PagedResult<T>` from day one. |
| "PATCH is complicated, let's just use PUT" | PUT overwrites fields the caller didn't intend to change. |
| "We'll version the API when we need to" | Breaking changes without versioning break consumers the moment they deploy. |
| "Nobody depends on that undocumented behavior" | Hyrum's Law: if it's observable, somebody depends on it. |
| "We can return the domain entity directly" | Domain entities expose internal setters and navigation properties consumers should never see. |
| "Internal APIs don't need contracts" | Internal consumers are still consumers. Typed contracts prevent implicit coupling between layers. |
| "Validation in the domain layer is enough" | The domain rejects bad invariants. The API layer rejects malformed input. They guard different things. |

## Red Flags

- Endpoints returning different JSON shapes depending on runtime conditions
- Mixed error strategies across endpoints (throw vs. `null` vs. `ProblemDetails`)
- Business rule validation (uniqueness, state machines) inside FluentValidation rules
- Breaking changes to existing request properties — renames, type changes, removals
- List endpoints without pagination
- Verbs in REST URLs (`/api/createUser`, `/api/getUsers`)
- External API responses used directly without typed deserialization
- Domain entities appearing in response DTOs
- Raw `int` / `string` IDs passed across layer boundaries

## Verification

After designing an API endpoint:

- [ ] Request and response are separate `sealed record` types, defined before the handler
- [ ] All `Error` cases map to `ProblemDetails` via a single shared extension method
- [ ] Validation concerns are correctly placed: format/shape in FluentValidation, invariants in domain `Result<T>` factories
- [ ] List endpoints return a paginated response type
- [ ] New fields are optional and additive — no existing field was renamed or removed
- [ ] No domain entity type appears in any response DTO
- [ ] Value Objects used for all IDs crossing layer boundaries
- [ ] Naming follows the conventions table above
- [ ] Build passes with `TreatWarningsAsErrors=true`
