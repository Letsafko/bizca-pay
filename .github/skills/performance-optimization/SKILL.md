---
name: performance-optimization
description: Optimizes backend application performance. Use when performance requirements exist, when you suspect performance regressions, or when API response times or database query times need improvement. Use when profiling reveals bottlenecks that need fixing.
---

# Performance Optimization

## Overview

Measure before optimizing. Performance work without measurement is guessing — and guessing leads to premature optimization that adds complexity without improving what matters. Profile first, identify the actual bottleneck, fix it, measure again. Optimize only what measurements prove matters.

## When to Use

- Performance requirements exist in the spec (response time SLAs, throughput targets)
- Monitoring reports slow API responses or high DB query times
- You suspect a change introduced a regression
- Building features that handle large datasets or high traffic

**When NOT to use:** Don't optimize before you have evidence of a problem. Premature optimization adds complexity that costs more than the performance it gains.

## API Response Time Targets

| Endpoint type | Good | Needs Improvement | Poor |
|---|---|---|---|
| Simple read (by ID) | ≤ 50ms (p95) | ≤ 200ms | > 200ms |
| List with filters | ≤ 200ms (p95) | ≤ 500ms | > 500ms |
| Write (create/update) | ≤ 200ms (p95) | ≤ 500ms | > 500ms |

## The Optimization Workflow

```
1. MEASURE  → Establish baseline with real data
2. IDENTIFY → Find the actual bottleneck (not assumed)
3. FIX      → Address the specific bottleneck
4. VERIFY   → Measure again, confirm improvement
5. GUARD    → Add monitoring or tests to prevent regression
```

### Step 1: Measure

**Enable EF Core query logging** to see generated SQL and timing:

```json
// appsettings.Development.json
{
  "Database": {
    "EnableSensitiveDataLogging": true,
    "EnableDetailedErrors": true
  }
}
```

**Use OpenTelemetry** for distributed tracing across ASP.NET Core + EF Core:

```csharp
builder.Services.AddOpenTelemetry()
    .WithTracing(b => b
        .AddAspNetCoreInstrumentation()
        .AddEntityFrameworkCoreInstrumentation()
        .AddConsoleExporter());
```

**Use `BenchmarkDotNet`** for isolated micro-benchmarks of hot paths:

```csharp
[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80)]
public class UserQueryBenchmark
{
    [Benchmark]
    public async Task<User?> GetByExternalId()
        => await _repo.GetByExternalIdAsync(_testId);
}
```

### Where to Start Measuring

```
What is slow?
├── Single endpoint slow
│   ├── Is it a DB query?
│   │   ├── Check EF Core logs for N+1 patterns
│   │   ├── Check missing indexes (EXPLAIN ANALYZE in PostgreSQL)
│   │   └── Check tracking vs no-tracking query mode
│   ├── Is it serialization?
│   │   └── Profile allocation in BenchmarkDotNet
│   └── Is it an external HTTP call?
│       └── Check if it can be async, cached, or batched
├── All endpoints slow
│   ├── Check connection pool exhaustion (DbContext lifetime)
│   ├── Check synchronous blocking in async context (Task.Result, .Wait())
│   └── Check for missing indexes causing full table scans
└── Intermittent slowness
    ├── Check for lock contention (long-running transactions)
    ├── Check GC pressure (large allocations in hot paths)
    └── Check for connection pool saturation under load
```

### Step 2: Identify the Bottleneck

| Symptom | Likely Cause | Investigation |
|---|---|---|
| Slow single entity fetch | Missing index on FK or lookup column | `EXPLAIN ANALYZE` on the generated query |
| Slow list endpoint | N+1 EF Core queries | Check EF Core logs for repeated similar queries |
| All queries slow | `UseQueryTrackingBehavior.TrackAll` on read-only endpoints | Ensure `AsNoTracking()` or `NoTracking` global setting |
| Memory growth | Large result sets fully materialized | Add pagination, use `IAsyncEnumerable<T>` |
| High DB connections | `DbContext` not disposed, or scoped lifetime issue | Verify `AddDbContext` registers as `Scoped` |

### Step 3: Fix Common Anti-Patterns

#### N+1 Queries (EF Core)

```csharp
// BAD: N+1 — one query per user to load address
var users = await _db.Set<User>().ToListAsync();
foreach (var user in users)
{
    var address = await _db.Set<Address>()
        .FirstOrDefaultAsync(a => a.UserId == user.Id);
}

// GOOD: Single query with Include
var users = await _db.Set<User>()
    .Include(u => u.Address)
    .ToListAsync();
```

#### Missing `AsNoTracking` on Read Paths

```csharp
// BAD: Tracking is enabled globally but this is a read-only projection
var users = await _db.Set<User>().ToListAsync();

// GOOD: This project uses UseQueryTrackingBehavior.NoTracking globally
// (already configured in DependencyInjections.cs)
// If overriding on a specific query that needs tracking for an update:
var user = await _db.Set<User>()
    .AsTracking()
    .FirstOrDefaultAsync(u => u.Id == id);
```

#### Unbounded Data Fetching

```csharp
// BAD: Fetches all records
var allUsers = await _db.Set<User>().ToListAsync();

// GOOD: Paginated with cursor or offset
var users = await _db.Set<User>()
    .OrderBy(u => u.Id)
    .Skip((page - 1) * pageSize)
    .Take(pageSize)
    .ToListAsync();
```

#### Synchronous Blocking in Async Context

```csharp
// BAD: Blocks a thread pool thread; causes deadlocks under load
var user = _db.Set<User>().FirstOrDefault(u => u.Id == id);
var result = someAsyncMethod().Result;       // never do this
someAsyncMethod().Wait();                    // never do this

// GOOD: Async all the way down
var user = await _db.Set<User>().FirstOrDefaultAsync(u => u.Id == id);
var result = await someAsyncMethod();
```

#### Missing Indexes on Frequently Queried Columns

When adding a new filter or sort that doesn't follow an existing index, add an explicit EF Core index in the entity configuration:

```csharp
// AddressEntityConfiguration.cs
builder.HasIndex(static a => a.UserId)
    .HasDatabaseName("ix_address_userId");  // explicit name avoids iX_ casing bug

// For composite filters
builder.HasIndex(static u => new { u.Status, u.Active })
    .HasDatabaseName("ix_user_status_active");
```

> **Note:** `UseCamelCaseNamingConvention()` converts EF's auto-generated `IX_` prefix to `iX_`. Always specify `HasDatabaseName("ix_...")` explicitly to avoid casing inconsistency.

#### Caching Frequently-Read, Rarely-Changed Data

```csharp
// For reference data (civility refs, status refs) — use IMemoryCache
public sealed class CivilityRefCache(
    ApplicationDbContext db,
    IMemoryCache cache)
{
    private const string Key = "civility_refs";

    public async Task<IReadOnlyList<CivilityRef>> GetAllAsync()
    {
        return await cache.GetOrCreateAsync(Key, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
            return await db.Set<CivilityRef>().ToListAsync();
        }) ?? [];
    }
}
```

#### Selecting Only What You Need (Projections)

```csharp
// BAD: Materializes the full entity graph when only 2 fields are needed
var users = await _db.Set<User>()
    .Include(u => u.UserChannels)
    .Include(u => u.Address)
    .ToListAsync();

// GOOD: Project to a DTO — much less data transferred
var summaries = await _db.Set<User>()
    .Select(u => new UserSummaryDto(u.ExternalUserId.Value, u.FirstName, u.LastName))
    .ToListAsync();
```

## Database Index Guidelines

- Every FK column must have an index (EF Core does **not** auto-create them under `UseCamelCaseNamingConvention` without `HasDatabaseName`)
- Filter columns used in `WHERE` clauses on high-volume tables need indexes
- Composite indexes: put the most selective column first
- Avoid over-indexing write-heavy tables — each index slows INSERT/UPDATE

## Connection Pool

`DbContext` is registered as `Scoped` (`AddDbContext` default). This means one context per HTTP request. Do **not** register it as `Singleton` — this causes connection pool exhaustion and thread-safety issues.

```
Signs of connection pool exhaustion:
- Timeout waiting for a connection from the pool
- Errors increase under load, normal under low traffic
- DbContext disposed before async operation completed (await missing)
```

## Common Rationalizations

| Rationalization | Reality |
|---|---|
| "We'll optimize later" | Performance debt compounds. Fix obvious anti-patterns now, defer micro-optimizations. |
| "EF Core is slow by design" | EF Core with proper use is fast. N+1 queries, missing indexes, and tracking on reads are the actual culprits. |
| "This optimization is obvious" | If you didn't measure, you don't know. Profile first. |
| "AsNoTracking everywhere" | Tracking is off globally (`UseQueryTrackingBehavior.NoTracking`). Applying it redundantly is noise; omitting it where tracking IS needed for updates is a bug. |
| "The framework handles performance" | EF Core prevents some issues but cannot fix missing indexes or N+1 queries caused by your query shapes. |

## Red Flags

- Optimization without profiling data to justify it
- N+1 query patterns in EF Core (`Include` missing on related data)
- List endpoints without pagination
- EF Core tracking enabled for read-only query paths (check `UseQueryTrackingBehavior` global setting)
- `Task.Result` or `.Wait()` in async code paths
- Missing database indexes on FK or frequently-filtered columns
- Shadow FK indexes with `iX_` prefix (override with `HasDatabaseName("ix_...")`)
- No performance monitoring in production (no OpenTelemetry / APM)

## Verification

After any performance-related change:

- [ ] Before and after measurements exist (specific numbers — query time, p95 latency)
- [ ] The specific bottleneck is identified and addressed
- [ ] API response times are within defined thresholds
- [ ] No N+1 queries in new data fetching code (verified with EF Core logs)
- [ ] New database indexes have `HasDatabaseName` with lowercase `ix_` prefix
- [ ] Existing tests still pass (optimization didn't break behavior)
