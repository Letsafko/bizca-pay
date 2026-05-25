# Architecture Bizca avec .NET Aspire

## Vue d'ensemble

L'architecture Bizca utilise **.NET Aspire** pour orchestrer l'ensemble des microservices et dépendances.

```
┌────────────────────────────────────────────────────────────────┐
│              Bizca.Services.AppHost (.NET Aspire)              │
│                                                                │
│  ┌──────────────────────────────────────────────────────────┐ │
│  │                      Dashboard Aspire                     │ │
│  │              https://localhost:17000                      │ │
│  └──────────────────────────────────────────────────────────┘ │
│                                                                │
│  ┌───────────┐  ┌────────────┐  ┌──────────┐  ┌───────────┐  │
│  │ Keycloak  │  │ PostgreSQL │  │ OpenID   │  │  Users    │  │
│  │  (Docker) │  │  (Docker)  │  │   API    │  │   API     │  │
│  │           │  │            │  │          │  │           │  │
│  │  :8080    │  │  :dynamic  │  │ :dynamic │  │ :dynamic  │  │
│  └─────┬─────┘  └──────┬─────┘  └────┬─────┘  └─────┬─────┘  │
│        │               │             │              │         │
└────────┼───────────────┼─────────────┼──────────────┼─────────┘
         │               │             │              │
         ▼               ▼             ▼              ▼
    Authentification  Base de    Gestion      Gestion
    & Identité       données     tokens       utilisateurs
```

## Stack technologique

| Composant | Technologie | Version | Description |
|---|---|---|---|
| **Orchestration** | .NET Aspire | 13.1.1 | Orchestration des services et dépendances |
| **Runtime** | .NET | 10.0 | Runtime d'exécution |
| **API Framework** | ASP.NET Core Minimal API | 10.0 | Framework REST API |
| **Database** | PostgreSQL | latest | Base de données relationnelle |
| **Identity Provider** | Keycloak | latest | Serveur d'authentification OAuth2/OIDC |
| **OpenAPI** | Scalar + Microsoft.AspNetCore.OpenApi | 2.6.0 / 10.0.3 | Documentation API interactive |

## Services orchestrés

### 1. Keycloak (Identity Provider)

```csharp
.AddContainer("keycloak", "quay.io/keycloak/keycloak", "latest")
.WithHttpEndpoint(port: 8080, targetPort: 8080, name: "http")
.WithEnvironment("KEYCLOAK_ADMIN", "admin")
.WithEnvironment("KEYCLOAK_ADMIN_PASSWORD", "admin")
.WithArgs("start-dev")
```

- **Image** : `quay.io/keycloak/keycloak:latest`
- **Port** : `8080`
- **Admin** : `admin` / `admin`
- **Mode** : Development (start-dev)
- **Volume** : Bind mount `keycloak-data`

### 2. PostgreSQL (Database)

```csharp
.AddPostgres("postgres")
.WithDataVolume("postgres-data")
.WithPgWeb()
```

- **Image** : PostgreSQL (via Aspire)
- **Database** : `bizca-users`
- **Port** : Dynamique
- **Volume** : `postgres-data`
- **PgWeb** : Interface web activée

### 3. Bizca.OpenId.Api (Authentication Service)

```csharp
.AddProject<Bizca_OpenId_Api>("openid-api")
.WaitFor(keycloak)
```

- **Framework** : ASP.NET Core Minimal API
- **Responsabilité** : Interface avec Keycloak (token, refresh, logout)
- **Dépendances** : Keycloak
- **Port** : Dynamique (assigné par Aspire)

**Endpoints** :
- `POST /auth/token` — Échange authorization code pour access token
- `POST /auth/refresh` — Rafraîchit un access token
- `POST /auth/logout` — Révoque un token
- `GET /health` — Health check

### 4. Bizca.Users.Api (Users Service)

```csharp
.AddProject<Bizca_Users_Api>("users-api")
.WithReference(database, connectionName: "database")
.WaitFor(database)
```

- **Framework** : ASP.NET Core Minimal API
- **Responsabilité** : Gestion des utilisateurs
- **Dépendances** : PostgreSQL
- **Port** : Dynamique (assigné par Aspire)
- **Database** : `bizca-users`

## Diagramme de dépendances

```
Keycloak (8080)
    │
    ▼
OpenID API (dynamic)

PostgreSQL (dynamic)
    │
    ▼
Users API (dynamic)
```

**Ordre de démarrage** :
1. Keycloak + PostgreSQL (parallèle)
2. OpenID API (attend Keycloak)
3. Users API (attend PostgreSQL)

## Démarrage de la stack complète

### Prérequis

```powershell
# Installer Aspire workload
dotnet workload install aspire

# Vérifier Docker Desktop
docker --version
```

### Lancer tous les services

```powershell
# Depuis la racine du projet
dotnet run --project microservices/Bizca.Services.AppHost/Bizca.Services.AppHost.csproj
```

### Accès aux interfaces

| Service | URL | Credentials |
|---|---|---|
| **Aspire Dashboard** | `https://localhost:17000` | — |
| **Keycloak Admin** | `http://localhost:8080` | `admin` / `admin` |
| **PgWeb** | `http://localhost:{port}` | — (voir dashboard) |
| **OpenID API** | `http://localhost:{port}/scalar/v1` | — (voir dashboard) |
| **Users API** | `http://localhost:{port}/scalar/v1` | — (voir dashboard) |

## Configuration Keycloak

### Étapes initiales

1. **Démarrer l'AppHost**
   ```powershell
   dotnet run --project microservices/Bizca.Services.AppHost/Bizca.Services.AppHost.csproj
   ```

2. **Accéder à Keycloak** : `http://localhost:8080`

3. **Se connecter** : `admin` / `admin`

4. **Créer le realm `bizca`**
   - Administration Console → Create Realm
   - Realm name : `bizca`
   - Enabled : ON

5. **Créer le client `bizca-backend`**
   - Clients → Create client
   - Client type : OpenID Connect
   - Client ID : `bizca-backend`
   - Client authentication : ON
   - Authorization : OFF
   - Root URL : `http://localhost:*`
   - Valid redirect URIs : `http://localhost:*`

6. **Récupérer le Client Secret**
   - Clients → `bizca-backend` → Credentials
   - Copier le Client Secret

7. **Configurer OpenID API**
   - Éditer `security/Bizca.OpenId.Api/appsettings.Development.json`
   - Mettre à jour `KeycloakOptions:ClientSecret`

## Volumes persistants

Les données sont persistées dans des volumes Docker :

```powershell
# Lister les volumes
docker volume ls | findstr bizca

# Supprimer les volumes (reset complet)
docker volume rm postgres-data
docker volume rm keycloak-data
```

## Ajout d'un nouveau service

1. **Créer le projet API** dans `microservices/{service}/src/`

2. **Ajouter la référence** dans `Bizca.Services.AppHost.csproj` :
   ```xml
   <ProjectReference Include="..\{service}\src\Bizca.{Service}.Api\Bizca.{Service}.Api.csproj" />
   ```

3. **Enregistrer dans `AppHost.cs`** :
   ```csharp
   builder
       .AddProject<Bizca_{Service}_Api>("service-name")
       .WithReference(database) // si besoin de DB
       .WaitFor(dependency); // si dépendance
   ```

4. **Compiler et exécuter** :
   ```powershell
   dotnet build microservices/Bizca.Services.AppHost/Bizca.Services.AppHost.csproj
   dotnet run --project microservices/Bizca.Services.AppHost/Bizca.Services.AppHost.csproj
   ```

## Monitoring et observabilité

Le Dashboard Aspire fournit :

- ✅ **Logs en temps réel** de tous les services
- ✅ **Métriques** (CPU, mémoire, requêtes HTTP)
- ✅ **Traces distribuées** (OpenTelemetry)
- ✅ **État de santé** de chaque service
- ✅ **Endpoints** et ports dynamiques

## Structure des projets

```
bizca/
├── microservices/
│   ├── Bizca.Services.AppHost/        ← Orchestration Aspire
│   │   ├── AppHost.cs
│   │   ├── Bizca.Services.AppHost.csproj
│   │   └── README.md
│   │
│   └── user/
│       └── src/
│           ├── Bizca.Users.Api/
│           ├── Bizca.Users.Domain/
│           └── Bizca.Users.Infrastructure/
│
├── security/
│   └── Bizca.OpenId.Api/              ← API d'authentification
│
└── sdk/
    ├── Api/                            ← SDK OpenAPI commun
    └── OpenId/                         ← SDK validation JWT
```

## Références

- [.NET Aspire Documentation](https://learn.microsoft.com/dotnet/aspire/)
- [Keycloak Documentation](https://www.keycloak.org/documentation)
- [PostgreSQL Documentation](https://www.postgresql.org/docs/)

## Auteurs

- **Bizca Team**

