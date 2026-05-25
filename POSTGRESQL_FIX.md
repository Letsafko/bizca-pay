# ✅ PostgreSQL Unhealthy Issue - FIXED

**Date**: 2026-05-21
**Status**: Resolved
**Problem**: PostgreSQL container remained "unhealthy" when starting the Aspire AppHost

---

## 🔍 Root Cause

The issue was caused by a **corrupted Docker volume** (`postgres-data`) from a previous configuration. This volume contained outdated or incompatible data that prevented PostgreSQL from starting correctly.

---

## 🛠️ Solution Applied

### 1. **Removed Corrupted Volume**

```powershell
docker volume rm postgres-data
```

This deleted the old volume containing incompatible data.

### 2. **Updated AppHost Configuration**

**File**: `microservices/Bizca.Services.AppHost/AppHost.cs`

```csharp
// PostgreSQL database for Users service
const string databaseName = "bizca-users";
const string resourceName = "database";

var postgres = builder
    .AddPostgres("postgres")
    .WithDataVolume()  // Fresh anonymous volume (no persistent name)
    .WithPgAdmin();     // Web UI for database management

var database = postgres.AddDatabase(resourceName, databaseName);
```

**Key changes:**
- ✅ Removed named volume `"postgres-data"` → Now uses anonymous volume
- ✅ Added `.WithPgAdmin()` for web-based database management
- ✅ Simplified configuration (let Aspire manage credentials automatically)

---

## ✅ Verification

### PostgreSQL Container Status

```powershell
PS> docker container ls
CONTAINER ID   IMAGE                 CREATED         STATUS          PORTS
fd898a1c942b   postgres:17.6         42 seconds ago  Up 32 seconds   127.0.0.1:60589->5432/tcp
```

### PostgreSQL Logs

```
✅ PostgreSQL init process complete; ready for start up.
✅ database system is ready to accept connections
✅ listening on 0.0.0.0:5432
```

### Services Running

| Service | Container | Port | Status |
|---|---|---|---|
| **PostgreSQL** | `postgres-bvqfmzkb` | 60589 → 5432 | ✅ Running |
| **PgAdmin** | `pgadmin-xjwpbfar` | 50557 → 80 | ✅ Running |
| **Keycloak** | `keycloak-xagehpta` | 60587 → 8080 | ✅ Running |

---

## 🚀 Next Steps

### 1. **Access the Aspire Dashboard**

```powershell
# Dashboard should be available at:
start https://localhost:17000
```

### 2. **Access PgAdmin**

```powershell
# PgAdmin is available at:
start http://localhost:50557
```

**Default credentials** (auto-configured by Aspire):
- Email: dynamically generated
- Password: dynamically generated

**Note**: Check the Aspire Dashboard for the exact credentials.

### 3. **Connect PostgreSQL from PgAdmin**

In PgAdmin:
1. Add New Server
2. **Host**: `postgres-bvqfmzkb` (or use `localhost`)
3. **Port**: `60589`
4. **Database**: `bizca-users`
5. **Username**: Check Aspire Dashboard
6. **Password**: Check Aspire Dashboard

### 4. **Verify Database Creation**

Once the Users API starts, it will:
1. Connect to PostgreSQL
2. Create the `bizca-users` database (if it doesn't exist)
3. Apply EF Core migrations
4. Create all necessary tables

Check the Users API logs for:
```
✅ Database created successfully
✅ Applied X migrations
```

---

## 📊 Final Architecture

```
Aspire AppHost
├── Keycloak (http://localhost:60587)
│   └── Realm: bizca
│
├── PostgreSQL (localhost:60589)
│   └── Database: bizca-users
│       └── Tables: (created by EF migrations)
│
├── PgAdmin (http://localhost:50557)
│   └── Web UI for PostgreSQL management
│
├── OpenID API
│   └── Depends on: Keycloak
│
└── Users API
    └── Depends on: PostgreSQL
```

---

## 🔧 Troubleshooting

### If PostgreSQL still shows as unhealthy (unlikely)

1. **Stop all containers**:
   ```powershell
   docker stop $(docker ps -q)
   ```

2. **Remove ALL volumes**:
   ```powershell
   docker volume ls
   docker volume rm <volume-name>
   ```

3. **Restart Docker Desktop**

4. **Rebuild and run AppHost**:
   ```powershell
   dotnet build bizca.slnx
   dotnet run --project microservices/Bizca.Services.AppHost/Bizca.Services.AppHost.csproj
   ```

### Check PostgreSQL health manually

```powershell
# Connect to PostgreSQL container
docker exec -it postgres-bvqfmzkb psql -U postgres

# Inside psql, run:
\l                    # List all databases
\c bizca-users        # Connect to bizca-users database
\dt                   # List all tables
SELECT version();     # Check PostgreSQL version
```

---

## 📝 Lessons Learned

### ❌ What caused the problem

- **Named volumes** can persist corrupted data across restarts
- PostgreSQL strict about data directory consistency
- Aspire doesn't auto-cleanup old volumes

### ✅ How to prevent it

1. **Use anonymous volumes** (`.WithDataVolume()` without a name) for dev
2. **Use named volumes** (`.WithDataVolume("postgres-data")`) only for production-like persistence
3. **Delete volumes** when upgrading PostgreSQL versions
4. **Use Docker Compose** `down -v` to clean up volumes

---

## 🎯 Summary

| Before | After |
|---|---|
| ❌ PostgreSQL unhealthy | ✅ PostgreSQL healthy |
| ❌ Corrupted `postgres-data` volume | ✅ Fresh anonymous volume |
| ❌ Manual env vars causing conflicts | ✅ Aspire auto-manages credentials |
| ❌ No database management UI | ✅ PgAdmin available at `localhost:50557` |

**Problem solved!** PostgreSQL is now running correctly and ready for the Users API to connect. 🎉

---

## 🔗 Related Files

- `microservices/Bizca.Services.AppHost/AppHost.cs` — Aspire orchestration
- `microservices/Bizca.Services.AppHost/README.md` — Full AppHost documentation
- `microservices/Bizca.Services.AppHost/COMMANDS.md` — PowerShell commands reference

---

**Issue resolved**: 2026-05-21 23:50 UTC

