# Documentation OpenAPI — Bizca.OpenId.Api

## Configuration avec Bizca.Sdk.Api

Le projet **Bizca.OpenId.Api** utilise les extensions OpenAPI standardisées du SDK **Bizca.Sdk.Api**, qui fournissent une configuration centralisée pour tous les microservices Bizca.

### Packages utilisés

```xml
<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="10.0.3" />
<PackageReference Include="Scalar.AspNetCore" Version="2.6.0" />
```

Ces packages sont référencés via le projet `Bizca.Sdk.Api` qui fournit les extensions :

```xml
<ProjectReference Include="..\..\sdk\Api\Api.csproj" />
```

### Configuration dans Program.cs

```csharp
using Bizca.Sdk.Api.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// OpenAPI support with Bizca extensions
builder.Services.AddBizcaOpenApi(builder.Configuration);

var app = builder.Build();

// OpenAPI / Scalar UI in development only
if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Local"))
{
    app.UseBizcaOpenApi();
}
```

### Configuration dans appsettings.json

```json
{
  "OpenApiOptions": {
    "Title": "Bizca OpenID API",
    "Description": "Authentication and token management API for Bizca platform, interfacing with Keycloak.",
    "Versions": [ "v1" ],
    "EnableBearerSecurity": false,
    "BearerSchemeName": "Bearer",
    "BearerFormat": "JWT"
  }
}
```

**Note** : `EnableBearerSecurity` est à `false` car ce microservice **génère** les tokens, il n'est pas lui-même protégé par authentification Bearer.

### Accès à l'interface

En environnement de développement ou local :

- **Scalar UI** : `http://localhost:{port}/scalar/v1`
- **Spécification OpenAPI JSON** : `http://localhost:{port}/openapi/v1.json`

### Extensions Bizca.Sdk.Api

Les méthodes d'extensions fournies par le SDK :

| Méthode | Description |
|---|---|
| `AddBizcaOpenApi(IConfiguration)` | Enregistre les services OpenAPI versionnés, Scalar et le versioning d'API |
| `UseBizcaOpenApi()` | Mappe les endpoints OpenAPI et Scalar (uniquement en dev/local) |

**Avantages** :
- ✅ Configuration centralisée pour tous les microservices
- ✅ Support du versioning d'API (ex: v1, v2)
- ✅ Transformation automatique de la documentation (titres, descriptions, sécurité)
- ✅ Activé uniquement en développement et environnement local
- ✅ Support Bearer JWT optionnel via configuration

### Endpoints documentés

Tous les endpoints sont documentés avec :

- `.WithSummary()` — Résumé court
- `.WithDescription()` — Description détaillée
- `.WithTags()` — Groupement par catégorie
- `.Produces<T>()` — Types de réponse attendus
- `.ProducesProblem()` — Codes d'erreur possibles

**Exemple :**

```csharp
app.MapPost("/auth/token", HandleAsync)
    .WithName("GetToken")
    .WithTags("Authentication")
    .WithSummary("Exchange authorization code for access token")
    .WithDescription("Supports Authorization Code + PKCE and Client Credentials flows.")
    .Produces<TokenResponse>(StatusCodes.Status200OK)
    .ProducesProblem(StatusCodes.Status400BadRequest)
    .ProducesProblem(StatusCodes.Status500InternalServerError);
```

### Endpoints exposés

| Endpoint | Méthode | Tag | Description |
|---|---|---|---|
| `/auth/token` | POST | Authentication | Échange authorization code pour access token (Authorization Code + PKCE, Client Credentials) |
| `/auth/refresh` | POST | Authentication | Rafraîchit un access token avec un refresh token |
| `/auth/logout` | POST | Authentication | Révoque un token (logout) |
| `/health` | GET | Health | Health check |

### Environnement de production

En production, l'interface Scalar **n'est pas exposée** (condition `IsDevelopment() || IsEnvironment("Local")`).

Seule la spécification OpenAPI peut être générée au build-time via :

```xml
<OpenApiGenerateDocuments>true</OpenApiGenerateDocuments>
<OpenApiDocumentsDirectory>$(MSBuildThisFileDirectory)artifacts/openapi</OpenApiDocumentsDirectory>
```

Le fichier généré se trouve dans `security/Bizca.OpenId.Api/artifacts/openapi/Bizca.OpenId.Api.json`.

### Personnalisation

Pour modifier les options OpenAPI, éditer `appsettings.json` :

```json
{
  "OpenApiOptions": {
    "Title": "Nouveau titre",
    "Description": "Nouvelle description",
    "Versions": [ "v1", "v2" ],
    "EnableBearerSecurity": true,
    "BearerSchemeName": "Bearer",
    "BearerFormat": "JWT"
  }
}
```

### Support du versioning

Pour ajouter plusieurs versions d'API :

1. **Configuration** : Ajouter les versions dans `appsettings.json` :

```json
{
  "OpenApiOptions": {
    "Versions": [ "v1", "v2" ]
  }
}
```

2. **Endpoints** : Marquer les endpoints avec la version appropriée :

```csharp
app.MapPost("/auth/token", HandleAsync)
    .HasApiVersion(new ApiVersion(1.0))
    .WithTags("Authentication");
```

Chaque version générera une spécification dédiée : `/openapi/v1.json`, `/openapi/v2.json`.

### Validation

```powershell
# Compiler le projet
dotnet build security/Bizca.OpenId.Api/Bizca.OpenId.Api.csproj

# Lancer l'API en développement
dotnet run --project security/Bizca.OpenId.Api/Bizca.OpenId.Api.csproj

# Accéder à Scalar UI
start http://localhost:5100/scalar/v1
```

### Différences avec la configuration manuelle

| Avant (manuel) | Après (Bizca.Sdk.Api) |
|---|---|
| `builder.Services.AddOpenApi()` | `builder.Services.AddBizcaOpenApi(builder.Configuration)` |
| `app.MapOpenApi()` + `app.MapScalarApiReference()` | `app.UseBizcaOpenApi()` |
| Configuration en code | Configuration dans `appsettings.json` |
| Pas de versioning d'API | Support versioning intégré |
| Titre hardcodé | Titre configurable par environnement |

### Références

- [Scalar Documentation](https://github.com/scalar/scalar)
- [ASP.NET Core OpenAPI](https://learn.microsoft.com/aspnet/core/fundamentals/openapi)
- [API Versioning](https://github.com/dotnet/aspnet-api-versioning)


