---
name: ci-cd-and-automation
description: Automates CI/CD pipeline setup. Use when setting up or modifying build and deployment pipelines. Use when you need to automate quality gates, configure test runners in CI, or establish deployment strategies.
---

# CI/CD and Automation

## Overview

Automate quality gates so that no change reaches production without passing build, tests, and security checks. CI/CD is the enforcement mechanism for every other skill — it catches what humans and agents miss, consistently on every single change.

**Shift Left:** Catch problems as early in the pipeline as possible. A bug caught at build time costs minutes; the same bug caught in production costs hours. Move checks upstream — static analysis before tests, tests before staging, staging before production.

**Faster is Safer:** Smaller batches and more frequent releases reduce risk. A deployment with 3 changes is easier to debug than one with 30.

## When to Use

- Setting up a new project's CI pipeline
- Adding or modifying automated checks
- Configuring deployment pipelines
- When a change should trigger automated verification
- Debugging CI failures

## The Quality Gate Pipeline

Every change goes through these gates before merge:

```
Pull Request Opened
    │
    ▼
┌─────────────────┐
│   BUILD          │  dotnet build --no-restore (TreatWarningsAsErrors=true)
│   ↓ pass         │
│   UNIT TESTS     │  dotnet test --filter Category=Unit
│   ↓ pass         │
│   INTEGRATION    │  dotnet test --filter Category=Integration  (Testcontainers)
│   ↓ pass         │
│   SECURITY AUDIT │  dotnet list package --vulnerable
└─────────────────┘
    │
    ▼
  Ready for review
```

**No gate can be skipped.** If build fails, fix it — don't suppress the warning. If a test fails, fix the code — don't skip the test.

## GitHub Actions Configuration

### Basic .NET CI Pipeline

```yaml
# .github/workflows/ci.yml
name: CI

on:
  pull_request:
    branches: [main]
  push:
    branches: [main]

jobs:
  build-and-test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.x'

      - name: Restore
        run: dotnet restore

      - name: Build
        run: dotnet build --no-restore --configuration Release

      - name: Unit tests
        run: dotnet test --no-build --configuration Release --filter "Category=Unit"

      - name: Security audit
        run: dotnet list package --vulnerable --include-transitive
```

### With Integration Tests (Testcontainers)

Integration tests use **Testcontainers** — they spin up a real PostgreSQL container from inside the test process. The GitHub Actions runner does **not** need a `services:` block for Postgres; the container is managed by the test fixture itself.

The only requirement is Docker being available on the runner — `ubuntu-latest` provides this by default.

```yaml
  integration-tests:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.x'

      - name: Restore
        run: dotnet restore

      - name: Build
        run: dotnet build --no-restore --configuration Release

      - name: Integration tests
        run: dotnet test --no-build --configuration Release --filter "Category=Integration"
```

> **Note:** Testcontainers requires Docker socket access. On `ubuntu-latest` GitHub-hosted runners, Docker is pre-installed. Self-hosted runners must have Docker running and the CI user in the `docker` group.

### EF Core Migrations in CI

For environments that need schema verification (not usually CI — Testcontainers handles it in testing):

```yaml
      - name: Install EF Core tools
        run: dotnet tool install --global dotnet-ef

      - name: Verify migrations
        run: |
          dotnet ef migrations list \
            --project microservices/user/src/Bizca.Users.Infrastructure \
            --startup-project microservices/user/src/Bizca.Users.Api
        env:
          ConnectionStrings__Default: ${{ secrets.CI_DB_CONNECTION_STRING }}
```

### Security Audit

```yaml
      - name: Check for vulnerabilities
        run: dotnet list package --vulnerable --include-transitive
        # Fails the build if any known vulnerable packages are found
```

## Feeding CI Failures Back to Agents

The power of CI with AI agents is the feedback loop. When CI fails:

```
CI fails
    │
    ▼
Copy the failure output
    │
    ▼
Feed it to the agent:
"The CI pipeline failed with this error:
[paste specific error]
Fix the issue and verify locally before pushing again."
    │
    ▼
Agent fixes → pushes → CI runs again
```

**Key patterns:**

```
Build failure    → Agent reads the error location and fixes the C# compile error
Test failure     → Agent follows debugging-and-error-recovery skill
Security warning → Agent updates the vulnerable package or adds justification
```

## Deployment Strategies

### Staged Rollouts

```
PR merged to main
    │
    ▼
  Staging deployment (auto — deploy to staging environment)
    │ Manual verification
    ▼
  Production deployment (manual trigger or auto after staging)
    │
    ▼
  Monitor for errors (15-minute window)
    │
    ├── Errors detected → Rollback
    └── Clean → Done
```

### Rollback Plan

Every deployment should be reversible:

```yaml
# Manual rollback workflow
name: Rollback
on:
  workflow_dispatch:
    inputs:
      version:
        description: 'Docker image tag to rollback to'
        required: true

jobs:
  rollback:
    runs-on: ubuntu-latest
    steps:
      - name: Rollback deployment
        run: |
          # Re-deploy the specified previous image tag
          echo "Rolling back to ${{ inputs.version }}"
          # Platform-specific: kubectl, az container, etc.
```

### EF Core Migration Rollback

```bash
# Apply a specific previous migration
dotnet ef database update <MigrationName> \
  --project microservices/user/src/Bizca.Users.Infrastructure \
  --startup-project microservices/user/src/Bizca.Users.Api

# Remove the last generated migration (before applying to DB)
dotnet ef migrations remove \
  --project microservices/user/src/Bizca.Users.Infrastructure \
  --startup-project microservices/user/src/Bizca.Users.Api
```

## Environment Management

```
appsettings.json           → Committed (defaults, no secrets)
appsettings.Development.json → Committed (dev-friendly overrides, no secrets)
appsettings.Local.json     → NOT committed (local overrides with real connection strings)
User Secrets (dotnet user-secrets) → NOT committed (local development only)
CI secrets                 → Stored in GitHub Secrets
Production secrets         → Stored in Azure Key Vault / AWS Secrets Manager / etc.
```

CI should never have production secrets. Use separate secrets for CI testing.

## Automation Beyond CI

### Dependabot / Renovate for NuGet

```yaml
# .github/dependabot.yml
version: 2
updates:
  - package-ecosystem: nuget
    directory: /
    schedule:
      interval: weekly
    open-pull-requests-limit: 5
```

### Branch Protection

- **Required reviews:** At least 1 approval before merge
- **Required status checks:** CI must pass before merge
- **Branch protection:** No force-pushes to main
- **Auto-merge:** If all checks pass and approved, merge automatically

## CI Optimization

When the pipeline exceeds 10 minutes:

```
Slow CI pipeline?
├── Cache NuGet packages
│   └── Use actions/cache for ~/.nuget/packages
├── Run jobs in parallel
│   └── Split unit tests, integration tests, and build into separate jobs
├── Only run what changed
│   └── Use path filters to skip unrelated jobs (e.g., skip integration for docs-only PRs)
├── Use matrix builds
│   └── Shard integration test suites across multiple runners
└── Use larger runners
    └── GitHub-hosted larger runners for CPU-heavy builds
```

**Example: caching and parallelism**
```yaml
jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.x'
      - uses: actions/cache@v4
        with:
          path: ~/.nuget/packages
          key: ${{ runner.os }}-nuget-${{ hashFiles('**/*.csproj', '**/Directory.Packages.props') }}
          restore-keys: ${{ runner.os }}-nuget-
      - run: dotnet restore
      - run: dotnet build --no-restore --configuration Release

  unit-tests:
    needs: build
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.x'
      - uses: actions/cache@v4
        with:
          path: ~/.nuget/packages
          key: ${{ runner.os }}-nuget-${{ hashFiles('**/*.csproj', '**/Directory.Packages.props') }}
      - run: dotnet restore
      - run: dotnet test --filter "Category=Unit" --configuration Release

  integration-tests:
    needs: build
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.x'
      - uses: actions/cache@v4
        with:
          path: ~/.nuget/packages
          key: ${{ runner.os }}-nuget-${{ hashFiles('**/*.csproj', '**/Directory.Packages.props') }}
      - run: dotnet restore
      - run: dotnet test --filter "Category=Integration" --configuration Release
```

## Common Rationalizations

| Rationalization | Reality |
|---|---|
| "CI is too slow" | Optimize the pipeline (see CI Optimization above), don't skip it. A 5-minute pipeline prevents hours of debugging. |
| "This change is trivial, skip CI" | Trivial changes break builds. CI is fast for trivial changes anyway. |
| "The test is flaky, just re-run" | Flaky tests mask real bugs and waste everyone's time. Fix the flakiness. |
| "We'll add CI later" | Projects without CI accumulate broken states. Set it up on day one. |
| "Testcontainers are slow in CI" | A Postgres container starts in ~3 seconds. The confidence is worth it — don't mock the database. |

## Red Flags

- No CI pipeline in the project
- CI failures ignored or silenced
- Tests disabled in CI to make the pipeline pass
- Production deploys without staging verification
- No rollback mechanism
- Secrets stored in code or CI config files (not secrets manager)
- Long CI times with no optimization effort

## Verification

After setting up or modifying CI:

- [ ] All quality gates are present (build, unit tests, integration tests, security audit)
- [ ] Pipeline runs on every PR and push to main
- [ ] Failures block merge (branch protection configured)
- [ ] CI results feed back into the development loop
- [ ] Secrets are stored in the secrets manager, not in code
- [ ] Deployment has a rollback mechanism
- [ ] Pipeline runs in under 10 minutes for the test suite
