# Commandes utiles — Bizca.Services.AppHost

## Démarrage et arrêt

### Démarrer tous les services

```powershell
# Depuis la racine du projet
dotnet run --project microservices/Bizca.Services.AppHost/Bizca.Services.AppHost.csproj
```

### Démarrer avec logs détaillés

```powershell
dotnet run --project microservices/Bizca.Services.AppHost/Bizca.Services.AppHost.csproj --environment Development
```

### Arrêter les services

Appuyer sur `Ctrl+C` dans le terminal où l'AppHost est exécuté.

## Compilation

### Compiler le projet AppHost

```powershell
dotnet build microservices/Bizca.Services.AppHost/Bizca.Services.AppHost.csproj
```

### Compiler toute la solution

```powershell
dotnet build bizca.slnx
```

### Nettoyer et recompiler

```powershell
dotnet clean bizca.slnx
dotnet build bizca.slnx --no-incremental
```

## Gestion Docker

### Lister les containers Bizca

```powershell
docker ps -a | findstr bizca
```

### Arrêter tous les containers

```powershell
docker ps -q | ForEach-Object { docker stop $_ }
```

### Supprimer tous les containers arrêtés

```powershell
docker container prune -f
```

### Lister les volumes Bizca

```powershell
docker volume ls | findstr -E "postgres|keycloak"
```

### Supprimer les volumes (reset complet des données)

```powershell
docker volume rm postgres-data
# Si le volume keycloak-data existe comme volume Docker
docker volume rm keycloak-data
```

### Nettoyer complètement Docker

```powershell
# ATTENTION : Supprime TOUS les containers, volumes et images non utilisés
docker system prune -a --volumes -f
```

## Accès aux services

### Dashboard Aspire

```powershell
start https://localhost:17000
# ou
start http://localhost:15000
```

### Keycloak Admin Console

```powershell
start http://localhost:8080
```

### Vérifier si Keycloak est prêt

```powershell
curl http://localhost:8080/health/ready
```

## Logs et debugging

### Voir les logs d'un container spécifique

```powershell
# Lister les containers
docker ps

# Voir les logs (remplacer <container-id>)
docker logs <container-id>

# Suivre les logs en temps réel
docker logs -f <container-id>
```

### Logs Keycloak

```powershell
docker logs -f (docker ps -q --filter "ancestor=quay.io/keycloak/keycloak")
```

### Logs PostgreSQL

```powershell
docker logs -f (docker ps -q --filter "ancestor=postgres")
```

### Inspecter un container

```powershell
docker inspect <container-id>
```

## Base de données

### Se connecter à PostgreSQL

```powershell
# Trouver le port de PostgreSQL dans le Dashboard Aspire
# Puis :
docker exec -it <postgres-container-id> psql -U postgres -d bizca-users
```

### Sauvegarder la base de données

```powershell
docker exec <postgres-container-id> pg_dump -U postgres bizca-users > backup.sql
```

### Restaurer la base de données

```powershell
Get-Content backup.sql | docker exec -i <postgres-container-id> psql -U postgres -d bizca-users
```

## Keycloak

### Exporter la configuration Keycloak

```powershell
docker exec <keycloak-container-id> /opt/keycloak/bin/kc.sh export --dir /tmp/keycloak-export
docker cp <keycloak-container-id>:/tmp/keycloak-export ./keycloak-backup
```

### Importer la configuration Keycloak

```powershell
docker cp ./keycloak-backup <keycloak-container-id>:/tmp/keycloak-import
docker exec <keycloak-container-id> /opt/keycloak/bin/kc.sh import --dir /tmp/keycloak-import
```

### Créer un utilisateur de test dans Keycloak (API)

```powershell
# Obtenir le token admin
$adminToken = (Invoke-RestMethod -Method Post -Uri "http://localhost:8080/realms/master/protocol/openid-connect/token" `
  -Body @{
    grant_type = "password"
    client_id = "admin-cli"
    username = "admin"
    password = "admin"
  } -ContentType "application/x-www-form-urlencoded").access_token

# Créer un utilisateur
Invoke-RestMethod -Method Post -Uri "http://localhost:8080/admin/realms/bizca/users" `
  -Headers @{ Authorization = "Bearer $adminToken" } `
  -Body (@{
    username = "testuser"
    enabled = $true
    email = "test@example.com"
    credentials = @(@{
      type = "password"
      value = "password"
      temporary = $false
    })
  } | ConvertTo-Json) `
  -ContentType "application/json"
```

## Troubleshooting

### Port déjà utilisé

```powershell
# Trouver le processus qui utilise le port 8080
netstat -ano | findstr :8080

# Tuer le processus (remplacer <PID>)
taskkill /PID <PID> /F
```

### Réinitialiser complètement l'environnement

```powershell
# 1. Arrêter l'AppHost (Ctrl+C)

# 2. Arrêter tous les containers
docker ps -q | ForEach-Object { docker stop $_ }

# 3. Supprimer les volumes
docker volume rm postgres-data

# 4. Nettoyer les build
dotnet clean bizca.slnx

# 5. Recompiler
dotnet build bizca.slnx

# 6. Redémarrer
dotnet run --project microservices/Bizca.Services.AppHost/Bizca.Services.AppHost.csproj
```

### Vérifier la santé des services

```powershell
# OpenID API
curl http://localhost:<port>/health

# Users API
curl http://localhost:<port>/health

# Keycloak
curl http://localhost:8080/health/ready
```

## Tests

### Exécuter les tests unitaires

```powershell
dotnet test bizca.slnx --filter "Category=Unit"
```

### Exécuter les tests d'intégration

```powershell
# Nécessite Docker
dotnet test bizca.slnx --filter "Category!=Unit"
```

## Mise à jour des dépendances

### Mise à jour Aspire

```powershell
dotnet workload update
```

### Mise à jour des packages NuGet

```powershell
# Vérifier les mises à jour disponibles
dotnet list bizca.slnx package --outdated

# Mettre à jour un package spécifique dans Directory.Packages.props
# (manuel)
```

## Performance

### Activer le mode Release

```powershell
dotnet run --project microservices/Bizca.Services.AppHost/Bizca.Services.AppHost.csproj --configuration Release
```

### Build optimisé

```powershell
dotnet build bizca.slnx --configuration Release
```

## Références rapides

- **Dashboard Aspire** : `https://localhost:17000`
- **Keycloak** : `http://localhost:8080` (admin/admin)
- **Documentation** : `microservices/Bizca.Services.AppHost/README.md`
- **Architecture** : `microservices/Bizca.Services.AppHost/ARCHITECTURE.md`

