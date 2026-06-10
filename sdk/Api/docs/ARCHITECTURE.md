# Bizca.Sdk.Api.OpenId — Architecture

## Overview

The **Bizca.Sdk.Api.OpenId** module is part of the `Bizca.Sdk.Api` package and provides JWT validation and claims enrichment for API Gateways.

## Directory Structure

```
sdk/Api/OpenId/
├── Extensions/
│   ├── ServiceCollectionExtensions.cs   # AddBizcaOpenId()
│   └── WebApplicationExtensions.cs      # UseBizcaOpenId()
│
├── Middleware/
│   ├── TokenValidationMiddleware.cs     # Local JWT validation
│   └── ClaimsEnrichmentMiddleware.cs    # HTTP header enrichment
│
├── Options/
│   ├── BizcaOpenIdOptions.cs            # Configuration model
│   └── BizcaOpenIdOptionsSetup.cs       # IOptions setup
│
├── README.md                             # Main documentation
├── GATEWAY_INTEGRATION.md                # Integration guide
└── CHANGELOG.md                          # Version history
```

## Namespace

All classes use the `Bizca.Sdk.Api.OpenId.*` namespace:

- `Bizca.Sdk.Api.OpenId.Extensions`
- `Bizca.Sdk.Api.OpenId.Middleware`
- `Bizca.Sdk.Api.OpenId.Options`

## Components

### 1. Extensions

**ServiceCollectionExtensions.cs**
- `AddBizcaOpenId(IServiceCollection, IConfiguration)` — Registers services from `appsettings.json`
- `AddBizcaOpenId(IServiceCollection, Action<BizcaOpenIdOptions>)` — Registers with code-based config

**WebApplicationExtensions.cs**
- `UseBizcaOpenId(WebApplication)` — Adds middleware to pipeline

### 2. Middleware

**TokenValidationMiddleware.cs**
- Validates JWT signature, issuer, audience, expiration
- Fetches JWKS from `{Authority}/.well-known/openid-configuration` (cached)
- Attaches `ClaimsPrincipal` to `HttpContext.User`
- Returns 401/503 on validation failure

**ClaimsEnrichmentMiddleware.cs**
- Extracts claims from validated JWT
- Injects HTTP headers for downstream microservices
- Headers: `X-User-Id`, `X-Roles`, `X-Tenant-Id`, `X-User-Email`, `X-User-Name`

### 3. Options

**BizcaOpenIdOptions.cs**
- Configuration properties: `Authority`, `Issuer`, `Audience`, `RequireHttpsMetadata`, `ClockSkewSeconds`

**BizcaOpenIdOptionsSetup.cs**
- Binds `BizcaOpenIdOptions` section from `appsettings.json` using `IConfigureOptions<T>`

## Data Flow

```
1. HTTP Request
   ↓ Authorization: Bearer <JWT>

2. TokenValidationMiddleware
   ├─ Extract Bearer token
   ├─ Fetch JWKS (cached)
   ├─ Validate JWT
   └─ Attach ClaimsPrincipal to HttpContext.User

3. ClaimsEnrichmentMiddleware
   ├─ Extract claims from HttpContext.User
   └─ Inject HTTP headers

4. Downstream Microservice
   ← X-User-Id, X-Roles, X-Tenant-Id, etc.
```

## Dependencies

| Package | Version | Purpose |
|---|---|---|
| `Microsoft.AspNetCore.Authentication.JwtBearer` | 10.0.3 | JWT validation |
| `Microsoft.IdentityModel.Protocols.OpenIdConnect` | 8.3.0 | OIDC protocol |
| `System.IdentityModel.Tokens.Jwt` | 8.3.0 | JWT handling |

All managed via `Directory.Packages.props` (centralized versioning).

## Integration Points

### API Gateway

```csharp
using Bizca.Sdk.Api.OpenId.Extensions;

builder.Services.AddBizcaOpenId(builder.Configuration);

app.UseRouting();
app.UseBizcaOpenId();
app.MapReverseProxy();
```

### Configuration

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

### Microservice

```csharp
var userId = context.Request.Headers["X-User-Id"];
var roles = context.Request.Headers["X-Roles"];
```

## Design Principles

### Fail Closed

If JWT validation fails or JWKS is unavailable, the request is **rejected**:
- Invalid token → 401 Unauthorized
- JWKS fetch error → 503 Service Unavailable

### Local Validation

JWT tokens are validated **locally** using cached JWKS — no calls to Keycloak for every request.

### Zero Trust

Downstream microservices receive **enriched headers** but still validate business rules. Headers are trusted because they come from the gateway, but the gateway is the only entry point.

### Separation of Concerns

| Responsibility | Component |
|---|---|
| JWT validation | `TokenValidationMiddleware` |
| Claims extraction | `ClaimsEnrichmentMiddleware` |
| Configuration | `BizcaOpenIdOptions` |
| Service registration | `ServiceCollectionExtensions` |
| Middleware registration | `WebApplicationExtensions` |

## Testing Strategy

### Unit Tests

- Mock `BizcaOpenIdOptions` and `HttpContext`
- Test middleware behavior in isolation
- Verify header injection logic

### Integration Tests

- Use real Keycloak instance (Testcontainers)
- Test end-to-end JWT validation
- Verify headers are forwarded to downstream microservices

### Performance Tests

- Measure JWT validation latency
- Verify JWKS caching effectiveness
- Test under high concurrent load

## Security Considerations

### HTTPS Enforcement

`RequireHttpsMetadata: true` in production ensures JWKS is fetched over HTTPS.

### Clock Skew Tolerance

Tokens are accepted if expiration is within 5 minutes (configurable) to account for server time differences.

### JWKS Caching

JWKS keys are cached to avoid performance impact. Cache is refreshed automatically on key rotation.

### Header Injection

Headers are injected by the gateway and trusted by microservices. Microservices must not accept these headers from external sources.

## Versioning

The SDK follows **Semantic Versioning**:
- Major: Breaking changes (namespace, API signature)
- Minor: New features (backward-compatible)
- Patch: Bug fixes

Current version: **1.0.0**

## References

- [README.md](./README.md) — Main documentation
- [GATEWAY_INTEGRATION.md](./GATEWAY_INTEGRATION.md) — Integration guide
- [CHANGELOG.md](./CHANGELOG.md) — Version history

## Authors

- **Bizca Team**

