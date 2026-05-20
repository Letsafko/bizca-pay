# Security Checklist

Quick reference for ASP.NET Core security. Use alongside the `security-and-hardening` skill.

## Table of Contents

- [Pre-Commit Checks](#pre-commit-checks)
- [Authentication](#authentication)
- [Authorization](#authorization)
- [Input Validation](#input-validation)
- [Security Headers](#security-headers)
- [CORS Configuration](#cors-configuration)
- [Data Protection](#data-protection)
- [Dependency Security](#dependency-security)
- [Error Handling](#error-handling)
- [OWASP Top 10 Quick Reference](#owasp-top-10-quick-reference)

---

## Pre-Commit Checks

- [ ] No secrets in committed files: `git diff --cached | Select-String -Pattern "password|secret|api_key|token" -CaseSensitive:$false`
- [ ] `.gitignore` covers: `appsettings.Local.json`, `*.pfx`, `*.key`, `.env`
- [ ] `appsettings.json` contains only non-secret placeholders — real values in User Secrets or environment variables
- [ ] `appsettings.Local.json` is in `.gitignore` and never committed

---

## Authentication

- [ ] Passwords hashed with `IPasswordHasher<T>` (ASP.NET Core Identity) — never MD5, SHA1, or plain storage
- [ ] JWT tokens validated: signature (`AddJwtBearer`), expiration (`exp`), issuer (`iss`), audience (`aud`)
- [ ] Rate limiting applied to authentication endpoints: `app.UseRateLimiter()` + `.RequireRateLimiting("auth")`
- [ ] Password reset / verification tokens are time-limited (≤ 1 hour) and single-use
- [ ] Account lockout configured after repeated failures (`LockoutOptions` via Identity)
- [ ] MFA supported for sensitive operations (recommended)

---

## Authorization

- [ ] Every protected minimal API endpoint has `.RequireAuthorization()` or `[Authorize]`
- [ ] Endpoints grouped with `MapGroup(...).RequireAuthorization()` — individual routes opt out with `.AllowAnonymous()`
- [ ] Every resource access scoped to the authenticated user's identity (prevents IDOR)
- [ ] Admin-only operations checked with role/policy: `.RequireAuthorization("AdminPolicy")`
- [ ] JWT `sub` / `nameidentifier` claim used consistently to identify the calling user

---

## Input Validation

- [ ] All user input validated at API boundaries (FluentValidation validators or inline minimal API validation)
- [ ] Validation errors returned as `Result<T>` failures with `ErrorType.Validation` — never silently swallowed
- [ ] String lengths constrained (min/max via `RuleFor(...).Length(...)`)
- [ ] Numeric ranges validated (`InclusiveBetween`)
- [ ] Email, URL, and date formats validated with proper validators (`.EmailAddress()`, custom regex)
- [ ] File uploads: MIME type restricted, size limited (`MaxRequestBodySize`), content verified
- [ ] EF Core queries use LINQ only — no unsafe string interpolation in `FromSqlRaw` / `ExecuteSqlRaw`
- [ ] URL redirects validated against an allowlist (prevent open redirect)

---

## Security Headers

```csharp
// Program.cs
app.Use(async (context, next) =>
{
    context.Response.Headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";
    context.Response.Headers["X-Content-Type-Options"]    = "nosniff";
    context.Response.Headers["X-Frame-Options"]           = "DENY";
    context.Response.Headers["Referrer-Policy"]           = "strict-origin-when-cross-origin";
    context.Response.Headers["Permissions-Policy"]        = "camera=(), microphone=(), geolocation=()";
    await next();
});
app.UseHsts(); // Adds HSTS in non-Development environments
```

---

## CORS Configuration

```csharp
// Restrictive (recommended)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins("https://app.bizca.com")
              .AllowCredentials()
              .WithMethods("GET", "POST", "PUT", "PATCH", "DELETE")
              .WithHeaders("Content-Type", "Authorization"));
});

// NEVER in production:
policy.AllowAnyOrigin().AllowCredentials(); // Not allowed — throws at runtime
policy.AllowAnyOrigin();                    // Allows all origins — use only for public unauthenticated APIs
```

---

## Data Protection

- [ ] Secrets stored in User Secrets (local dev), environment variables, or Azure Key Vault — never in `appsettings.json`
- [ ] `IConfiguration` binds secrets via `ConfigureOptions<T>` + validation (`ValidateDataAnnotations()`)
- [ ] Sensitive fields excluded from response DTOs and logs (`passwordHash`, `resetToken`, tokens, PII)
- [ ] Sensitive data not logged — use structured logging with destructuring policies to strip sensitive properties
- [ ] HTTPS enforced: `app.UseHttpsRedirection()` + `UseHsts()`
- [ ] `DataProtectionProvider` used for any tokens/cookies that must survive server restarts (not default in-memory keys)

---

## Dependency Security

```bash
# Check for known CVEs in NuGet packages
dotnet list package --vulnerable

# Check for outdated packages
dotnet list package --outdated

# Check transitive dependencies too
dotnet list package --vulnerable --include-transitive

# Pin versions centrally in Directory.Packages.props
# Never use floating versions like 6.* in production
```

---

## Error Handling

```csharp
// Program.cs — generic error handler for production
app.UseExceptionHandler(err => err.Run(async ctx =>
{
    ctx.Response.StatusCode  = 500;
    ctx.Response.ContentType = "application/problem+json";
    await ctx.Response.WriteAsJsonAsync(new ProblemDetails
    {
        Status = 500,
        Title  = "An unexpected error occurred.",
        Type   = "https://tools.ietf.org/html/rfc9110#section-15.6.1"
    });
}));

// NEVER expose in production:
// - Exception.Message (may contain SQL / file paths)
// - Exception.StackTrace
// - DbUpdateException details (exposes table/column names)
```

---

## OWASP Top 10 Quick Reference

| # | Vulnerability | Prevention in ASP.NET Core |
|---|---|---|
| 1 | Broken Access Control | `.RequireAuthorization()` on every endpoint, ownership check per resource |
| 2 | Cryptographic Failures | HTTPS + HSTS, `IPasswordHasher<T>`, User Secrets / Key Vault |
| 3 | Injection | LINQ queries (parameterized by EF Core), FluentValidation allowlists |
| 4 | Insecure Design | `spec-driven-development` skill, threat modeling before implementation |
| 5 | Security Misconfiguration | Security headers middleware, no stack traces in production, `dotnet list package --vulnerable` |
| 6 | Vulnerable Components | `dotnet list package --vulnerable --include-transitive`, pin versions in `Directory.Packages.props` |
| 7 | Auth Failures | `IPasswordHasher<T>`, rate limiting, JWT validation, account lockout |
| 8 | Data Integrity Failures | Optimistic concurrency (`IVersionedEntity`), signed artifacts in CI |
| 9 | Logging Failures | Structured logging (Serilog/OTEL), destructuring policies to strip PII, audit trail for sensitive ops |
| 10 | SSRF | `IHttpClientFactory` + allowlist outbound URLs, validate all user-supplied URLs |
