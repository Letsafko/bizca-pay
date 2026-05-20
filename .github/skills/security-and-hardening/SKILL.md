---
name: security-and-hardening
description: Hardens code against vulnerabilities. Use when handling user input, authentication, data storage, or external integrations. Use when building any feature that accepts untrusted data, manages user sessions, or interacts with third-party services.
---

# Security and Hardening

## Overview

Security-first development practices for .NET web APIs. Treat every external input as hostile, every secret as sacred, and every authorization check as mandatory. Security isn't a phase — it's a constraint on every line of code that touches user data, authentication, or external systems.

## When to Use

- Building anything that accepts user input
- Implementing authentication or authorization
- Storing or transmitting sensitive data
- Integrating with external APIs or services
- Handling PII data

## The Three-Tier Boundary System

### Always Do (No Exceptions)

- **Validate all external input** at the system boundary (before it reaches domain or infrastructure)
- **Use parameterized queries** — EF Core with LINQ handles this automatically; never concatenate user input into raw SQL
- **Use HTTPS** for all external communication
- **Hash passwords** with `ASP.NET Core Identity`'s `PasswordHasher<T>` (bcrypt-based by default) — never store plaintext
- **Set security headers** (HSTS, X-Frame-Options, X-Content-Type-Options) via ASP.NET Core middleware
- **Run `dotnet list package --vulnerable`** before every release

### Ask First (Requires Human Approval)

- Adding new authentication flows or changing auth logic
- Storing new categories of sensitive data (PII, payment info)
- Adding new external service integrations
- Changing CORS configuration
- Adding file upload handlers
- Modifying rate limiting or throttling
- Granting elevated permissions or roles

### Never Do

- **Never commit secrets** to version control (connection strings, API keys, tokens)
- **Never log sensitive data** (passwords, tokens, full credit card numbers, internal exception stacks to external callers)
- **Never trust client-side validation** as a security boundary — always validate on the server
- **Never disable security headers** for convenience
- **Never expose stack traces** or internal error details in API responses
- **Never store secrets in `appsettings.json`** in source control — use `appsettings.Local.json`, User Secrets (`dotnet user-secrets`), or a vault

## OWASP Top 10 Prevention

### 1. Injection (SQL, OS Command)

EF Core with LINQ prevents SQL injection by parameterizing queries automatically. The risk surfaces when using raw SQL:

```csharp
// BAD: SQL injection via string interpolation
var users = await _db.Set<User>()
    .FromSqlRaw($"SELECT * FROM user WHERE firstName = '{firstName}'")
    .ToListAsync();

// BAD: Same with FormattableString if not using FromSqlInterpolated
var raw = $"SELECT * FROM user WHERE firstName = '{firstName}'";
var users = await _db.Database.ExecuteSqlRawAsync(raw);

// GOOD: Parameterized raw SQL
var users = await _db.Set<User>()
    .FromSqlInterpolated($"SELECT * FROM user WHERE \"firstName\" = {firstName}")
    .ToListAsync();

// BEST: Use LINQ — no raw SQL needed
var users = await _db.Set<User>()
    .Where(u => u.FirstName == firstName)
    .ToListAsync();
```

### 2. Broken Authentication

```csharp
// Password hashing — use ASP.NET Core Identity's PasswordHasher
var hasher = new PasswordHasher<User>();
string hash = hasher.HashPassword(user, plainTextPassword);

PasswordVerificationResult result = hasher.VerifyHashedPassword(user, hash, inputPassword);
if (result == PasswordVerificationResult.Failed)
    return Error.Failure("AUTH_INVALID_CREDENTIALS");

// JWTs: validate issuer, audience, lifetime, and signing key
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = configuration["Jwt:Issuer"],
            ValidAudience            = configuration["Jwt:Audience"],
            IssuerSigningKey         = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(configuration["Jwt:Secret"]!))
        };
    });
```

### 3. Broken Access Control

```csharp
// Always check authorization, not just authentication
app.MapPatch("/users/{externalUserId}", async (
    Guid externalUserId,
    UpdateUserRequest request,
    ClaimsPrincipal principal,
    IUserService userService) =>
{
    var callerExternalId = principal.GetExternalUserId(); // from JWT claim

    // Ensure caller is updating their own resource (or is admin)
    if (callerExternalId != externalUserId && !principal.IsInRole("Admin"))
        return Results.Forbid();

    var result = await userService.UpdateAsync(externalUserId, request);
    return result.IsSuccess ? Results.Ok(result.Value) : result.Error.ToHttpResult();
}).RequireAuthorization();
```

### 4. Security Misconfiguration

```csharp
// Security headers
app.UseHsts();
app.UseHttpsRedirection();
app.UseXContentTypeOptions();   // Prevents MIME sniffing
app.UseXfo(o => o.Deny());      // X-Frame-Options: DENY

// CORS — restrict to known origins
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins(configuration["AllowedOrigins"]!.Split(','))
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials());
});

// In production, never use AllowAnyOrigin() with AllowCredentials()
```

### 5. Sensitive Data Exposure

```csharp
// Never return sensitive fields in API responses
// Use dedicated response DTOs, never expose domain entities directly
public record UserResponse(
    Guid ExternalUserId,
    string FirstName,
    string LastName
    // PasswordHash, SecurityStamp, etc. are NOT included
);

// Use environment variables / User Secrets for connection strings
// appsettings.json:
// "Database": { "ConnectionString": "" }  ← empty in source control
// appsettings.Local.json (gitignored):
// "Database": { "ConnectionString": "Host=localhost;..." }

// Or via dotnet user-secrets:
// dotnet user-secrets set "Database:ConnectionString" "Host=localhost;..."
```

### 6. Vulnerable and Outdated Components

```bash
# Check for known CVEs in NuGet dependencies
dotnet list package --vulnerable --include-transitive

# Update all packages to their latest compatible versions
dotnet outdated  # (requires dotnet-outdated tool)
dotnet add package <PackageName>  # updates to latest
```

## Input Validation Patterns

### Validation at the API Boundary

Every endpoint must validate its input before invoking domain or infrastructure logic. Use minimal validation (required fields, max lengths) and let the domain enforce business rules.

```csharp
// Request record with validation attributes
public record CreateUserRequest(
    [Required, MaxLength(100)] string FirstName,
    [Required, MaxLength(100)] string LastName,
    [Required, EmailAddress, MaxLength(256)] string Email
);

// Minimal API with built-in validation
app.MapPost("/users", async (
    [FromBody] CreateUserRequest request,
    IUserService service) =>
{
    var result = await service.CreateAsync(request);
    return result.IsSuccess
        ? Results.Created($"/users/{result.Value.ExternalUserId}", result.Value)
        : result.Error.ToHttpResult();
}).AddEndpointFilter<ValidationEndpointFilter>();
```

Domain-level validation (business rules) happens inside Value Object factories and returns `Result<T>`:

```csharp
// Value objects validate their own invariants
var email = Email.Create(request.Email);
if (!email.IsSuccess)
    return email.Error;  // propagates INVALID_EMAIL error upstream
```

### Error Responses — Don't Expose Internals

```csharp
// Global exception handler — never expose stack traces in production
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode  = 500;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = 500,
            Title  = "An unexpected error occurred.",
            // Do NOT include exception.Message or StackTrace here
        });
    });
});
```

## Rate Limiting

ASP.NET Core 7+ has built-in rate limiting middleware:

```csharp
// Global rate limit
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit      = 100,
                Window           = TimeSpan.FromMinutes(15),
            }));

    // Stricter limit for auth endpoints
    options.AddPolicy("auth", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window      = TimeSpan.FromMinutes(15),
            }));
});

app.UseRateLimiter();

// Apply stricter policy to specific endpoints
app.MapPost("/auth/login", LoginHandler)
   .RequireRateLimiting("auth");
```

## Secrets Management

```
appsettings.json           → Committed (no secrets — only keys with empty values)
appsettings.Development.json → Committed (non-sensitive dev defaults)
appsettings.Local.json     → NOT committed (real local connection strings)
User Secrets               → NOT committed (dotnet user-secrets set "Key" "Value")
CI secrets                 → GitHub Secrets (accessible via ${{ secrets.NAME }])
Production secrets         → Azure Key Vault / AWS Secrets Manager / HashiCorp Vault
```

**.gitignore must include:**
```
appsettings.Local.json
*.pfx
*.pem
*.key
```

**Always check before committing:**
```bash
git diff --cached | Select-String -Pattern "password|secret|connectionstring|apikey|token" -CaseSensitive:$false
```

## Security Review Checklist

```markdown
### Authentication
- [ ] Passwords hashed with PasswordHasher<T> (ASP.NET Core Identity)
- [ ] JWT tokens validated (issuer, audience, lifetime, signing key)
- [ ] Auth endpoints have rate limiting

### Authorization
- [ ] Every protected endpoint calls `.RequireAuthorization()`
- [ ] Users can only access their own resources (check ExternalUserId from claims)
- [ ] Admin actions require admin role verification

### Input
- [ ] All user input validated at the API boundary
- [ ] EF Core LINQ used for queries (no raw SQL with user input)
- [ ] Response DTOs never expose PasswordHash, SecurityStamp, or internal IDs

### Data
- [ ] No secrets in source control (appsettings.json, code, comments)
- [ ] appsettings.Local.json in .gitignore
- [ ] Sensitive fields excluded from API responses

### Infrastructure
- [ ] HTTPS enforced (UseHttpsRedirection + UseHsts)
- [ ] Security headers configured (X-Frame-Options, X-Content-Type-Options)
- [ ] CORS restricted to known origins (no `AllowAnyOrigin()`)
- [ ] NuGet dependencies audited: `dotnet list package --vulnerable`
- [ ] Exception handler does not expose stack traces in production
```

## Common Rationalizations

| Rationalization | Reality |
|---|---|
| "This is an internal API, security doesn't matter" | Internal APIs get compromised. Attackers target the weakest link. |
| "We'll add security later" | Security retrofitting is 10x harder than building it in. Add it now. |
| "EF Core handles injection" | EF Core LINQ does. `FromSqlRaw` with string interpolation does not. |
| "It's just a prototype" | Prototypes become production. Security habits from day one. |
| "The connection string is just for dev" | `appsettings.Local.json` exists for this. Don't commit the real value to `appsettings.json`. |

## Red Flags

- `FromSqlRaw` or `ExecuteSqlRaw` with string-interpolated user input
- Secrets in `appsettings.json` committed to source control
- API endpoints missing `.RequireAuthorization()`
- CORS configured with `AllowAnyOrigin()` + `AllowCredentials()`
- No rate limiting on authentication endpoints
- Exception details exposed to callers in production responses
- NuGet packages with known critical vulnerabilities unremediated

## Verification

After implementing security-relevant code:

- [ ] `dotnet list package --vulnerable` shows no critical or high vulnerabilities
- [ ] No secrets in source code or git history
- [ ] All user input validated at system boundaries
- [ ] Authentication and authorization checked on every protected endpoint
- [ ] Security headers present in response (verify with curl or browser DevTools)
- [ ] Error responses don't expose internal stack traces or connection details
- [ ] Rate limiting active on auth endpoints
