# Bizca Services AppHost

## Vue d'ensemble

**Bizca.Services.AppHost** est le projet d'orchestration .NET Aspire qui gère tous les services et dépendances de la plateforme Bizca.

Il orchestre :
-  **Keycloak** — Serveur d'authentification et d'identité
- ️ **PostgreSQL** — Base de données pour le service Users
-  **Bizca.OpenId.Api** — API d'authentification (interface avec Keycloak)
-  **Bizca.Users.Api** — API de gestion des utilisateurs

## Architecture

```
Bizca.Services.AppHost
├── Keycloak (quay.io/keycloak/keycloak:latest)
│   ├── Port: 8080 (HTTP)
│   ├── Admin: admin / admin
│   └── Mode: start-dev
│
├── PostgreSQL
│   ├── Database: bizca-users
│   ├── Volume: postgres-data
│   └── PgWeb: enabled
│
├── OpenID API
│   ├── Service: Bizca.OpenId.Api
│   └── Depends: Keycloak
│
└── Users API
    ├── Service: Bizca.Users.Api
    ├── Database: bizca-users
    └── Depends: PostgreSQL
```

## Démarrage

### Prérequis

- .NET 10 SDK
- Docker Desktop (pour Keycloak et PostgreSQL)
- Aspire Workload : `dotnet workload install aspire`

### Lancer l'orchestration

```powershell
# Depuis la racine du projet
dotnet run --project microservices/Bizca.Services.AppHost/Bizca.Services.AppHost.csproj
```

Ou via Visual Studio / Rider :
- Définir `Bizca.Services.AppHost` comme projet de démarrage
- Appuyer sur F5

### Accès aux services

Une fois l'AppHost démarré, accédez au **Dashboard Aspire** :

- **Dashboard** : `https://localhost:17000` ou `http://localhost:15000`

Le dashboard affiche :
- L'état de tous les services
- Les logs en temps réel
- Les métriques et traces OpenTelemetry
- Les endpoints disponibles

### Services exposés

| Service | Description | Endpoint |
|---|---|---|
| **Keycloak** | Serveur d'authentification | `http://localhost:8080` |
| **PgWeb** | Interface web PostgreSQL | `http://localhost:{port}` (dynamique) |
| **OpenID API** | API d'authentification | `http://localhost:{port}` (dynamique) |
| **Users API** | API de gestion des utilisateurs | `http://localhost:{port}` (dynamique) |

**Note** : Les ports des APIs sont assignés dynamiquement par Aspire. Consultez le dashboard pour les ports exacts.

## Configuration Keycloak

### Accès admin Keycloak

- **URL** : `http://localhost:8080`
- **Username** : `admin`
- **Password** : `admin`

### Configuration du realm Bizca

1. Se connecter à la console admin Keycloak
2. Créer un nouveau realm : `bizca`
3. Créer un client :
   - **Client ID** : `bizca-backend`
   - **Client authentication** : ON
   - **Authorization** : OFF
   - **Valid redirect URIs** : `http://localhost:*`
4. Récupérer le **Client Secret** dans l'onglet "Credentials"
5. Mettre à jour `appsettings.Development.json` de `Bizca.OpenId.Api` avec le secret

## Volumes persistants

Les données sont stockées dans des volumes Docker :

- **PostgreSQL** : Volume anonyme (non persistant par défaut)
- **Keycloak** : `keycloak-data` (bind mount vers `./keycloak-data/`)

### Volume PostgreSQL

Par défaut, PostgreSQL utilise un **volume anonyme** qui sera détruit quand le conteneur s'arrête. Cela évite les problèmes de données corrompues entre les redémarrages.

**Pour activer la persistance** (production uniquement) :

Modifier `AppHost.cs` :
```csharp
var postgres = builder
    .AddPostgres("postgres")
    .WithDataVolume("bizca-postgres-data")  // Volume nommé = persistant
    .WithPgAdmin();
```

**⚠️ Important** : Les volumes nommés peuvent causer des problèmes "unhealthy" s'ils contiennent des données corrompues. En cas de problème, supprimez le volume :

```powershell
docker volume rm bizca-postgres-data
```

## Dépendances entre services

```
┌─────────────┐
│  Keycloak   │
└──────┬──────┘
       │
       ▼
┌─────────────┐      ┌──────────────┐
│ OpenID API  │      │  PostgreSQL  │
└─────────────┘      └──────┬───────┘
                            │
                            ▼
                     ┌──────────────┐
                     │  Users API   │
                     └──────────────┘
```

- **OpenID API** attend que Keycloak soit prêt avant de démarrer
- **Users API** attend que PostgreSQL soit prêt avant de démarrer

## Ajout d'un nouveau service

Pour ajouter un nouveau microservice à l'orchestration :

1. **Ajouter la référence de projet** dans `Bizca.Services.AppHost.csproj` :

```xml
<ProjectReference Include="..\{service-folder}\Bizca.{Service}.Api.csproj" />
```

2. **Enregistrer le service** dans `AppHost.cs` :

```csharp
builder
    .AddProject<Bizca_{Service}_Api>("service-name")
    .WithReference(database, connectionName: "database") // si besoin de DB
    .WaitFor(database); // si besoin d'attendre une ressource
```

3. **Recompiler** :

```powershell
dotnet build microservices/Bizca.Services.AppHost/Bizca.Services.AppHost.csproj
```

## Personnalisation

### Changer les ports Keycloak

Modifier `AppHost.cs` :

```csharp
var keycloak = builder
    .AddContainer("keycloak", "quay.io/keycloak/keycloak", "latest")
    .WithHttpEndpoint(port: 8080, targetPort: 8080, name: "http") // Changer ici
```

### Ajouter une variable d'environnement

```csharp
builder
    .AddProject<Bizca_Users_Api>("users-api")
    .WithEnvironment("MY_VAR", "my-value");
```

### Ajouter une base de données supplémentaire

```csharp
var newDatabase = postgres.AddDatabase("new-db-resource", "new-database-name");

builder
    .AddProject<Bizca_Service_Api>("service-api")
    .WithReference(newDatabase, connectionName: "new-db-resource");
```

## Troubleshooting

### Keycloak ne démarre pas

- Vérifier que Docker Desktop est lancé
- Vérifier que le port 8080 n'est pas déjà utilisé : `netstat -ano | findstr :8080`
- Consulter les logs dans le Dashboard Aspire

### PostgreSQL ne démarre pas


### Les services ne se connectent pas à Keycloak

- Vérifier que Keycloak est bien démarré (Dashboard Aspire)
- Vérifier la configuration dans `appsettings.Development.json` de `Bizca.OpenId.Api`
- S'assurer que le realm `bizca` et le client `bizca-backend` existent dans Keycloak

## Références

- [.NET Aspire Documentation](https://learn.microsoft.com/dotnet/aspire/)
- [Keycloak Documentation](https://www.keycloak.org/documentation)
- [PostgreSQL Docker Image](https://hub.docker.com/_/postgres)

## Auteurs

- **Bizca Team**

