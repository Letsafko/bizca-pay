# Performance Checklist

Quick reference checklist for .NET backend performance. Use alongside the `performance-optimization` skill.

## Table of Contents

- [API Response Time Targets](#api-response-time-targets)
- [EF Core / Database Checklist](#ef-core--database-checklist)
- [Async & Threading Checklist](#async--threading-checklist)
- [Caching Checklist](#caching-checklist)
- [Infrastructure Checklist](#infrastructure-checklist)
- [Measurement Commands](#measurement-commands)
- [Common Anti-Patterns](#common-anti-patterns)

---

## API Response Time Targets

| Percentile | Target | Action if exceeded |
|------------|--------|--------------------|
| p50 | < 50ms | Baseline |
| p95 | < 200ms | Investigate slow queries, add caching |
| p99 | < 500ms | Profile with `dotnet-trace`, check indexes |

---

## EF Core / Database Checklist

### Queries

- [ ] No N+1 patterns — use `Include` / `ThenInclude` or explicit joins
- [ ] Read-only queries use `AsNoTracking()` (or rely on global `UseQueryTrackingBehavior.NoTracking`)
- [ ] List endpoints paginated: `.Skip(offset).Take(limit)` — never `ToListAsync()` on unbounded sets
- [ ] Projections used for read models: `.Select(u => new UserDto { ... })` instead of loading full entities
- [ ] Bulk inserts/updates use `ExecuteUpdateAsync` / `ExecuteDeleteAsync` (EF Core 7+) instead of per-entity loops
- [ ] `SaveChangesAsync(CancellationToken)` called once per unit of work, not in a loop

### Indexes

- [ ] Indexes exist on all FK columns and frequently filtered/sorted columns
- [ ] Index names use explicit `HasDatabaseName("ix_<table>_<column>")` — avoids `iX_` casing bug from `UseCamelCaseNamingConvention()`
- [ ] Composite indexes ordered by selectivity (most selective column first)
- [ ] Slow query log enabled in PostgreSQL (`log_min_duration_statement = 200`)
- [ ] `EXPLAIN ANALYZE` run on queries > 100ms

### Connection & Schema

- [ ] Connection pooling configured (Npgsql default pool size reviewed for load)
- [ ] `UseNpgsql()` registered once via `IServiceCollection` — never `new NpgsqlConnection()` manually
- [ ] Migrations reviewed before applying — no full table rewrites on large tables without `CONCURRENTLY`

---

## Async & Threading Checklist

- [ ] No `.Result`, `.Wait()`, or `.GetAwaiter().GetResult()` blocking calls on async code
- [ ] No `async void` methods (except event handlers) — use `async Task`
- [ ] `CancellationToken` threaded through all async methods to DB and HTTP calls
- [ ] CPU-bound work offloaded to `Task.Run(...)` — not blocking request threads
- [ ] `IHostedService` / `BackgroundService` used for recurring background work (not `Task.Run` at startup)
- [ ] `SemaphoreSlim` or `Channel<T>` used where concurrent access to shared state is needed (not `lock` + async)

---

## Caching Checklist

- [ ] `IMemoryCache` used for short-lived, node-local reference data (lookup tables, config)
- [ ] Cache entries have explicit expiry (`AbsoluteExpirationRelativeToNow` or `SlidingExpiration`)
- [ ] Cache keys are stable and deterministic — no user-specific data in shared cache entries
- [ ] `IDistributedCache` (Redis) used when multiple instances share state
- [ ] HTTP `Cache-Control` headers set on GET endpoints where data is not user-specific
- [ ] Health check endpoint (`/health`) excluded from caching

---

## Infrastructure Checklist

- [ ] Health check endpoint registered: `app.MapHealthChecks("/health")`
- [ ] PostgreSQL health check registered: `services.AddHealthChecks().AddNpgsql(...)`
- [ ] Response compression enabled: `app.UseResponseCompression()` (`gzip`/`brotli`)
- [ ] `Kestrel` limits configured (`MaxRequestBodySize`, keep-alive timeout)
- [ ] OpenTelemetry traces + metrics exported (Aspire dashboard or external collector)
- [ ] Horizontal scaling validated: no in-process state that breaks under multiple instances

---

## Measurement Commands

```bash
# Run all tests with timing
dotnet test --logger "console;verbosity=normal"

# Profile a running service (attach by PID)
dotnet-trace collect --process-id <PID> --output trace.nettrace

# Analyze the trace
dotnet-trace report trace.nettrace --report topN

# Watch live counters (GC, thread pool, requests/sec)
dotnet-counters monitor --process-id <PID> \
  System.Runtime \
  Microsoft.AspNetCore.Hosting \
  Npgsql

# Microbenchmarks (add BenchmarkDotNet to a dedicated project)
dotnet run -c Release --project Bizca.Users.Benchmarks

# Check PostgreSQL slow queries (psql)
SELECT query, mean_exec_time, calls
FROM pg_stat_statements
ORDER BY mean_exec_time DESC
LIMIT 20;

# Identify missing indexes
SELECT relname, seq_scan, idx_scan
FROM pg_stat_user_tables
WHERE seq_scan > idx_scan
ORDER BY seq_scan DESC;
```

---

## Common Anti-Patterns

| Anti-Pattern | Impact | Fix |
|---|---|---|
| EF Core N+1 | Linear DB round-trips as data grows | `Include` / projection / batch load |
| `ToListAsync()` without pagination | Memory exhaustion, timeouts | `.Skip().Take()` on every list query |
| Missing index on FK / filter column | Full table scan, slow as rows grow | `HasIndex(...).HasDatabaseName("ix_...")` |
| `.Result` / `.Wait()` on async code | Thread pool starvation under load | `await` all the way up the call chain |
| `new NpgsqlConnection()` per request | Connection pool exhausted | Inject `ApplicationDbContext` via DI |
| `SaveChangesAsync()` inside a loop | N round-trips to DB | Batch changes, single `SaveChangesAsync()` |
| In-memory cache without expiry | Stale data, memory growth | Always set `AbsoluteExpirationRelativeToNow` |
| `iX_` shadow index names | Wrong casing in migration SQL | Explicit `HasDatabaseName("ix_<table>_<col>")` |
| Full entity load for read-only DTOs | Unnecessary columns + tracking overhead | `.Select(x => new Dto { ... })` projections |
