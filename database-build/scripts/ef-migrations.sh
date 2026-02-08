#!/usr/bin/env bash

set -e

wait_for_sql_server() {
  echo "Waiting for SQL Server on ${SQL_HOST}:${SQL_PORT} ..."
  for i in {1..60}; do
    if (echo >"/dev/tcp/${SQL_HOST}/${SQL_PORT}") >/dev/null 2>&1; then
      echo "SQL Server is up."
      return 0
    fi
    sleep 5
  done
  echo "Timeout waiting for SQL Server."
  exit 1
}

run_migration() {
  local context=$1
  local dbname=$2
  local db_conn="Server=${SQL_HOST},${SQL_PORT};Database=${dbname};User Id=${DB_USER};Password=${DB_PASSWORD};TrustServerCertificate=True;"

  echo "Applying migrations for [$context] on database [$dbname]..."
  dotnet ef database update \
    --no-build \
    --context "$context" \
    --connection "$db_conn" \
    --project "$EF_MIGRATIONS_PROJECT" \
    --startup-project "$EF_STARTUP_MIGRATIONS_PROJECT" \
    --verbose
  echo "Migrations applied for [$context] on database [$dbname]"
}

echo 'Configuring required dotnet tools...'

export PATH="$PATH:/home/docker/.dotnet/tools"
dotnet ef --version

wait_for_sql_server

echo "Starting EF Core migrations..."

while IFS='=' read -r context dbname; do
  [[ -z "${context// }" || "$context" == \#* ]] && continue
  run_migration "$context" "$dbname"
done <<< "$CTX_DB"

echo "EF Core migrations completed."