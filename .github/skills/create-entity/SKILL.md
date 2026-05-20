---
name: create-entity
description: Guides agents through creating a domain Entity following the project's Entity<TId> pattern. Use when introducing a new aggregate root or child entity in the domain layer.
---

# Create Entity

## Overview
Domain entities in this codebase extend `Entity<TId>` (or `Entity` for id-less roots) from `Bizca.Sdk.SharedKernel`. They enforce private construction, expose a static `Create` factory, and keep all setters `private`.

## When to Use
- Adding a new aggregate root (e.g. `Order`, `Product`).
- Adding a child entity owned by an aggregate (e.g. `UserChannel`, `Address`).
- NOT for pure value concepts with no identity → use a Value Object instead.

## Steps

### 1. Determine identity type
Choose the ValueObject that represents the entity's PK (e.g. `UserId : IValueObject<UserId, int>`). If the entity needs optimistic concurrency, implement `IVersionedEntity`.

### 2. Create the file
Place in `microservices/{service}/src/Bizca.{Service}.Domain/{Aggregate}/{EntityName}.cs`.

### 3. Implement the pattern
Real example — `UserChannel` (child entity of `User`):
```csharp
using System;
using System.Collections.Generic;
using Bizca.Sdk.SharedKernel;
using Bizca.Users.Domain.Users.Models;
using Bizca.Users.Domain.Users.ValueObjects;

namespace Bizca.Users.Domain.Users;

public sealed class UserChannel : Entity<UserChannelId>
{
    private UserChannel(
        ChannelValue channelValue,
        ChannelType channelTypeId,
        DateTimeOffset createdDatetime,
        DateTimeOffset lastModifiedDatetime) : base(createdDatetime, lastModifiedDatetime)
    {
        _userChannelConfirmations = [];
        ChannelValue = channelValue;
        ChannelTypeId = channelTypeId;
        Confirmed = false;
    }

    public static UserChannel Create(
        ChannelValue channelValue,
        ChannelType channelType,
        DateTimeOffset creationDate)
    {
        return new UserChannel(channelValue, channelType, creationDate, lastModifiedDatetime: creationDate);
    }

    public ChannelValue ChannelValue { get; private set; }
    public ChannelType ChannelTypeId { get; private set; }
    public bool Confirmed { get; private set; }
    public IReadOnlyList<UserChannelConfirmation> UserChannelConfirmations => _userChannelConfirmations;
    private readonly List<UserChannelConfirmation> _userChannelConfirmations;
}
```
Note: entities are always `public sealed class`.

### 4. Child collections — two patterns
**Aggregate root (nullable list, lazy)** — used when children are loaded via EF navigation:
```csharp
public IReadOnlyList<UserChannel> UserChannels => _userChannels ?? [];
private readonly List<UserChannel>? _userChannels;
```
**Child entity (non-nullable, initialized in constructor)** — used when the list is always populated:
```csharp
public IReadOnlyList<UserChannelConfirmation> UserChannelConfirmations => _userChannelConfirmations;
private readonly List<UserChannelConfirmation> _userChannelConfirmations;
// in constructor: _userChannelConfirmations = [];
```
EF Core accesses both fields via `SetPropertyAccessMode(PropertyAccessMode.Field)` in the configuration.

### 5. Domain events ⚠️ Pattern available, not yet used
The SDK infrastructure exists (`AddDomainEvent`, `ClearDomainEvents`, `DomainEvents` on `Entity`), but **no entity in the codebase currently raises events**. When implementing, call `AddDomainEvent` inside the entity method that mutates state — see the `domain-event` skill.

### 6. Aggregate root vs child entity
| Aspect | Aggregate root | Child entity |
|---|---|---|
| Extends | `Entity<TId>` | `Entity<TId>` |
| Instantiated by | Its own `Create` factory | Parent aggregate via `ChildEntity.Create(...)` |
| EF navigation | Exposed as `IReadOnlyList<Child>` with field backing | Owned by parent via `HasMany`/`HasOne` |

## Common Rationalizations
| Rationalization | Reality |
|---|---|
| "I'll make the constructor `internal` for testability" | `private` constructor + static factory IS the testable API. |
| "I don't need `lastModifiedDatetime` in `Create`" | Pass `createdDatetime` for both on creation — convention used throughout the codebase (see `UserChannel.Create`). |
| "I can use auto-properties with public setters" | All setters are `private set`. EF Core maps via shadow properties and field access modes. |

## Red Flags
- Public or `internal` constructor on an entity.
- Entity class not `sealed`.
- Setters that are `public set` anywhere on the entity.
- Collections exposed as `List<T>` instead of `IReadOnlyList<T>`.
- Missing `Version { get; init; }` on an aggregate that needs optimistic concurrency.

## Verification
- [ ] Class is `public sealed class`.
- [ ] Constructor is `private`, `base(createdDatetime, lastModifiedDatetime)` called.
- [ ] Static `Create` factory is the only way to instantiate.
- [ ] All properties have `private set`.
- [ ] Child collections backed by the correct field pattern (nullable `?` for aggregate roots, non-nullable initialized with `[]` for child entities).
- [ ] EF entity configuration created and registered in `ApplicationDbContext.OnModelCreating`.
- [ ] Build passes with `TreatWarningsAsErrors=true`.

