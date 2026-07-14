---
name: result-error-handling
description: Guides agents through using the Result<T>/Error pattern from Bizca.Sdk.SharedKernel for domain validation and error propagation. Use when implementing domain validation, factory methods, or any operation that can fail with a typed error.
---

# Result / Error Handling

## Overview
This project uses a Railway-Oriented Programming style via `Result` and `Result<TValue>` from `Bizca.Sdk.SharedKernel`. Exceptions are reserved for truly exceptional infrastructure failures. Domain validation returns typed errors through the Result chain.

## When to Use
- A factory (`Create`) or domain operation can fail due to invalid input.
- Propagating validation failures from domain through application to API.
- NOT for infrastructure exceptions (DB connection failure, etc.) → let those propagate as exceptions.

## Error types
```csharp
Error.Failure(code, description)   // General failure
Error.NotFound(code, description)  // Resource not found
Error.Problem(code, description)   // Domain rule violation
Error.Conflict(code, description)  // Uniqueness / state conflict
new ValidationError(error1, error2) // Multiple validation errors
```

## Pattern: factory validation
```csharp
public static Result<UserId> Create(int value)
{
    const string errorCode = "INVALID_USER_ID";
    return value switch
    {
        <= 0 => Error.Problem(errorCode, "Value must be greater than 0"),
        _ => new UserId(value)   // implicit Result<UserId> via implicit operator
    };
}
```
The `Result<TValue>` implicit operators handle both the success and failure paths:
- `TValue` → `Result<TValue>` (success)
- `Error` → `Result<TValue>` (failure)

## Pattern: consuming a Result
```csharp
var result = UserId.Create(rawId);
if (!result.IsSuccess)
    return result.Error;   // propagate up as Result<T>

var userId = result.Value; // safe to access because IsSuccess == true
```

## Pattern: ValidationError (multiple errors)
```csharp
var errors = new List<Error>();
var firstNameResult = FirstName.Create(dto.FirstName);
if (!firstNameResult.IsSuccess) errors.Add(firstNameResult.Error);

var lastNameResult = LastName.Create(dto.LastName);
if (!lastNameResult.IsSuccess) errors.Add(lastNameResult.Error);

if (errors.Count > 0)
    return new ValidationError([.. errors]);
```

## Error code conventions
- Format: `SCREAMING_SNAKE_CASE`
- Defined as a `const string` inside the method or a `static class` (not as magic strings inline).
- Examples from codebase: `INVALID_USER_ID`, `INVALID_CHANNEL_VALUE`, `INVALID_COUNTRY_CODE_LENGTH`

## Common Rationalizations
| Rationalization | Reality |
|---|---|
| "I'll just throw an exception for invalid input" | Exceptions are not part of the domain API contract; they break the railway pattern and complicate callers. |
| "I can access `.Value` without checking `IsSuccess`" | `Result<T>.Value` throws `InvalidOperationException` when `IsSuccess == false`. |
| "Error codes don't need to be constants" | Magic strings produce non-unique errors and break consumers who match on codes. |

## Red Flags
- `throw new ArgumentException(...)` inside a domain factory.
- Accessing `.Value` before verifying `IsSuccess`.
- Returning `null` instead of `Error.NotFound(...)`.
- Inline string literals for error codes.

## Verification
- [ ] All factory methods return `Result<T>`.
- [ ] Error codes are `const string` variables, not inline literals.
- [ ] All `result.Value` accesses are guarded by an `IsSuccess` check.
- [ ] Validation accumulates multiple errors into `ValidationError` when appropriate.
- [ ] Build passes with `TreatWarningsAsErrors=true`.

