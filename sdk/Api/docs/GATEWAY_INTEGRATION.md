# Gateway Integration Guide

## Quick Start

This guide shows how to integrate **Bizca.Sdk.Api.OpenId** into your API Gateway for JWT validation and claims enrichment.

## Prerequisites

- .NET 10 SDK
- Keycloak instance
- API Gateway project (ASP.NET Core Minimal API or MVC)

## Step 1: Reference the SDK

Add a reference to `Bizca.Sdk.Api` in your API Gateway project:

```xml
<ProjectReference Include="..\..\sdk\Api\Api.csproj" />
```

## Step 2: Configure Services

In your `Program.cs`:

```csharp
using Bizca.Sdk.Api.OpenId.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add OpenID Connect JWT validation
builder.Services.AddBizcaOpenId(builder.Configuration);

// Add your reverse proxy or other services
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

// Configure middleware pipeline
app.UseRouting();
app.UseBizcaOpenId();  // MUST be after UseRouting()

// Map your endpoints or reverse proxy
app.MapReverseProxy();

await app.RunAsync();
```

## Step 3: Configure Options

In `appsettings.json`:

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

For local development (`appsettings.Development.json`):

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

## Step 4: Test the Integration

### 1. Obtain a JWT token

```bash
curl -X POST http://localhost:5100/auth/token \
  -H "Content-Type: application/json" \
  -d '{
    "grant_type": "client_credentials"
  }'
```

Response:
```json
{
  "access_token": "eyJhbGciOiJSUzI1NiIsInR5c...",
  "token_type": "Bearer",
  "expires_in": 3600
}
```

### 2. Call your API Gateway

```bash
curl -X GET http://localhost:5000/api/users/me \
  -H "Authorization: Bearer eyJhbGciOiJSUzI1NiIsInR5c..."
```

The gateway will:
1. Validate the JWT
2. Extract claims
3. Inject headers (`X-User-Id`, `X-Roles`, etc.)
4. Forward to downstream microservice

## Middleware Order

**Critical**: The middleware order matters!

```csharp
var app = builder.Build();

// 1. Routing (MUST be first)
app.UseRouting();

// 2. JWT validation + claims enrichment
app.UseBizcaOpenId();

// 3. Your endpoints or reverse proxy
app.MapReverseProxy();
```

**Common mistake**:
```csharp
// ❌ WRONG - UseBizcaOpenId() before UseRouting()
app.UseBizcaOpenId();
app.UseRouting();
```

## Reverse Proxy Configuration (YARP)

Example `ReverseProxy` section in `appsettings.json`:

```json
{
  "ReverseProxy": {
    "Routes": {
      "users-route": {
        "ClusterId": "users-cluster",
        "Match": {
          "Path": "/api/users/{**catch-all}"
        },
        "Transforms": [
          {
            "RequestHeadersCopy": true
          }
        ]
      }
    },
    "Clusters": {
      "users-cluster": {
        "Destinations": {
          "destination1": {
            "Address": "http://localhost:5200"
          }
        }
      }
    }
  }
}
```

**Important**: `RequestHeadersCopy: true` ensures enriched headers are forwarded.

## Enriched Headers

The middleware injects these headers for downstream microservices:

| Header | Source | Example |
|---|---|---|
| `X-User-Id` | JWT `sub` claim | `123e4567-e89b-12d3-a456-426614174000` |
| `X-Roles` | JWT `role` claim | `admin,user` |
| `X-Tenant-Id` | JWT `tenant_id` claim | `tenant-001` |
| `X-User-Email` | JWT `email` claim | `alice@example.com` |
| `X-User-Name` | JWT `preferred_username` claim | `alice` |

## Microservice Code

Your downstream microservices can read these headers directly:

```csharp
app.MapGet("/api/users/me", (HttpContext context) =>
{
    var userId = context.Request.Headers["X-User-Id"].ToString();
    var roles = context.Request.Headers["X-Roles"].ToString().Split(',');

    return Results.Ok(new
    {
        userId,
        roles
    });
});
```

**No JWT parsing needed** — the gateway already did it!

## Health Check Exclusion

These routes are **not validated** by the middleware:

- `/health`
- `/_health`

All other routes require a valid JWT token.

## Error Responses

| Scenario | HTTP Status | Error Code |
|---|---|---|
| Missing `Authorization` header | 401 | `unauthorized` |
| Token expired | 401 | `token_expired` |
| Invalid signature/claims | 401 | `invalid_token` |
| JWKS unavailable | 503 | `service_unavailable` |

Example error response:

```json
{
  "error": "token_expired",
  "message": "The access token has expired"
}
```

## Production Checklist

- [ ] `RequireHttpsMetadata` is `true`
- [ ] `Authority` and `Issuer` point to production Keycloak
- [ ] `Audience` matches Keycloak client ID
- [ ] Middleware is after `UseRouting()`
- [ ] Reverse proxy copies request headers
- [ ] Health checks are excluded from validation

## Troubleshooting

### 401 Unauthorized — "Missing or invalid Authorization header"

**Cause**: No `Authorization` header or wrong format.

**Solution**: Include the header:

```http
Authorization: Bearer <your-jwt-token>
```

### 401 Unauthorized — "Token has expired"

**Cause**: JWT `exp` claim is in the past.

**Solution**: Refresh the token via `/auth/refresh`.

### 503 Service Unavailable — "Authentication service unavailable"

**Cause**: Cannot reach JWKS endpoint.

**Solution**:
1. Check Keycloak is running
2. Verify `Authority` URL
3. Check network connectivity

### Headers not forwarded to microservice

**Cause**: Reverse proxy not copying headers.

**Solution**: Add transform in YARP configuration:

```json
{
  "Transforms": [
    {
      "RequestHeadersCopy": true
    }
  ]
}
```

## Advanced Configuration

### Code-based configuration

```csharp
builder.Services.AddBizcaOpenId(options =>
{
    options.Authority = builder.Configuration["Keycloak:Authority"]!;
    options.Issuer = builder.Configuration["Keycloak:Issuer"]!;
    options.Audience = builder.Configuration["Keycloak:Audience"]!;
    options.RequireHttpsMetadata = builder.Environment.IsProduction();
});
```

### Custom claim mapping

Modify `ClaimsEnrichmentMiddleware.cs` to add custom headers.

### Skip validation for specific routes

Modify `TokenValidationMiddleware.cs` to add routes to the exclusion list.

## References

- [YARP Documentation](https://microsoft.github.io/reverse-proxy/)
- [Keycloak Documentation](https://www.keycloak.org/documentation)
- [Bizca OpenID SDK README](./README.md)

## Support

For issues or questions, contact the Bizca Team or open an issue in the repository.

