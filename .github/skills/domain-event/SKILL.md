---
name: domain-event
description: Guides agents through creating a domain event and its handler following the project's DomainEvent / IDomainEventHandler<T> pattern. Use when a domain state change must be communicated to other parts of the system without direct coupling.
---

# Domain Event

## Overview
Domain events decouple side-effects (notifications, projections, audit logs) from the domain model. An entity raises an event via `AddDomainEvent(...)`. A handler registered via DI receives it asynchronously.

**Current state:** The SDK infrastructure is fully implemented (`AddDomainEvent`, `ClearDomainEvents`, `DomainEvents` on `Entity`). The handler + dispatch pattern (steps 3–5) has not yet been implemented in any microservice — these steps describe the intended design, not existing code.

## When to Use
- A domain state transition needs to trigger a side-effect (email, audit, saga step).
- An aggregate root changes state and other bounded contexts need to know.
- NOT for synchronous in-transaction validation → use domain methods returning `Result<T>` instead.

## Steps

### 1. Create the event class
Place in `microservices/{service}/src/Bizca.{Service}.Domain/{Aggregate}/`:
```csharp
using System;
using Bizca.Sdk.SharedKernel;

namespace Bizca.Users.Domain.Users;

public sealed class UserCreatedEvent(DateTime creationDateUtc, Guid externalUserId)
    : DomainEvent(creationDateUtc)
{
    public Guid ExternalUserId { get; } = externalUserId;
}
```
Rules:
- Inherit `DomainEvent(DateTime creationDateUtc)`.
- Carry only the data that consumers need (IDs, not full entities).
- Name in past tense: `UserCreated`, `OrderConfirmed`, `ChannelVerified`.

### 2. Raise the event inside the entity
```csharp
public static User Create(UserProfile profile, string? passwordHash, string? securityStamp, DateTimeOffset now)
{
    var user = new User(...);
    user.AddDomainEvent(new UserCreatedEvent(now.UtcDateTime, user.ExternalUserId));
    return user;
}
```
`AddDomainEvent` is protected on `Entity`; call it from inside the entity class only.

### 3. Create the handler ⚠️ Pattern not yet implemented
Place in `microservices/{service}/src/Bizca.{Service}.Infrastructure/{Feature}/`:
```csharp
using System.Threading;
using System.Threading.Tasks;
using Bizca.Sdk.SharedKernel;
using Bizca.Users.Domain.Users;

namespace Bizca.Users.Infrastructure.Notifications;

public sealed class UserCreatedEventHandler : IDomainEventHandler<UserCreatedEvent>
{
    public Task Handle(UserCreatedEvent domainEvent, CancellationToken cancellationToken)
    {
        // send email, publish message, etc.
        return Task.CompletedTask;
    }
}
```

### 4. Register the handler ⚠️ Pattern not yet implemented
In `DependencyInjections.cs`:
```csharp
services.AddScoped<IDomainEventHandler<UserCreatedEvent>, UserCreatedEventHandler>();
```

### 5. Dispatch events ⚠️ Pattern not yet implemented — application layer pending
After persisting an aggregate, dispatch its domain events. This will live in the application layer once CQRS handlers are in place:
```csharp
foreach (var domainEvent in entity.DomainEvents)
    await handler.Handle(domainEvent, cancellationToken);

entity.ClearDomainEvents();
```
Call `ClearDomainEvents()` after dispatch to prevent re-processing.

## Common Rationalizations
| Rationalization | Reality |
|---|---|
| "I'll pass the full entity to the event payload" | Events are contracts; passing entities creates tight coupling. Use IDs only. |
| "I'll dispatch events inside the entity" | Entities have no access to DI; dispatch is the application/infrastructure layer's responsibility. |
| "I don't need to clear domain events after dispatch" | Uncleaned events will be re-dispatched on the next save in the same request, causing duplicate side-effects. |

## Red Flags
- Event payload contains a full entity reference instead of IDs.
- Domain event raised outside the entity (e.g. in a controller or service).
- `ClearDomainEvents()` not called after dispatch.
- Handler performing domain logic instead of infrastructure side-effects.

## Verification
- [ ] Event class inherits `DomainEvent`, named in past tense, payload is IDs only.
- [ ] `AddDomainEvent` called inside the entity method that changes state.
- [ ] ⚠️ Handler and dispatch (steps 3–5) are only applicable once the application layer exists — skip for now if implementing on a microservice without CQRS handlers.

