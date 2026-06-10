# Bizca OpenID Connect SDK

## Overview

**Bizca.Sdk.Api.OpenId** provides JWT validation and claims enrichment middleware for API Gateways in the Bizca platform. It validates JWT tokens locally using cached JWKS keys and enriches HTTP headers with extracted claims for downstream microservices.

## Architecture

```
[Client]
   ↓ Authorization: Bearer <JWT>
[API Gateway]
   ├─→ TokenValidationMiddleware (validates JWT locally via JWKS cache)
   └─→ ClaimsEnrichmentMiddleware (injects X-User-Id, X-Roles, X-Tenant-Id headers)
       ↓
[Microservice]
   (receives enriched headers, no JWT parsing needed)
```

## Features

✅ **Local JWT validation** — No calls to Keycloak for every request
✅ **JWKS caching** — Automatic key refresh on rotation
✅ **Fail closed** — Rejects requests if validation fails or JWKS is unavailable
✅ **Claims enrichment** — Injects user context in HTTP headers
✅ **Production-ready** — Clock skew tolerance, proper error handling

## Installation

The SDK is part of `Bizca.Sdk.Api` package. Reference it in your API Gateway project:

```xml
<ProjectReference Include="..\..\sdk\Api\Api.csproj" />
```

## Configuration

### 1. Register services in `Program.cs`

```csharp
using Bizca.Sdk.Api.OpenId.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Register OpenID Connect JWT validation
builder.Services.AddBizcaOpenId(builder.Configuration);

var app = builder.Build();

// Add JWT validation and claims enrichment middleware
// MUST be called AFTER UseRouting() and BEFORE protected endpoints
app.UseRouting();
app.UseBizcaOpenId();

// Your reverse proxy or endpoints
app.MapReverseProxy();

await app.RunAsync();
```

### 2. Configure options in `appsettings.json`

```json
{
  "BizcaOpenIdOptions": {
    "Authority": "https://keycloak.example.com/realms/bizca",
    "Issuer": "https://keycloak.example.com/realms/bizca",
    "Audience": "bizca-api-gateway",
    "RequireHttpsMetadata": true,
    "ClockSkewSeconds": 300
  }
}
```

### 3. Development configuration (`appsettings.Development.json`)

```json
{
  "BizcaOpenIdOptions": {
    "Authority": "http://localhost:8080/realms/bizca",
    "Issuer": "http://localhost:8080/realms/bizca",
    "Audience": "bizca-api-gateway-dev",
    "RequireHttpsMetadata": false
  }
}
```

## Configuration Options

| Option | Type | Default | Description |
|---|---|---|---|
| `Authority` | `string` | — | OIDC authority URL for JWKS discovery |
| `Issuer` | `string` | — | Expected token issuer (`iss` claim) |
| `Audience` | `string` | — | Expected token audience (`aud` claim) |
| `RequireHttpsMetadata` | `bool` | `true` | Require HTTPS for metadata endpoint |
| `ClockSkewSeconds` | `int` | `300` | Clock skew tolerance (5 minutes) |

## How It Works

### 1. JWT Validation (`TokenValidationMiddleware`)

```csharp
https://
├── Extracts Bearer token from Authorization header
├── Fetches JWKS from {Authority}/.well-known/openid-configuration (cached)
├── Validates JWT signature, issuer, audience, expiration
├── Attaches ClaimsPrincipal to HttpContext.User
└── Rejects (401/503) if validation fails
```

**Excluded routes**:
- `/health`
- `/_health`

**Error responses**:

| Scenario | Status | Error Code |
|---|---|---|
| Missing/invalid `Authorization` header | 401 | `unauthorized` |
| Token expired | 401 | `token_expired` |
| Invalid signature/claims | 401 | `invalid_token` |
| JWKS unreachable | 503 | `service_unavailable` |

### 2. Claims Enrichment (`ClaimsEnrichmentMiddleware`)

Extracts claims from validated JWT and injects them into HTTP headers:

| JWT Claim | HTTP Header | Example |
|---|---|---|
| `sub` | `X-User-Id` | `123e4567-e89b-12d3-a456-426614174000` |
| `role` | `X-Roles` | `admin,user` (comma-separated) |
| `tenant_id` | `X-Tenant-Id` | `tenant-001` |
| `email` | `X-User-Email` | `alice@example.com` |
| `preferred_username` | `X-User-Name` | `alice` |

**Downstream microservices** receive these headers directly — no JWT parsing required.

## Usage Example

### API Gateway `Program.cs`

```csharp
using Bizca.Sdk.Api.OpenId.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add OpenID Connect validation
builder.Services.AddBizcaOpenId(builder.Configuration);

// Add reverse proxy (YARP)
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

app.UseRouting();
app.UseBizcaOpenId();  // <-- Validate JWT + enrich headers

app.MapReverseProxy();

await app.RunAsync();
```

### Microservice receiving enriched headers

```csharp
app.MapGet("/api/profile", (HttpContext context) =>
{
    var userId = context.Request.Headers["X-User-Id"].ToString();
    var roles = context.Request.Headers["X-Roles"].ToString().Split(',');
    var email = context.Request.Headers["X-User-Email"].ToString();

    return Results.Ok(new
    {
        userId,
        roles,
        email
    });
});
```

**No JWT parsing needed** — the gateway already extracted and validated the claims.

## End-to-End Flow

```
1. Client → GET /api/resource
   Authorization: Bearer eyJhbGciOiJSUzI1NiIsInR5c...

2. API Gateway
   ├─→ TokenValidationMiddleware
   │   ├─ Fetch JWKS (cached)
   │   ├─ Validate JWT signature
   │   ├─ Validate issuer, audience, expiration
   │   └─ Attach claims to HttpContext.User
   │
   └─→ ClaimsEnrichmentMiddleware
       ├─ Extract sub → X-User-Id
       ├─ Extract role → X-Roles
       └─ Extract email → X-User-Email

3. Microservice receives:
   GET /api/resource
   X-User-Id: 123e4567-e89b-12d3-a456-426614174000
   X-Roles: admin,user
   X-User-Email: alice@example.com
```

## Security Considerations

### Fail Closed

If JWT validation fails or JWKS is unavailable, the request is **rejected**:
- Missing/invalid token → **401 Unauthorized**
- JWKS fetch error → **503 Service Unavailable**

### JWKS Caching

- JWKS keys are fetched once and cached
- Automatic refresh on Keycloak key rotation
- No performance impact on request processing

### HTTPS Enforcement

In production, `RequireHttpsMetadata` must be `true`:

```json
{
  "BizcaOpenIdOptions": {
    "RequireHttpsMetadata": true
  }
}
```

Set to `false` only for local development.

### Clock Skew Tolerance

Tokens are accepted if their expiration is within the clock skew window (default: 5 minutes):

```json
{
  "BizcaOpenIdOptions": {
    "ClockSkewSeconds": 300
  }
}
```

## Alternative Configuration (Code-based)

Instead of `appsettings.json`, you can configure options in code:

```csharp
builder.Services.AddBizcaOpenId(options =>
{
    options.Authority = "https://keycloak.example.com/realms/bizca";
    options.Issuer = "https://keycloak.example.com/realms/bizca";
    options.Audience = "bizca-api-gateway";
    options.RequireHttpsMetadata = true;
    options.ClockSkewSeconds = 300;
});
```

## Troubleshooting

### 401 Unauthorized — Missing Authorization header

**Cause**: Client did not send `Authorization: Bearer <token>` header.

**Solution**: Ensure client includes the header:

```http
GET /api/resource HTTP/1.1
Authorization: Bearer eyJhbGciOiJSUzI1NiIsInR5c...
```

### 401 Unauthorized — Token expired

**Cause**: JWT `exp` claim is in the past.

**Solution**: Client must refresh the token via `/auth/refresh` endpoint.

### 401 Unauthorized — Invalid token

**Cause**: JWT signature, issuer, or audience is invalid.

**Solution**:
1. Verify `Issuer` and `Audience` in `appsettings.json` match Keycloak configuration
2. Ensure token was issued by the correct Keycloak realm
3. Check Keycloak client configuration

### 503 Service Unavailable — JWKS fetch error

**Cause**: Cannot reach `{Authority}/.well-known/openid-configuration`

**Solution**:
1. Verify Keycloak is running
2. Check network connectivity
3. Verify `Authority` URL in `appsettings.json`

## Testing

### Unit Testing

Mock `BizcaOpenIdOptions` for testing:

```csharp
var options = Options.Create(new BizcaOpenIdOptions
{
    Authority = "https://keycloak.test",
    Issuer = "https://keycloak.test",
    Audience = "test-audience",
    RequireHttpsMetadata = false
});

var middleware = new TokenValidationMiddleware(next, logger, options);
```

### Integration Testing

Use a real Keycloak instance (via Testcontainers) or mock JWKS endpoint.

## Versioning

The SDK follows semantic versioning:
- **Major**: Breaking changes
- **Minor**: New features (backward-compatible)
- **Patch**: Bug fixes

## Dependencies

| Package | Version | Purpose |
|---|---|---|
| `Microsoft.AspNetCore.Authentication.JwtBearer` | 10.0.3 | JWT validation |
| `Microsoft.IdentityModel.Protocols.OpenIdConnect` | 8.3.0 | OIDC protocol support |
| `System.IdentityModel.Tokens.Jwt` | 8.3.0 | JWT handling |

All versions centralized in `Directory.Packages.props`.

## References

- [RFC 7519 — JSON Web Token (JWT)](https://datatracker.ietf.org/doc/html/rfc7519)
- [OpenID Connect Core 1.0](https://openid.net/specs/openid-connect-core-1_0.html)
- [Microsoft IdentityModel Documentation](https://github.com/AzureAD/azure-activedirectory-identitymodel-extensions-for-dotnet)

## Authors

- **Bizca Team**

