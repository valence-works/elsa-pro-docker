# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What This Is

A production-ready Docker packaging of [Elsa Workflows 3.x](https://elsa-workflows.github.io/elsa-core/) — a .NET workflow automation platform. The repo builds and publishes Docker images to Docker Hub (`valenceworks/elsa-pro-server`, `valenceworks/elsa-pro-studio`). There are no test projects; correctness is validated by running the stack.

## Build & Run

**Build a single project:**
```bash
dotnet build src/ElsaProServer/ElsaProServer.csproj
dotnet build src/ElsaProStudio.BlazorServer/ElsaProStudio.BlazorServer.csproj
```

**Run the server locally (no Docker):**
```bash
cd src/ElsaProServer
dotnet run
```

**Build Docker images:**
```bash
docker build -t elsa-pro-server -f src/ElsaProServer/Dockerfile .
docker build -t elsa-pro-studio -f src/ElsaProStudio.BlazorServer/Dockerfile .
```

**Full dev stack (server + studio + all optional infrastructure):**
```bash
docker compose up -d
# Server API: http://localhost:8080
# Studio UI:  http://localhost:8081
# RabbitMQ:   http://localhost:15672
# SMTP4Dev:   http://localhost:3000
```

Required env vars (copy `.env.example`): `ELSA_ADMIN_EMAIL`, `ELSA_ADMIN_PASSWORD`.

## Architecture

Five .NET 10 projects in `src/`:

| Project | Role |
|---|---|
| `ElsaProServer` | Elsa runtime API (port 8080) |
| `ElsaProStudio.BlazorServer` | Blazor Server workflow designer UI (port 8081) |
| `ElsaProServer.Identity` | User/role management, admin provisioning |
| `ElsaProServer.ServiceDefaults` | Aspire shared setup (OTel, health checks, resilience) |
| `ElsaProServer.AppHost` | Aspire orchestration for local dev |
| `SamplePackage` | Reference plugin for Nuplane extensibility |

**Key architectural concepts:**

- **CShells** — Multi-tenant "shell" abstraction inside `ElsaProServer`. Each shell is an isolated Elsa workflow engine instance. Configured in `Program.cs`.
- **Nuplane** — Runtime plugin loader. Plugins are NuGet packages loaded from feeds or a local `packages/` directory at startup. This is how database providers (PostgreSQL, SQL Server, MySQL, Oracle, MongoDB), message buses (RabbitMQ, Azure Service Bus), and schedulers (Quartz) are added without recompiling the server image.
- **Configuration layering** (low → high precedence): `appsettings.json` → `/config/config.json` (volume-mounted) → environment variables.

**Studio → Server connection:** The Blazor Studio connects to the ElsaProServer API. In Docker Compose the URL is set via environment variable in the studio container. See `config/elsa-studio/` for examples.

## Dependency Management

All NuGet versions are centralized in `Directory.Packages.props` — do not add `Version=` attributes to individual `<PackageReference>` elements.

NuGet feeds (in `NuGet.config`):
- Elsa previews: `https://f.feedz.io/elsa-workflows/elsa-3/nuget/index.json`
- CShells previews: `https://f.feedz.io/sfmskywalker/cshells/nuget/index.json`

## CI/CD

`.github/workflows/build-and-push.yml` — Extracts the Elsa version from `Directory.Packages.props`, builds multi-platform Docker images via Buildx, and pushes to Docker Hub with semantic version tags. Version bumps happen by updating `Directory.Packages.props`.
