---
name: create-value-object
description: Guides agents through creating a domain Value Object following the project's IValueObject<T,TValue> pattern. Use when adding a new value object to the domain layer, wrapping a primitive with validation, or replacing a raw primitive with a typed wrapper.
---

# Create Value Object

## Overview
Value Objects in this codebase wrap a single primitive with validation, structural equality, and an implicit conversion operator. They implement `IValueObject<T, TValue>` (from `Bizca.Sdk.SharedKernel`) and extend `ValueObject`.

## When to Use
- A domain concept is currently expressed as a raw `int`, `string`, `Guid`, etc.
- A new domain identifier or constrained string/number must be introduced.
- NOT for complex multi-property domain concepts → use an Entity instead.

## Steps

### 1. Create the file
Place it in `microservices/{service}/src/Bizca.{Service}.Domain/{Aggregate}/ValueObjects/{Name}.cs`.

### 2. Implement the pattern
```csharp
using System.Collections.Generic;
using Bizca.Sdk.SharedKernel;

namespace Bizca.Users.Domain.Users.ValueObjects;

public sealed class MyId : ValueObject, IValueObject<MyId, int>
{
    public int Value { get; }

    private MyId(int value) => Value = value;

    public static Result<MyId> Create(int value)
    {
        const string errorCode = "INVALID_MY_ID";
        return value switch
        {
            <= 0 => Error.Problem(errorCode, "Value must be greater than 0"),
            _ => new MyId(value)
        };
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public static implicit operator int(MyId id) => id.Value;
}
```

### 3. Key rules
- Constructor is **always `private`**.
- Factory method is **always `static Result<T> Create(...)`**.
- Return `Error.Problem(code, description)` for invalid input; return `new T(value)` for valid (implicitly converts via `Result<T>` implicit operator).
- Error codes use `SCREAMING_SNAKE_CASE` (e.g. `INVALID_CHANNEL_VALUE`).
- `GetEqualityComponents()` yields every field that defines equality.
- Add an `implicit operator TValue(T obj)` for ergonomic unwrapping.
- For **nullable/optional** creation (no validation needed), add a `static T? TryCreate(TValue? value)` alongside `Create` — see `CountryCode.TryCreate`.

### 4. EF Core wiring (Infrastructure layer)
If backed by `int`, use `IntValueObjectConverter<T>` + `IntValueObjectValueGenerator<T>` in the entity configuration:
```csharp
builder.Property(static x => x.Id)
    .ValueGeneratedOnAdd()
    .HasValueGenerator<IntValueObjectValueGenerator<MyId>>()
    .ToIntValueObjectConverter("myIdColumnName");
```
For `string`/`Guid`, use an inline lambda conversion:
```csharp
builder.Property(static e => e.ExternalId)
    .HasConversion(static x => x.Value, static x => MyStringId.Create(x).Value);
```

## Common Rationalizations
| Rationalization | Reality |
|---|---|
| "It's just a string, no need to wrap it" | Primitives leak validation; the domain becomes untrustworthy without constrained types. |
| "I'll add `Create` validation later" | The pattern requires a `private` constructor — there's no other way to instantiate the type. |
| "I can skip `GetEqualityComponents`" | ValueObject equality is structurally defined; skipping it breaks equality checks and EF change tracking. |

## Red Flags
- Public constructor on a ValueObject.
- `Create` returning `T` directly instead of `Result<T>`.
- Error code as a magic string inline instead of a `const`.

## Verification
- [ ] Constructor is `private`.
- [ ] `Create` returns `Result<T>`.
- [ ] `GetEqualityComponents` yields all identity fields.
- [ ] `implicit operator` present for ergonomic unwrapping.
- [ ] EF configuration registers the correct converter.
- [ ] Build passes with `TreatWarningsAsErrors=true`.

