---
name: efcore-entity-configuration
description: Guides agents through writing an EF Core IEntityTypeConfiguration<T> for a domain entity. Use when mapping a new or existing entity to a database table, adding columns, configuring relationships, or adding indexes.
---

# EF Core Entity Configuration

## Overview
Every entity is mapped via a dedicated `internal sealed class XyzEntityConfiguration : IEntityTypeConfiguration<Xyz>` in `Bizca.{Service}.Infrastructure/Context/Configurations/`. All configurations are discovered automatically via `ApplyConfigurationsFromAssembly` in `ApplicationDbContext.OnModelCreating` — no manual registration required.

## When to Use
- A new domain entity needs a database mapping.
- Relationships, indexes, constraints, or column names must be configured.
- NOT for referential/enum tables → use the `enum-referential-data` skill instead.

## Steps

### 1. Create the configuration file
`microservices/{service}/src/Bizca.{Service}.Infrastructure/Context/Configurations/{Entity}EntityConfiguration.cs`

### 2. Implement the pattern
Real example — `UserChannelEntityConfiguration`:
```csharp
using Bizca.Users.Domain.Users;
using Bizca.Users.Domain.Users.Models;
using Bizca.Users.Domain.Users.ValueObjects;
using Bizca.Users.Infrastructure.Context.Extensions;
using Bizca.Users.Infrastructure.Context.ReferentialData;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bizca.Users.Infrastructure.Context.Configurations;

internal sealed class UserChannelEntityConfiguration : IEntityTypeConfiguration<UserChannel>
{
    public void Configure(EntityTypeBuilder<UserChannel> builder)
    {
        builder.ToTable("userChannel", DatabaseConstants.Schema);           // schema = "usr"
        builder.HasKey(static e => e.Id).HasName(Constants.PkUserChannel);

        // Int-backed ValueObject PK — extension method sets conversion + column name
        builder.Property(static x => x.Id)
            .ValueGeneratedOnAdd()
            .HasConversion<IntValueObjectConverter<UserChannelId>>()
            .HasValueGenerator<IntValueObjectValueGenerator<UserChannelId>>()
            .HasColumnName(Constants.UserChannelIdColumnName);

        builder.Property(static e => e.ChannelValue)
            .HasConversion(static x => (string)x, static x => ChannelValue.Create(x).Value)
            .HasMaxLength(100)
            .HasColumnName("channelValue");

        builder.Property(static e => e.ChannelTypeId)
            .HasConversion(static x => (int)x, static x => (ChannelType)x)
            .HasColumnName("channelTypeId");

        builder.Property(static e => e.Confirmed).HasColumnName("confirmed");

        // Auditing — no version on child entities
        builder.AddAuditingProperties().IgnoreAuditingProperties();

        // Collection with field-backing
        builder.HasMany(static x => x.UserChannelConfirmations)
               .WithOne()
               .HasForeignKey(Constants.UserChannelIdColumnName)
               .OnDelete(DeleteBehavior.ClientCascade)
               .IsRequired()
               .HasConstraintName(Constants.FkUserChannelConfirmation)
               .Metadata.PrincipalToDependent
               ?.SetPropertyAccessMode(PropertyAccessMode.Field);

        // FK to referential table
        builder.HasOne<ChannelTypeRef>()
               .WithMany()
               .IsRequired()
               .HasForeignKey(static e => e.ChannelTypeId)
               .HasConstraintName(Constants.FkUserChannelChannelTypeId);

        builder.HasIndex(static e => e.ChannelTypeId).HasDatabaseName(Constants.IxUserChannelChannelTypeId);
    }

    private static class Constants
    {
        internal const string PkUserChannel = "pk_userChannel";
        internal const string UserChannelIdColumnName = "userChannelId";
        internal const string FkUserChannelChannelTypeId = "fk_userChannel_channelTypeId";
        internal const string IxUserChannelChannelTypeId = "ix_userChannel_channelTypeId";
        internal const string FkUserChannelConfirmation = "fk_userChannel_userChannelConfirmation";
    }
}
```

### 3. Registration — automatic via `ApplyConfigurationsFromAssembly`
No action required. `ApplicationDbContext.OnModelCreating` uses:
```csharp
modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
```
Any `IEntityTypeConfiguration<T>` class placed in the correct assembly is discovered automatically. The class must have a parameterless constructor (implicit when no constructor is declared).

### 4. Convention reference
| Concept | Rule |
|---|---|
| Table names | camelCase string literals — **no global snake_case conversion**, what you write is what goes to the DB |
| Database schema | `DatabaseConstants.Schema` → `"usr"` |
| Column names | camelCase string literals (e.g. `"userChannelId"`, `"channelValue"`) |
| PK name | `"pk_{tableName}"` |
| FK name | `"fk_{table}_{relatedTable}"` |
| Index name | `"ix_{table}_{columnName}"` |
| Constraint names | All in a nested `private static class Constants` |
| Auditing | Always call `.AddAuditingProperties().IgnoreAuditingProperties()` |
| `IVersionedEntity` | Call `.AddVersionAsShadowProperty()` **before** auditing helpers (aggregate roots only) |
| String columns | Non-unicode by default (set globally in `ConfigureConventions`) |
| DateTime columns | Auto-converted to UTC via `UtcDateTimeConverter` (set globally) |

### 5. Value conversions cheat sheet
| Type | Pattern |
|---|---|
| `int`-backed ValueObject **PK** (aggregate root) | `.ToIntValueObjectConverter(columnName)` extension — shorthand for `HasConversion<IntValueObjectConverter<T>>() + HasColumnName(...)` |
| `int`-backed ValueObject **PK** (child entity) | `HasConversion<IntValueObjectConverter<T>>()` + `HasColumnName(...)` separately (see `UserChannelEntityConfiguration`) |
| `string`-backed ValueObject | `HasConversion(x => x.Value, x => MyType.Create(x).Value)` |
| Nullable ValueObject | `HasConversion(x => (string?)x, x => MyType.TryCreate(x))` |
| `enum` → `int` | `HasConversion(x => (int)x, x => (MyEnum)x)` |

## Common Rationalizations
| Rationalization | Reality |
|---|---|
| "EF will convert my camelCase names to snake_case automatically" | There is no global snake_case convention in `ApplicationDbContext`. Column names are written as camelCase and stored exactly as-is. |
| "EF will figure out the column names automatically" | All FK/PK/index names require explicit `.HasName()`/`.HasConstraintName()` calls — they won't be inferred. |
| "I need to register it manually in `ApplicationDbContext`" | `ApplyConfigurationsFromAssembly` is used — no manual registration needed. Placing the class in the correct assembly is sufficient. |
| "I can skip `.IgnoreAuditingProperties()`" | Without it, EF tries to map `DomainEvents` as a navigation property, causing a migration error. |

## Red Flags
- Configuration class is `public` instead of `internal sealed`.
- Missing `SetPropertyAccessMode(PropertyAccessMode.Field)` on a collection backed by a private field.
- Constraint / index names hardcoded from a `Constants` nested class.
- Constructor with parameters added to a configuration — `ApplyConfigurationsFromAssembly` requires a parameterless constructor.

## Verification
- [ ] Configuration class is `internal sealed`.
- [ ] All string constants in a nested `private static class Constants`.
- [ ] `.AddAuditingProperties().IgnoreAuditingProperties()` called.
- [ ] No manual registration in `ApplicationDbContext` — `ApplyConfigurationsFromAssembly` discovers it automatically.
- [ ] `dotnet ef migrations add` produces only the expected SQL.
- [ ] Build passes with `TreatWarningsAsErrors=true`.

