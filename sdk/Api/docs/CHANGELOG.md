# Changelog

All notable changes to the Bizca OpenID Connect SDK will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2026-05-20

### Added
- Initial release of Bizca OpenID Connect SDK
- `TokenValidationMiddleware` for local JWT validation using JWKS cache
- `ClaimsEnrichmentMiddleware` for extracting claims and injecting HTTP headers
- `BizcaOpenIdOptions` configuration class
- `AddBizcaOpenId()` extension method for service registration
- `UseBizcaOpenId()` extension method for middleware registration
- Support for the following enriched headers:
  - `X-User-Id` (from `sub` claim)
  - `X-Roles` (from `role` claim)
  - `X-Tenant-Id` (from `tenant_id` claim)
  - `X-User-Email` (from `email` claim)
  - `X-User-Name` (from `preferred_username` claim)
- Fail closed security model (rejects on validation failure)
- Health check route exclusion (`/health`, `/_health`)
- Clock skew tolerance configuration
- HTTPS metadata enforcement option
- Comprehensive documentation (README, GATEWAY_INTEGRATION)

### Security
- Fail closed: rejects requests if JWT validation fails
- Fail closed: rejects requests if JWKS is unavailable (503)
- Local JWT validation — no calls to Keycloak for every request
- JWKS caching with automatic refresh on key rotation
- Configurable clock skew tolerance (default: 5 minutes)
- HTTPS enforcement for metadata endpoints (default: enabled)

## [Unreleased]

### Planned
- Multi-tenant support with tenant-specific JWKS endpoints
- Custom claim extractors via configuration
- Metrics and telemetry for JWT validation performance
- Rate limiting integration
- Support for additional token types (reference tokens, opaque tokens)

---

## Migration from Bizca.Sdk.OpenId (standalone project)

### Breaking Changes

**Namespace change**:
- Old: `Bizca.Sdk.OpenId.*`
- New: `Bizca.Sdk.Api.OpenId.*`

**Project reference**:
- Old: `<ProjectReference Include="..\..\sdk\OpenId\Bizca.Sdk.OpenId.csproj" />`
- New: `<ProjectReference Include="..\..\sdk\Api\Api.csproj" />`

**Using statements**:
```csharp
// Old
using Bizca.Sdk.OpenId.Extensions;

// New
using Bizca.Sdk.Api.OpenId.Extensions;
```

### Migration Steps

1. **Remove old project reference**:
   ```bash
   dotnet remove reference ../../sdk/OpenId/Bizca.Sdk.OpenId.csproj
   ```

2. **Add new project reference**:
   ```bash
   dotnet add reference ../../sdk/Api/Api.csproj
   ```

3. **Update using statements**:
   ```csharp
   // Replace
   using Bizca.Sdk.OpenId.Extensions;

   // With
   using Bizca.Sdk.Api.OpenId.Extensions;
   ```

4. **No configuration changes needed** — `appsettings.json` remains unchanged.

5. **Rebuild**:
   ```bash
   dotnet build
   ```

---

## Version History

| Version | Release Date | .NET Version | Breaking Changes |
|---|---|---|---|
| 1.0.0 | 2026-05-20 | .NET 10 | Initial release |

---

## Contributing

See [CONTRIBUTING.md](../../CONTRIBUTING.md) for guidelines on how to contribute to this project.

## License

This project is licensed under the MIT License - see the [LICENSE](../../LICENSE) file for details.

