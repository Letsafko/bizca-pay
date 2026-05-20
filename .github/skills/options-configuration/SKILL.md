---
name: options-configuration
description: Guides agents through adding strongly-typed configuration options following the IConfigureOptions<T> pattern. Use when a new infrastructure service requires settings from appsettings.json (connection strings, timeouts, feature flags, etc.).
---

# Options Configuration

## Overview
Configuration is bound to strongly-typed options classes via `IConfigureOptions<T>` implementations, not via `Configure<T>(configuration.GetSection(...))`. This separates option construction from registration and supports complex binding (e.g. combining a section with a connection string).

## When to Use
- A new infrastructure component needs settings (timeouts, URLs, credentials, etc.).
- An existing service requires an additional configuration property.
- NOT for single-value settings used only once → inline `configuration.GetValue<T>` is acceptable there.

## Steps

### 1. Create the options class
`microservices/{service}/src/Bizca.{Service}.Infrastructure/{Feature}/Options/{Feature}Options.cs`:
```csharp
namespace Bizca.Users.Infrastructure.Cache.Options;

public sealed class CacheOptions
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; init; }
    public int TimeoutSeconds { get; init; }
}
```
Rules:
- `sealed` class.
- Mutable properties set via binding use `set`; immutable ones use `init`.
- Default values prevent null-reference warnings.

### 2. Create the setup class
`...Options/{Feature}OptionsSetup.cs`:
```csharp
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Bizca.Users.Infrastructure.Cache.Options;

public sealed class CacheOptionsSetup(IConfiguration configuration) : IConfigureOptions<CacheOptions>
{
    private const string ConfigurationSectionName = nameof(CacheOptions);
    private readonly IConfiguration _configuration = configuration;

    public void Configure(CacheOptions options)
    {
        _configuration.GetSection(ConfigurationSectionName).Bind(options);
        // Override a specific field from a well-known key if needed:
        // options.Host = _configuration.GetConnectionString("cache") ?? options.Host;
    }
}
```
The section name in `appsettings.json` must match the options class name exactly (case-sensitive).

### 3. Register in DependencyInjections
```csharp
services.ConfigureOptions<CacheOptionsSetup>();
```
This is the only registration needed — do NOT call `services.Configure<CacheOptions>(...)` alongside it.

### 4. Consume in a service
```csharp
public sealed class CacheService(IOptions<CacheOptions> options)
{
    private readonly CacheOptions _options = options.Value;
}
```

### 5. Add to appsettings
`appsettings.json`:
```json
{
  "CacheOptions": {
    "Host": "localhost",
    "Port": 6379,
    "TimeoutSeconds": 5
  }
}
```
`appsettings.Development.json` can override individual properties.

## Real example
See `DatabaseOptions` + `DatabaseOptionsSetup` in `Bizca.Users.Infrastructure/Context/Options/` — it binds the `DatabaseOptions` section and additionally reads the connection string from `ConnectionStrings:database`.

## Common Rationalizations
| Rationalization | Reality |
|---|---|
| "I'll call `configuration.GetSection(...).Bind(options)` directly in `AddInfrastructure`" | The `IConfigureOptions<T>` setup class is the project's established pattern; deviating creates inconsistency and makes testing harder. |
| "I'll use `IConfiguration` directly in the service constructor" | Direct `IConfiguration` injection bypasses the options validation pipeline and makes the service harder to test. |
| "The section name doesn't need to match the class name" | The convention uses `nameof(XyzOptions)` as the section key — deviating requires documentation and causes confusion. |

## Red Flags
- `services.Configure<T>(configuration.GetSection(...))` used instead of `ConfigureOptions<TSetup>`.
- `IConfiguration` injected into domain or application layer classes.
- Options class with public constructor parameters instead of settable properties.

## Verification
- [ ] Options class is `sealed` with `set`/`init` properties.
- [ ] Setup class implements `IConfigureOptions<T>` and uses `nameof` for the section name.
- [ ] Registered via `services.ConfigureOptions<TSetup>()` in `DependencyInjections`.
- [ ] `appsettings.json` has the corresponding section.
- [ ] Build passes with `TreatWarningsAsErrors=true`.

