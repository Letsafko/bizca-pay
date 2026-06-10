# Bizca OpenID Connect — Architecture et Intégration

Ce document décrit l'architecture complète de l'authentification OpenID Connect dans Bizca, composée de deux éléments distincts :

1. **security/Bizca.OpenId.Api** — Microservice dédié qui encapsule Keycloak
2. **sdk/OpenId (Bizca.Sdk.OpenId)** — SDK consommé uniquement par l'API Gateway

---

## Table des matières

- [Architecture globale](#architecture-globale)
- [Composant 1 : Bizca.OpenId.Api](#composant-1--bizcaopenidapi)
- [Composant 2 : Bizca.Sdk.OpenId](#composant-2--bizcasdkopenid)
- [Flux d'authentification end-to-end](#flux-dauthentification-end-to-end)
- [Configuration](#configuration)
- [Déploiement](#déploiement)

---

## Architecture globale

```
[Client]
   ↓
   ├── POST /auth/token    →  security/Bizca.OpenId.Api  ←→  [Keycloak]
   ├── POST /auth/refresh  →  security/Bizca.OpenId.Api  ←→  [Keycloak]
   └── POST /auth/logout   →  security/Bizca.OpenId.Api  ←→  [Keycloak]

[Client]
   ↓
   GET /api/resource  →  [API Gateway]
                         (consomme sdk/OpenId)
                              ↓ valide JWT localement (JWKS cache)
                              ↓ enrichit les headers
                         [Microservices]
                         reçoivent X-User-Id, X-Roles, X-Tenant-Id
```

### Séparation des responsabilités

| Composant | Responsabilité | Communique avec Keycloak ? |
|---|---|---|
| **Bizca.OpenId.Api** | Token exchange, refresh, logout, userinfo | ✅ Oui (seul composant) |
| **Bizca.Sdk.OpenId** (Gateway) | Validation JWT locale + enrichissement headers | ❌ Non (cache JWKS local) |
| **Microservices** | Logique métier, consomment les headers enrichis | ❌ Non |

---

## Composant 1 : Bizca.OpenId.Api

### Structure

```
security/Bizca.OpenId.Api/
├── Endpoints/
│   ├── TokenEndpoint.cs       # POST /auth/token
│   ├── RefreshEndpoint.cs     # POST /auth/refresh
│   └── LogoutEndpoint.cs      # POST /auth/logout
│
├── Keycloak/
│   ├── KeycloakClient.cs      # Appels HTTP à Keycloak
│   └── JwksCache.cs           # Cache local des clés publiques JWKS
│
├── Options/
│   ├── KeycloakOptions.cs
│   └── KeycloakOptionsSetup.cs
│
├── Program.cs
├── appsettings.json
└── appsettings.Development.json
```

### Endpoints exposés

| Endpoint | Méthode | Description |
|---|---|---|
| `/auth/token` | POST | Échange authorization code pour un access token (Authorization Code + PKCE) ou obtient un token client (Client Credentials) |
| `/auth/refresh` | POST | Rafraîchit un access token expiré via un refresh token |
| `/auth/logout` | POST | Révoque un token (access ou refresh) |
| `/health` | GET | Health check |

### KeycloakClient

Encapsule tous les appels HTTP à Keycloak :

- `ExchangeCodeForTokenAsync(code, redirectUri, codeVerifier)` — Authorization Code flow
- `GetClientCredentialsTokenAsync()` — Client Credentials flow
- `RefreshTokenAsync(refreshToken)` — Refresh token
- `RevokeTokenAsync(token, tokenTypeHint)` — Revoke (logout)
- `GetUserInfoAsync(accessToken)` — User info endpoint

### JwksCache

Cache local thread-safe des clés publiques JWKS :

- Refresh automatique après expiration (configurable, défaut : 3600s)
- Refresh forcé sur demande via `RefreshAsync()`
- Fail closed : si Keycloak est injoignable, le cache reste valide jusqu'à expiration

### Configuration (appsettings.json)

```json
{
  "KeycloakOptions": {
    "Authority": "https://keycloak.example.com/realms/bizca",
    "ClientId": "bizca-backend",
    "ClientSecret": "REPLACE_WITH_ACTUAL_SECRET",
    "Realm": "bizca",
    "Scopes": "openid profile email",
    "JwksCacheDurationSeconds": 3600,
    "HttpTimeoutSeconds": 30
  }
}
```

**⚠️ Ne jamais committer `ClientSecret` en clair** — utiliser User Secrets en développement, Azure Key Vault en production.

### Packages NuGet utilisés

- `Microsoft.AspNetCore.OpenApi` — Spécification OpenAPI native
- `Scalar.AspNetCore` — UI Swagger/OpenAPI (développement uniquement)
- `Microsoft.AspNetCore.Authentication.JwtBearer` — Validation JWT
- `Microsoft.IdentityModel.Protocols.OpenIdConnect` — Découverte OIDC et JWKS
- `System.IdentityModel.Tokens.Jwt` — Manipulation JWT

---

## Composant 2 : Bizca.Sdk.OpenId

### Structure

```
sdk/OpenId/
├── Extensions/
│   ├── ServiceCollectionExtensions.cs   # AddBizcaOpenId()
│   └── WebApplicationExtensions.cs      # UseBizcaOpenId()
│
├── Middleware/
│   ├── TokenValidationMiddleware.cs     # Valide JWT entrant
│   └── ClaimsEnrichmentMiddleware.cs    # Injecte X-User-Id, X-Roles, X-Tenant-Id
│
└── Options/
    ├── BizcaOpenIdOptions.cs
    └── BizcaOpenIdOptionsSetup.cs
```

### TokenValidationMiddleware

Valide JWT localement via clés JWKS cachées :

- ✅ Vérifie la signature (RSA256)
- ✅ Vérifie l'issuer (`iss` claim)
- ✅ Vérifie l'audience (`aud` claim)
- ✅ Vérifie l'expiration (`exp` claim) avec clock skew tolérance
- ✅ Fail closed : si validation échoue → 401 ; si JWKS indisponible → 503

Routes exclues de la validation : `/health`, `/_health`

### ClaimsEnrichmentMiddleware

Injecte les claims JWT dans les headers HTTP forwarded aux microservices :

| Claim JWT | Header HTTP |
|---|---|
| `sub` (subject) | `X-User-Id` |
| `role` ou `ClaimTypes.Role` | `X-Roles` (valeurs séparées par virgules) |
| `tenant_id` ou `organization_id` | `X-Tenant-Id` |
| `email` | `X-User-Email` |
| `preferred_username` | `X-User-Name` |

Les microservices n'ont **jamais** à parser le JWT — ils reçoivent directement les headers de confiance.

### Configuration (appsettings.json)

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

### Intégration dans l'API Gateway

#### Program.cs

```csharp
using Bizca.Sdk.OpenId.Extensions;

var builder = WebApplication.CreateBuilder(args);

// 1. Enregistrer les services Bizca OpenID Connect
builder.Services.AddBizcaOpenId(builder.Configuration);

// Autres services (reverse proxy, YARP, etc.)
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

// 2. Activer le middleware d'authentification OpenID
//    APRÈS UseRouting(), AVANT les endpoints protégés
app.UseRouting();
app.UseBizcaOpenId();  // <-- Valide JWT + enrichit headers

// 3. Reverse proxy vers les microservices
app.MapReverseProxy();

await app.RunAsync().ConfigureAwait(false);
```

---

## Flux d'authentification end-to-end

### 1. Client obtient un token

```bash
curl -X POST http://localhost:5100/auth/token \
  -H "Content-Type: application/json" \
  -d '{
    "grant_type": "authorization_code",
    "code": "AUTH_CODE_FROM_OIDC_FLOW",
    "redirect_uri": "http://localhost:3000/callback",
    "code_verifier": "PKCE_VERIFIER"
  }'
```

**Réponse :**

```json
{
  "access_token": "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9...",
  "token_type": "Bearer",
  "expires_in": 3600,
  "refresh_token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refresh_expires_in": 86400,
  "scope": "openid profile email",
  "id_token": "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

### 2. Client appelle une API protégée via la Gateway

```bash
curl -X GET http://localhost:5000/api/users/me \
  -H "Authorization: Bearer eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9..."
```

**Ce qui se passe :**

1. La gateway reçoit la requête
2. `TokenValidationMiddleware` valide le JWT localement (JWKS cache)
3. `ClaimsEnrichmentMiddleware` injecte les headers enrichis
4. La gateway forward au microservice Users avec :

```http
GET /api/users/me HTTP/1.1
Host: localhost:5200
Authorization: Bearer eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9...
X-User-Id: 123e4567-e89b-12d3-a456-426614174000
X-Roles: admin,user
X-User-Email: alice@example.com
X-User-Name: alice
X-Tenant-Id: tenant-001
```

Le microservice Users n'a **jamais** à parser le JWT — il lit directement `X-User-Id` et `X-Roles`.

### 3. Rafraîchir le token

```bash
curl -X POST http://localhost:5100/auth/refresh \
  -H "Content-Type: application/json" \
  -d '{
    "refresh_token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
  }'
```

### 4. Se déconnecter (révoquer le token)

```bash
curl -X POST http://localhost:5100/auth/logout \
  -H "Content-Type: application/json" \
  -d '{
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "token_type_hint": "refresh_token"
  }'
```

---

## Configuration

### Development (local)

**Bizca.OpenId.Api** (`appsettings.Development.json`) :

```json
{
  "KeycloakOptions": {
    "Authority": "http://localhost:8080/realms/bizca",
    "ClientId": "bizca-backend-dev",
    "ClientSecret": "dev-secret-not-for-production"
  }
}
```

**API Gateway** (`appsettings.Development.json`) :

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

### Production

- `RequireHttpsMetadata = true`
- `ClientSecret` depuis Azure Key Vault ou secret Kubernetes
- `Authority` et `Issuer` pointent vers Keycloak en production avec HTTPS

---

## Déploiement

### Docker Compose (développement)

```yaml
version: '3.8'
services:
  keycloak:
    image: quay.io/keycloak/keycloak:latest
    ports:
      - "8080:8080"
    environment:
      KEYCLOAK_ADMIN: admin
      KEYCLOAK_ADMIN_PASSWORD: admin
    command: start-dev

  bizca-openid-api:
    build: ./security/Bizca.OpenId.Api
    ports:
      - "5100:8080"
    environment:
      KeycloakOptions__Authority: http://keycloak:8080/realms/bizca
      KeycloakOptions__ClientId: bizca-backend-dev
      KeycloakOptions__ClientSecret: dev-secret
    depends_on:
      - keycloak

  api-gateway:
    build: ./gateway
    ports:
      - "5000:8080"
    environment:
      BizcaOpenIdOptions__Authority: http://keycloak:8080/realms/bizca
      BizcaOpenIdOptions__Issuer: http://keycloak:8080/realms/bizca
      BizcaOpenIdOptions__Audience: bizca-api-gateway-dev
      BizcaOpenIdOptions__RequireHttpsMetadata: "false"
    depends_on:
      - bizca-openid-api
```

### Kubernetes

- Secret pour `ClientSecret`
- ConfigMap pour `Authority`, `Issuer`, `Audience`
- Health checks sur `/health`

---

## Sécurité

### Fail Closed

Si Keycloak est indisponible :

- **Bizca.OpenId.Api** : retourne 503 Service Unavailable (impossible d'obtenir un token)
- **API Gateway (SDK)** : retourne 503 si JWKS est expiré ET ne peut être rafraîchi (cache valide → validation continue)

### Rotation des clés

Keycloak peut changer ses clés de signature. Le cache JWKS détecte automatiquement les nouvelles clés via le endpoint `.well-known/jwks.json` et les met en cache.

### Secrets

- `ClientSecret` : **jamais en clair** dans `appsettings.json`
- Utiliser User Secrets en développement : `dotnet user-secrets set "KeycloakOptions:ClientSecret" "mon-secret"`
- En production : Azure Key Vault, AWS Secrets Manager, Kubernetes Secrets

---

## Packages NuGet centralisés

Tous les packages sont définis dans `Directory.Packages.props` (pas de `Version=` dans les `.csproj`) :

```xml
<ItemGroup Label="Security">
  <PackageVersion Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="10.0.3"/>
  <PackageVersion Include="Microsoft.IdentityModel.Protocols.OpenIdConnect" Version="8.3.0"/>
  <PackageVersion Include="System.IdentityModel.Tokens.Jwt" Version="8.3.0"/>
</ItemGroup>
```

---

## Tests

### Bizca.OpenId.Api

- Tests d'intégration avec Testcontainers (Keycloak en container)
- Tests de contrat pour les endpoints `/auth/*`

### Bizca.Sdk.OpenId

- Tests unitaires des middlewares avec `HttpContext` mocké
- Tests d'intégration de l'API Gateway avec validation JWT end-to-end

---

## Références

- [RFC 6749 — OAuth 2.0](https://datatracker.ietf.org/doc/html/rfc6749)
- [RFC 7636 — PKCE](https://datatracker.ietf.org/doc/html/rfc7636)
- [OpenID Connect Core 1.0](https://openid.net/specs/openid-connect-core-1_0.html)
- [Keycloak Documentation](https://www.keycloak.org/documentation)
- [Microsoft IdentityModel Documentation](https://github.com/AzureAD/azure-activedirectory-identitymodel-extensions-for-dotnet)

---

## Auteurs

- **Bizca Team**

