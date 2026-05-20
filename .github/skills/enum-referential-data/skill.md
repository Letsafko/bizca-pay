---
name: enum-referential-data
description: Guides agents through creating enum-backed referential/lookup tables with EF Core seed data. Use when a domain enum (e.g. Status, Civility, ChannelType) needs to be persisted as a reference table in the database.
---

# Enum Referential Data

## Overview
Enum-backed lookup tables follow a three-part pattern: a domain `enum`, a `ReferentialData<TId>` infrastructure record, and an `IEntityTypeConfiguration` that seeds all enum values automatically.

## When to Use
- A new domain enum must be stored as a reference/lookup table (e.g. `OrderStatus`, `PaymentMethod`).
- An existing enum gains values that must be seeded.
- NOT for free-form configuration data that changes at runtime.

## Steps

### 1. Define the domain enum
Place in `microservices/{service}/src/Bizca.{Service}.Domain/{Aggregate}/Models/{EnumName}.cs`:
```csharp
namespace Bizca.Users.Domain.Users.Models;

public enum OrderStatus
{
    Draft = 1,
    Confirmed = 2,
    Cancelled = 3
}
```
- Always assign **explicit integer values starting at 1** (never rely on default 0-based ordering).

### 2. Create the Ref entity
Place in `microservices/{service}/src/Bizca.{Service}.Infrastructure/Context/ReferentialData/{EnumName}Ref.cs`:
```csharp
using Bizca.Users.Domain.Users.Models;

namespace Bizca.Users.Infrastructure.Context.ReferentialData;

internal sealed class OrderStatusRef : ReferentialData<OrderStatus>;
```
`ReferentialData<TId>` requires `Id`, `Label`, and `Description` (all `required`).

### 3. Create the entity configuration with seed data
`microservices/{service}/src/Bizca.{Service}.Infrastructure/Context/Configurations/{EnumName}RefEntityConfiguration.cs`:
```csharp
using System;
using System.Linq;
using Bizca.Users.Domain.Users.Models;
using Bizca.Users.Infrastructure.Context.ReferentialData;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bizca.Users.Infrastructure.Context.Configurations;

internal sealed class OrderStatusRefEntityConfiguration : IEntityTypeConfiguration<OrderStatusRef>
{
    public void Configure(EntityTypeBuilder<OrderStatusRef> builder)
    {
        builder.ToTable("orderStatus", DatabaseConstants.Schema);
        builder.HasKey(static e => e.Id).HasName(Constants.PkOrderStatusRef);
        builder.Property(static x => x.Id)
            .HasConversion(static x => (int)x, static x => (OrderStatus)x)
            .HasColumnName("orderStatusId");

        builder.Property(static e => e.Label).HasMaxLength(50).HasColumnName("label");
        builder.Property(static e => e.Description).HasMaxLength(50).HasColumnName("description");

        var seed = Enum.GetValues<OrderStatus>()
            .Select(e => new OrderStatusRef { Id = e, Label = e.ToString(), Description = e.ToString() })
            .ToArray();

        builder.HasData(seed);
    }

    private static class Constants
    {
        internal const string PkOrderStatusRef = "pk_orderStatus_ref";
    }
}
```

### 4. Register in ApplicationDbContext
```csharp
modelBuilder.ApplyConfiguration(new Configurations.OrderStatusRefEntityConfiguration());
```
Register **before** any configuration that has a FK to this table.

### 5. Add the FK on the owning entity
In the owning entity's configuration:
```csharp
builder.HasOne<OrderStatusRef>()
    .WithMany()
    .HasForeignKey(static e => e.Status)
    .IsRequired()
    .HasConstraintName("fk_order_orderStatusId");

builder.HasIndex(static e => e.Status).HasDatabaseName("ix_order_orderStatusId");
```

### 6. Generate and review the migration
Follow the `ef-migration` skill.

## Common Rationalizations
| Rationalization | Reality |
|---|---|
| "I don't need a reference table, I'll store the int directly" | Reference tables enforce referential integrity and make the database self-documenting. |
| "I'll use 0 as the first enum value" | 0 is the default uninitialized value in C#; using it as a valid business state creates subtle bugs. |
| "I can seed in a migration script instead of `HasData`" | `HasData` keeps seed data co-located with the model and is idempotent across environments. |

## Red Flags
- Enum values starting at `0`.
- Reference configuration registered after the entity that has a FK to it.
- `Label` / `Description` seeded as empty strings.

## Verification
- [ ] Enum values start at `1` with explicit assignments.
- [ ] `ReferentialData<TEnum>` subclass created in Infrastructure.
- [ ] Configuration has `HasData` seeding all enum members.
- [ ] Registered in `ApplicationDbContext` before the owning entity's configuration.
- [ ] FK + index configured on the owning entity.
- [ ] Migration generated and reviewed.
- [ ] Build passes with `TreatWarningsAsErrors=true`.

