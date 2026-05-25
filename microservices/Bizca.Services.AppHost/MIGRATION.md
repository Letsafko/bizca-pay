# Migration vers Bizca.Services.AppHost

## Contexte

Le projet `Bizca.Users.AppHost` a été déplacé et renommé en `Bizca.Services.AppHost` pour orchestrer tous les services Bizca (pas uniquement Users).

## Changements

### Avant

```
microservices/
└── user/
    └── src/
        └── Bizca.Users.AppHost/  ← Orchestration uniquement pour Users
            └── AppHost.cs
```

### Après

```
microservices/
└── Bizca.Services.AppHost/  ← Orchestration pour tous les services
    └── AppHost.cs
```

## Services orchestrés

| Service | Description | Port |
|---|---|---|
| **Keycloak** | Serveur d'authentification | 8080 |
| **PostgreSQL** | Base de données Users | Dynamique |
| **OpenID API** | API d'authentification | Dynamique |
| **Users API** | API de gestion des utilisateurs | Dynamique |

## Ajout de Keycloak

Keycloak a été ajouté comme conteneur Docker Aspire :

```csharp
var keycloak = builder
    .AddContainer("keycloak", "quay.io/keycloak/keycloak", "latest")
    .WithHttpEndpoint(port: 8080, targetPort: 8080, name: "http")
    .WithEnvironment("KEYCLOAK_ADMIN", "admin")
    .WithEnvironment("KEYCLOAK_ADMIN_PASSWORD", "admin")
    .WithArgs("start-dev")
    .WithBindMount("keycloak-data", "/opt/keycloak/data");
```

### Configuration initiale Keycloak

1. Démarrer l'AppHost
2. Accéder à `http://localhost:8080`
3. Se connecter avec `admin` / `admin`
4. Créer le realm `bizca`
5. Créer le client `bizca-backend`

## Démarrage

```powershell
# Avant
dotnet run --project microservices/user/src/Bizca.Users.AppHost/Bizca.Users.AppHost.csproj

# Après
dotnet run --project microservices/Bizca.Services.AppHost/Bizca.Services.AppHost.csproj
```

## Références de projet

Les projets suivants référencent maintenant `Bizca.Services.AppHost` :

```xml
<ProjectReference Include="..\user\src\Bizca.Users.Api\Bizca.Users.Api.csproj" />
<ProjectReference Include="..\..\security\Bizca.OpenId.Api\Bizca.OpenId.Api.csproj" />
</ProjectReference>
```

## Impact sur le développement

- ✅ Tous les services démarrent ensemble via un seul point d'entrée
- ✅ Keycloak démarre automatiquement
- ✅ Les dépendances entre services sont gérées automatiquement
- ✅ Dashboard Aspire unifié pour tous les services

## Rollback

Si besoin de revenir à l'ancienne structure :

1. Restaurer le dossier `microservices/user/src/Bizca.Users.AppHost/`
2. Supprimer `microservices/Bizca.Services.AppHost/`
3. Mettre à jour la solution

## Date de migration

**20 mai 2026**

