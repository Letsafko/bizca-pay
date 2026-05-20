# API Design Checklist

Quick reference for REST API design in the Bizca .NET solution. Use alongside the `api-and-interface-design` skill.

## Table of Contents

- [Endpoint Design](#endpoint-design)
- [Request & Response Contracts](#request--response-contracts)
- [Error Handling — ProblemDetails](#error-handling--problemdetails)
- [Result<T> to HTTP Mapping](#resultt-to-http-mapping)
- [Minimal API Patterns](#minimal-api-patterns)
- [OpenAPI / Swagger](#openapi--swagger)
- [Common Anti-Patterns](#common-anti-patterns)

---

## Endpoint Design

- [ ] Resource names are plural nouns (`/users`, `/orders`) — not verbs
- [ ] Hierarchy reflects ownership (`/users/{id}/channels`, not `/channels?userId=`)
- [ ] HTTP verbs used correctly: `GET` read, `POST` create, `PUT`/`PATCH` update, `DELETE` remove
- [ ] `GET` endpoints are idempotent and side-effect free
- [ ] `POST` returns `201 Created` with `Location` header pointing to the new resource
- [ ] `PUT`/`PATCH` returns `200 OK` with updated resource or `204 No Content`
- [ ] `DELETE` returns `204 No Content`
- [ ] List endpoints are paginated (never return unbounded collections)

---

## Request & Response Contracts

- [ ] Request DTOs are flat, focused records — no domain entities exposed directly
- [ ] Response DTOs exclude internal fields (`passwordHash`, `concurrencyToken`, EF shadow properties)
- [ ] Nullable fields are explicitly annotated (`string?`) — `Nullable=enable` is enforced
- [ ] Date/time fields use `DateTimeOffset` (timezone-aware), not `DateTime`
- [ ] IDs in responses are strings or `Guid` — never expose internal numeric surrogate keys
- [ ] Consistent property naming in JSON: camelCase via `UseCamelCaseNamingConvention()` / `JsonNamingPolicy.CamelCase`
- [ ] Collection responses wrapped: `{ "items": [...], "totalCount": N }` (not bare arrays)

---

## Error Handling — ProblemDetails

All errors follow [RFC 9457](https://www.rfc-editor.org/rfc/rfc9457) `ProblemDetails` shape:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "Bad Request",
  "status": 400,
  "detail": "Email address is not valid.",
  "instance": "/users"
}
```

- [ ] `app.UseExceptionHandler()` registered globally — no raw exceptions reach the client
- [ ] No stack traces, SQL details, or EF exception messages in production responses
- [ ] Validation errors return `422 Unprocessable Entity` with field-level detail
- [ ] Not-found returns `404` with a meaningful `detail` field
- [ ] Conflict (duplicate) returns `409 Conflict`

---

## Result<T> to HTTP Mapping

Map `ErrorType` → HTTP status consistently across all endpoints:

| `ErrorType` | HTTP Status | Notes |
|-------------|-------------|-------|
| `None` (success) | `200` / `201` / `204` | Depends on verb |
| `Validation` | `422 Unprocessable Entity` | Field-level errors |
| `NotFound` | `404 Not Found` | |
| `Conflict` | `409 Conflict` | Duplicate key, state clash |
| `Failure` | `400 Bad Request` | Business rule violation |
| `Problem` | `500 Internal Server Error` | Unexpected / unrecoverable |

```csharp
// Canonical mapping helper (minimal API)
return result.IsSuccess
    ? Results.Ok(result.Value)
    : result.Error.Type switch
    {
        ErrorType.NotFound   => Results.NotFound(ToProblem(result.Error)),
        ErrorType.Conflict   => Results.Conflict(ToProblem(result.Error)),
        ErrorType.Validation => Results.UnprocessableEntity(ToProblem(result.Error)),
        _                    => Results.BadRequest(ToProblem(result.Error))
    };
```

---

## Minimal API Patterns

```csharp
// Group endpoints by resource
var users = app.MapGroup("/api/users").RequireAuthorization();

users.MapGet("{id:guid}", async (Guid id, IUserService svc, CancellationToken ct) =>
{
    var result = await svc.GetByIdAsync(new UserId(id), ct);
    return result.IsSuccess ? Results.Ok(result.Value) : Results.NotFound();
})
.WithName("GetUserById")
.Produces<UserResponse>()
.ProducesProblem(404);
```

- [ ] Endpoints grouped with `MapGroup` per resource
- [ ] `CancellationToken` accepted on all async handlers
- [ ] `WithName` set for `Location` header generation after `POST`
- [ ] `.Produces<T>()` / `.ProducesProblem(N)` declared for OpenAPI
- [ ] Authorization declared at group level — individual endpoints opt out explicitly

---

## OpenAPI / Swagger

- [ ] Swashbuckle / Scalar registered in `Program.cs`
- [ ] Every endpoint has `.WithSummary()` and `.WithDescription()`
- [ ] Request/response examples provided via `.WithOpenApi()`
- [ ] Bearer token authentication scheme registered in Swagger UI
- [ ] API available at `/swagger` in Development only (`app.UseSwagger()` gated on `IsDevelopment()`)
- [ ] XML doc comments (`<summary>`) enabled on public DTO types (`GenerateDocumentationFile=true`)

---

## Common Anti-Patterns

| Anti-Pattern | Problem | Fix |
|---|---|---|
| Exposing domain entities as responses | DB schema leaks, tight coupling | Use dedicated response DTOs |
| `GET /getUserById?id=...` | Non-RESTful, not cacheable | `GET /users/{id}` |
| Returning `200` for creation | Incorrect; client can't find the new resource | Return `201 Created` + `Location` header |
| Swallowing `Result.IsFailure` silently | Client gets `200` for a failed operation | Always map failure to an error HTTP status |
| Raw `Exception` reaching the client | Stack trace exposure | `UseExceptionHandler` + `ProblemDetails` |
| Missing `CancellationToken` | Request cancellations not propagated to DB | Add `CancellationToken ct` to every async handler |
| Unbounded list endpoints | Memory exhaustion, timeouts | Paginate with `skip`/`take` or cursor |
| `DateTime` instead of `DateTimeOffset` | Timezone ambiguity | Always use `DateTimeOffset` |
