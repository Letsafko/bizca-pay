---
name: shipping-and-launch
description: Prepares production launches. Use when preparing to deploy to production. Use when you need a pre-launch checklist, when setting up monitoring, when planning a staged rollout, or when you need a rollback strategy.
---

# Shipping and Launch

## Overview

Ship with confidence. The goal is not just to deploy — it's to deploy safely, with monitoring in place, a rollback plan ready, and a clear understanding of what success looks like. Every launch should be reversible, observable, and incremental.

## When to Use

- Deploying a feature to production for the first time
- Releasing a significant change to users
- Migrating data or infrastructure
- Opening a beta or early access program
- Any deployment that carries risk (all of them)

## The Pre-Launch Checklist

### Code Quality

- [ ] All tests pass: `dotnet test`
- [ ] Build succeeds with no warnings: `dotnet build --configuration Release`
- [ ] Code reviewed and approved
- [ ] No TODO comments that should be resolved before launch
- [ ] No debugging statements or temporary logging in production code
- [ ] Error handling covers expected failure modes
- [ ] All domain invariants enforced through Value Object factories and `Result<T>` chains

### Security

- [ ] No secrets in `appsettings.json` or source control
- [ ] `dotnet list package --vulnerable` shows no critical or high vulnerabilities
- [ ] Input validation on all user-facing endpoints
- [ ] Authentication and authorization checked on every protected endpoint
- [ ] Security headers configured (HSTS, X-Frame-Options, etc.)
- [ ] Rate limiting on authentication endpoints
- [ ] CORS configured to specific origins (not wildcard)

### Performance

- [ ] No N+1 queries in critical paths (verified with EF Core logs)
- [ ] List endpoints have pagination
- [ ] Database queries have appropriate indexes (FK columns, filter columns)
- [ ] No synchronous blocking in async code paths (`Task.Result`, `.Wait()`)
- [ ] EF Core global `NoTracking` behavior is set for read-only paths

### Infrastructure

- [ ] Environment variables / secrets set in production environment
- [ ] EF Core migrations applied: `dotnet ef database update`
- [ ] Health check endpoint exists and responds
- [ ] Logging and error reporting configured
- [ ] SSL/TLS configured for production endpoint
- [ ] Docker image built and tagged from the correct commit

### Documentation

- [ ] README updated with any new setup requirements
- [ ] API documentation current (Swagger/OpenAPI matches implementation)
- [ ] ADRs written for any architectural decisions made during this feature
- [ ] Changelog updated

## Feature Flag Strategy

Ship behind feature flags to decouple deployment from release:

```csharp
// Simple feature flag via configuration
if (featureFlags.IsEnabled("new-user-channel-flow"))
{
    return await _newChannelHandler.HandleAsync(command);
}
return await _legacyChannelHandler.HandleAsync(command);
```

**Feature flag lifecycle:**

```
1. DEPLOY with flag OFF     → Code is in production but inactive
2. ENABLE for team/beta     → Internal testing in production environment
3. GRADUAL ROLLOUT          → 5% → 25% → 50% → 100% of users
4. MONITOR at each stage    → Watch error rates, performance, user feedback
5. CLEAN UP                 → Remove flag and dead code path after full rollout
```

**Rules:**
- Every feature flag has an owner and an expiration date
- Clean up flags within 2 weeks of full rollout
- Don't nest feature flags (creates exponential combinations)
- Test both flag states (on and off) in integration tests

## Staged Rollout

### The Rollout Sequence

```
1. DEPLOY to staging
   └── Run full test suite against staging environment
   └── Manual smoke test of critical flows

2. DEPLOY to production (feature flag OFF)
   └── Verify deployment succeeded (health check endpoint returns 200)
   └── Check error monitoring (no new errors)

3. ENABLE for team (flag ON for internal users)
   └── Team uses the feature in production
   └── 24-hour monitoring window

4. CANARY rollout (flag ON for 5% of users)
   └── Monitor error rates, latency, user behavior
   └── Compare metrics: canary vs. baseline
   └── 24-48 hour monitoring window
   └── Advance only if all thresholds pass (see table below)

5. GRADUAL increase (25% → 50% → 100%)
   └── Same monitoring at each step
   └── Ability to roll back to previous percentage at any point

6. FULL rollout (flag ON for all users)
   └── Monitor for 1 week
   └── Clean up feature flag
```

### Rollout Decision Thresholds

| Metric | Advance (green) | Hold and investigate (yellow) | Roll back (red) |
|---|---|---|---|
| HTTP error rate | Within 10% of baseline | 10–100% above baseline | >2× baseline |
| P95 API latency | Within 20% of baseline | 20–50% above baseline | >50% above baseline |
| Business metrics | Neutral or positive | Decline <5% (may be noise) | Decline >5% |

### When to Roll Back

Roll back immediately if:
- Error rate increases by more than 2× baseline
- P95 latency increases by more than 50%
- User-reported issues spike
- Data integrity issues detected
- Security vulnerability discovered

## Monitoring and Observability

### What to Monitor

```
Application metrics:
├── HTTP error rate (total and by endpoint)
├── Response time (P50, P95, P99)
├── Request volume
└── Key business metrics

Infrastructure metrics:
├── CPU and memory utilization
├── Database connection pool usage
├── PostgreSQL query time and lock waits
└── Queue depth (if applicable)
```

### Error Reporting — ASP.NET Core

```csharp
// Global exception handler — logs internally, returns safe ProblemDetails to callers
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exceptionFeature = context.Features.Get<IExceptionHandlerFeature>();
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

        if (exceptionFeature?.Error is not null)
        {
            logger.LogError(exceptionFeature.Error,
                "Unhandled exception for {Method} {Path}",
                context.Request.Method,
                context.Request.Path);
        }

        context.Response.StatusCode  = 500;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = 500,
            Title  = "An unexpected error occurred.",
            // StackTrace, exception.Message → NOT included in response
        });
    });
});
```

### Health Check Endpoint

```csharp
// Program.cs
builder.Services.AddHealthChecks()
    .AddNpgSql(connectionString, name: "postgres");

app.MapHealthChecks("/health");
```

### Post-Launch Verification

In the first hour after launch:

```
1. Check /health endpoint returns 200
2. Check error monitoring (no new error types spiking)
3. Check latency dashboard (no regression vs. baseline)
4. Test the critical API flow manually (e.g., create a user, verify persistence)
5. Verify logs are flowing and structured correctly
6. Confirm rollback mechanism works (dry run if possible)
```

## Rollback Strategy

Every deployment needs a rollback plan before it happens:

```markdown
## Rollback Plan for [Feature/Release]

### Trigger Conditions
- HTTP error rate > 2× baseline
- P95 API latency > [X]ms
- User reports of [specific issue]

### Rollback Steps
1. Disable feature flag (if applicable — instant, no redeploy needed)
   OR
2. Redeploy previous Docker image tag
3. Verify rollback: /health endpoint, error monitoring
4. Communicate: notify team of rollback

### Database Considerations
- EF Core migration [MigrationName] can be rolled back with:
  `dotnet ef database update <PreviousMigrationName>`
- Data inserted by the new feature: [preserved / to be cleaned up by script]

### Time to Rollback
- Feature flag: < 1 minute
- Redeploy previous image: < 5 minutes
- EF Core migration rollback: < 15 minutes (if down-migration exists)
```

## See Also

- For security pre-launch checks, see `security-and-hardening`
- For performance pre-launch checks, see `performance-optimization`

## Common Rationalizations

| Rationalization | Reality |
|---|---|
| "It works in staging, it'll work in production" | Production has different data, traffic patterns, and edge cases. Monitor after deploy. |
| "We don't need feature flags for this" | Every feature benefits from a kill switch. Even "simple" changes can break things. |
| "Monitoring is overhead" | Not having monitoring means you discover problems from user complaints instead of dashboards. |
| "We'll add monitoring later" | Add it before launch. You can't debug what you can't see. |
| "Rolling back is admitting failure" | Rolling back is responsible engineering. Shipping a broken feature is the failure. |

## Red Flags

- Deploying without a rollback plan
- No monitoring or error reporting in production
- Big-bang releases (everything at once, no staging)
- Feature flags with no expiration or owner
- No one monitoring the deploy for the first hour
- Applying EF Core migrations to production without testing them on staging first
- "It's Friday afternoon, let's ship it"

## Verification

Before deploying:

- [ ] Pre-launch checklist completed (all sections green)
- [ ] Feature flag configured (if applicable)
- [ ] Rollback plan documented
- [ ] Monitoring dashboards set up
- [ ] EF Core migrations verified on staging
- [ ] Team notified of deployment

After deploying:

- [ ] /health endpoint returns 200
- [ ] Error rate is normal
- [ ] Latency is normal
- [ ] Critical API flow works end-to-end
- [ ] Logs are flowing
- [ ] Rollback tested or verified ready
