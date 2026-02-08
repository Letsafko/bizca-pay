# User Service Local Setup Guide

This guide explains how to set up and run the User microservice locally with its database using Docker.

## 📋 Prerequisites

- **.NET 10.0 SDK** - [Download here](https://dotnet.microsoft.com/download/dotnet/10.0)
  ```powershell
  dotnet --version
  ```

- **Docker Desktop** - [Download here](https://www.docker.com/products/docker-desktop)
  ```powershell
  docker --version
  docker-compose --version
  ```

- **SQL Editor** (optional) - Azure Data Studio, SQL Server Management Studio, or DBeaver to inspect the database

## 🗂️ Service Architecture

```
microservices/user/
├── src/
│   ├── Bizca.Users.Api/           # API entry point
│   ├── Bizca.Users.Domain/        # Business logic and domain entities
│   └── Bizca.Users.Infrastructure/ # Data access and DbContext
└── test/
    └── Bizca.User.IntegrationTests/

database-build/
├── docker-compose.yaml             # Container orchestration
├── Dockerfile                      # Docker image for EF Core migrations
├── .env                            # Environment variables
└── scripts/
    └── ef-migrations.sh            # Migration execution script
```

## 🚀 Docker Setup (Recommended)

This option automatically launches the database, applies migrations, and starts the API.

### Step 1: Verify Environment Variables

Check the `database-build/.env` file:

```dotenv
DB_PASSWORD=Password0@
DB_PORT=1433
DB_USER=SA
BIZA_USER_DB_NAME=bizca-user
```

### Step 2: Start Services

From the `database-build/` directory:

```powershell
cd D:\Projects\Perso\bizca\database-build
docker-compose up -d
```

**What happens:**
1. 🗄️ **mssql** - SQL Server 2022 starts on port 1433
2. 🔄 **migrator-user** - Applies EF Core migrations to the `bizca-user` database
3. 🌐 **user-web-api** - Launches the User API on http://localhost:5001
4. 📊 **seq** - Starts the log server on http://localhost:8081

### Step 3: Check Service Status

```powershell
# View running containers
docker-compose ps

# View API logs
docker-compose logs -f user-web-api

# View migrator logs
docker-compose logs migrator-user
```

### Step 4: Stop Services

```powershell
# Stop without removing data
docker-compose stop

# Stop and remove containers (keeps volumes)
docker-compose down

# Stop and remove everything (including data)
docker-compose down -v
```

## 🔄 Migration Management

### Understanding the Automatic Migration Process

The `migrator-user` container automatically applies migrations for user microservice when starting. Here's how it works:

1. **Wait for SQL Server** - The script waits until SQL Server is ready
2. **Apply Migrations** - Uses `dotnet ef database update` to apply pending migrations
3. **Verify Success** - Logs show whether migrations were applied successfully

The migration configuration is defined in `docker-compose.yaml`:

```yaml
migrator-user:
  container_name: ef-migrations-user
  depends_on:
    - mssql
  environment:
    EF_MIGRATIONS_PROJECT: "microservices/user/src/Bizca.Users.Infrastructure/Bizca.Users.Infrastructure.csproj"
    EF_STARTUP_MIGRATIONS_PROJECT: "microservices/user/src/Bizca.Users.Api/Bizca.Users.Api.csproj"
    CTX_DB: |
        Bizca.Users.Infrastructure.Context.ApplicationDbContext=${BIZCA_USER_DB_NAME}
```

### Creating a New Migration

Before creating a migration, ensure you have the necessary tools installed:

```powershell
# Install EF Core tools globally (if not already installed)
dotnet tool install --global dotnet-ef

# Update to the latest version
dotnet tool update --global dotnet-ef
```

From the `Bizca.Users.Api` directory:

```powershell
cd D:\Projects\Perso\bizca\microservices\user\src\Bizca.Users.Api

dotnet ef migrations add {MigrationName} `
  --project ..\Bizca.Users.Infrastructure\Bizca.Users.Infrastructure.csproj `
  --startup-project .\Bizca.Users.Api.csproj `
  --context ApplicationDbContext
```

**Best practices for migration names:**
- Use PascalCase (e.g., `AddUserEmailColumn`)
- Be descriptive (e.g., `CreateOrdersTable` not `Update1`)
- Include the action (Add, Create, Update, Remove)

### Applying Migrations with Docker

After creating a new migration, rebuild and restart the migrator:

```powershell
cd D:\Projects\Perso\bizca\database-build

# Stop the current migrator
docker-compose stop migrator-user

# Rebuild the migrator image with new migration files
docker-compose build migrator-user

# Start the migrator to apply new migrations
docker-compose up -d migrator-user

# Watch the migration process
docker-compose logs -f migrator-user
```

### Applying Migrations Manually (Without Docker Rebuild)

If you want to test migrations locally without rebuilding containers:

```powershell
cd D:\Projects\Perso\bizca\microservices\user\src\Bizca.Users.Api

# Ensure SQL Server container is running
docker-compose -f ..\..\..\..\database-build\docker-compose.yaml up -d mssql

# Apply migrations
dotnet ef database update `
  --project ..\Bizca.Users.Infrastructure\Bizca.Users.Infrastructure.csproj `
  --startup-project .\Bizca.Users.Api.csproj `
  --context ApplicationDbContext `
  --connection "Server=localhost,1433;Database=bizca-user;User Id=SA;Password=Password0@;TrustServerCertificate=True;"
```

### Rolling Back Migrations

To roll back to a specific migration:

```powershell
cd D:\Projects\Perso\bizca\microservices\user\src\Bizca.Users.Api

# Rollback to a previous migration
dotnet ef database update PreviousMigrationName `
  --project ..\Bizca.Users.Infrastructure\Bizca.Users.Infrastructure.csproj `
  --startup-project .\Bizca.Users.Api.csproj `
  --context ApplicationDbContext

# Or rollback all migrations (returns to empty database)
dotnet ef database update 0 `
  --project ..\Bizca.Users.Infrastructure\Bizca.Users.Infrastructure.csproj `
  --startup-project .\Bizca.Users.Api.csproj `
  --context ApplicationDbContext
```

### Removing a Migration

To remove the last migration (only if it hasn't been applied to other environments):

```powershell
dotnet ef migrations remove `
  --project ..\Bizca.Users.Infrastructure\Bizca.Users.Infrastructure.csproj `
  --startup-project .\Bizca.Users.Api.csproj `
  --context ApplicationDbContext
```

⚠️ **Warning:** Only remove migrations that haven't been deployed to production!

### Viewing Migration History

```powershell
# List all migrations
dotnet ef migrations list `
  --project ..\Bizca.Users.Infrastructure\Bizca.Users.Infrastructure.csproj `
  --startup-project .\Bizca.Users.Api.csproj `
  --context ApplicationDbContext

# Generate SQL script for a specific migration
dotnet ef migrations script PreviousMigration TargetMigration `
  --project ..\Bizca.Users.Infrastructure\Bizca.Users.Infrastructure.csproj `
  --startup-project .\Bizca.Users.Api.csproj `
  --context ApplicationDbContext `
  --output migration.sql

# Generate SQL script for all migrations
dotnet ef migrations script `
  --project ..\Bizca.Users.Infrastructure\Bizca.Users.Infrastructure.csproj `
  --startup-project .\Bizca.Users.Api.csproj `
  --context ApplicationDbContext `
  --output all-migrations.sql
```

### Rebuilding the Complete Stack with New Migrations

When you've added new migrations and want a clean start:

```powershell
cd D:\Projects\Perso\bizca\database-build

# Stop and remove everything
docker-compose down -v

# Rebuild all images (includes new migration files)
docker-compose build --no-cache

# Start fresh with new migrations
docker-compose up -d

# Monitor the migration process
docker-compose logs -f migrator-user
```

### Manually Triggering Migrations in Running Container

If the migrator container has already run, but you need to reapply:

```powershell
# Execute the migration script inside the running container
docker exec -it ef-migrations-user bash -c "./ef-migrations.sh"

# Or restart the migrator service
docker-compose restart migrator-user
docker-compose logs -f migrator-user
```

### Via Azure Data Studio or SSMS

Connection string:
```
Server=localhost,1433
Database=bizca-user
Authentication=SQL Server Authentication
User=SA
Password=Password0@
Trust Server Certificate=Yes
```

## 📊 Log Visualization with Seq

Seq is a centralized log server that collects structured logs from the application.

**Access:** http://localhost:8081

Seq is automatically configured in `appsettings.Development.json`:

```json
{
  "Serilog": {
    "WriteTo": [
      { "Name": "Console" },
      {
        "Name": "Seq",
        "Args": { "ServerUrl": "http://seq:5341" }
      }
    ]
  }
}
```

## 🛠️ Troubleshooting

### Issue: Port 1433 is already in use

```powershell
# Check which process is using the port
netstat -ano | findstr :1433

# Stop the process (replace PID with the process ID)
taskkill /PID <PID> /F

# Or change the port in .env and docker-compose.yaml
```

### Issue: Migrations are not applying

```powershell
# Check migrator logs
docker-compose logs migrator-user

# Look for specific errors
docker-compose logs migrator-user | Select-String "error"

# Restart the migrator
docker-compose restart migrator-user

# Apply migrations manually inside container
docker exec -it ef-migrations-user bash -c "./ef-migrations.sh"
```

### Issue: API cannot connect to the database

1. Verify SQL Server is running:
   ```powershell
   docker ps | findstr mssql
   ```

2. Check connection string in `appsettings.Development.json`

3. Test connection with sqlcmd:
   ```powershell
   docker exec -it mssql /opt/mssql-tools/bin/sqlcmd -S localhost -U SA -P "Password0@" -Q "SELECT @@VERSION"
   ```

4. Check if migrations were applied:
   ```powershell
   docker exec -it mssql /opt/mssql-tools/bin/sqlcmd -S localhost -U SA -P "Password0@" -Q "USE [bizca-user]; SELECT * FROM __EFMigrationsHistory"
   ```

### Issue: Docker permission errors

On Windows, ensure Docker Desktop has necessary permissions and WSL2 is configured correctly.

```powershell
# Check Docker version
docker version

# Verify Docker is working
docker run hello-world
```

### Issue: Completely clean the environment

```powershell
cd D:\Projects\Perso\bizca\database-build

# Stop all containers
docker-compose down -v

# Remove images
docker-compose down --rmi all -v

# Clean Docker system (optional - removes unused resources)
docker system prune -a

# Rebuild from scratch
docker-compose build --no-cache
docker-compose up -d
```

### Issue: Migration container exits immediately

Check the logs for detailed error messages:

```powershell
# View full logs
docker-compose logs migrator-user

# Check if SQL Server is ready
docker-compose logs mssql | Select-String "SQL Server is now ready"
```

## 📝 Environment Configuration

The User service supports multiple environments via `appsettings.{Environment}.json` files:

- **`appsettings.json`** - Base configuration
- **`appsettings.Development.json`** - For Docker and development with Seq
- **`appsettings.Local.json`** - For local development without Docker for API

## 🔗 Useful Links

- **User API**: http://localhost:5001
- **Seq Logs**: http://localhost:8081
- **SQL Server**: localhost, 1433

## 📚 References

- [Entity Framework Core](https://learn.microsoft.com/ef/core/)
- [EF Core Migrations](https://learn.microsoft.com/ef/core/managing-schemas/migrations/)
- [SQL Server Docker](https://hub.docker.com/_/microsoft-mssql-server)
- [Docker Compose](https://docs.docker.com/compose/)
- [Seq Documentation](https://docs.datalust.co/docs)

## 🤝 Contributing

When adding new migrations, make sure to:

1. **Create** migrations using `dotnet ef migrations add`
2. **Test locally** before committing
3. **Verify** that the `ef-migrations.sh` script applies them correctly in Docker
4. **Document** major schema changes in the migration file or commit message
5. **Review** the generated SQL to ensure it matches your intent
6. **Never** remove migrations that have been deployed to shared environments
7. **Coordinate** with the team before rolling back shared database migrations
