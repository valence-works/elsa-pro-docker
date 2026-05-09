# AGENTS.md

Guidance for coding agents working in this repository.

## Project Overview

This repository packages Elsa Workflows 3.x as production-ready Docker images:

- `valenceworks/elsa-pro-server`: Elsa runtime and management API.
- `valenceworks/elsa-pro-studio`: Unified Studio workflow designer UI.

The codebase targets .NET 10 and is organized around a Docker-first deployment model. There are currently no dedicated test projects; validate changes with builds and, when relevant, by running the Docker stack.

## Repository Layout

- `src/ElsaProServer`: Elsa runtime API server, Docker image, CShells/Nuplane configuration.
- `src/ElsaProStudio`: Unified Studio UI and Docker image.
- `src/ElsaProServer.Identity`: Identity, roles, and admin user provisioning.
- `src/ElsaProServer.ServiceDefaults`: Shared Aspire service defaults, observability, health checks, resilience.
- `src/ElsaProServer.AppHost`: Aspire local orchestration host.
- `src/SamplePackage`: Reference Nuplane plugin package.
- `config/`: Example mounted runtime configuration.
- `.github/workflows/build-and-push.yml`: Multi-platform Docker image build and Docker Hub publishing.

## Common Commands

Build the server:

```bash
dotnet build src/ElsaProServer/ElsaProServer.csproj
```

Build the Studio UI:

```bash
dotnet build src/ElsaProStudio/ElsaProStudio.csproj
```

Run the server locally:

```bash
cd src/ElsaProServer
dotnet run
```

Build Docker images:

```bash
docker build -t elsa-pro-server -f src/ElsaProServer/Dockerfile .
docker build -t elsa-pro-studio -f src/ElsaProStudio/Dockerfile .
```

Run the full development stack:

```bash
docker compose up -d
```

Default local endpoints:

- Server API: `http://localhost:8080`
- Studio UI: `http://localhost:8081`
- RabbitMQ management: `http://localhost:15672`
- SMTP4Dev: `http://localhost:3000`

Required environment variables are listed in `.env.example`; at minimum set `ELSA_ADMIN_EMAIL` and `ELSA_ADMIN_PASSWORD` for the compose stack.

## Architecture Notes

- CShells provides the multi-shell abstraction inside `ElsaProServer`. Each shell is an isolated Elsa workflow engine instance configured from `Program.cs` and runtime configuration.
- Nuplane loads runtime plugins from NuGet feeds or the local `packages/` directory. Use it for database providers, message buses, schedulers, and other optional capabilities without rebuilding the base image.
- Configuration precedence is `appsettings.json`, then `/config/config.json`, then environment variables.
- Studio connects to the server API through configuration. In Docker Compose this is provided via environment variables on the Studio container.
- SQLite is the default persistence provider. Other databases require the corresponding Nuplane package and connection string configuration.

## Dependency Management

- NuGet package versions are centralized in `Directory.Packages.props`.
- Do not add `Version=` attributes to individual `<PackageReference>` entries.
- NuGet feeds are configured in `NuGet.config`, including Elsa and CShells preview feeds.
- Version bumps for published images are driven by package versions in `Directory.Packages.props` and the CI workflow.

## Coding Guidelines

- Do not assume, hide confusion, or flatten uncertainty; surface questions, constraints, and tradeoffs explicitly.
- Define success criteria before implementation, then iterate until the criteria are verified or clearly state what could not be verified.
- Write the minimum code that solves the defined problem; do not add speculative abstractions, features, or cleanup.
- Keep changes narrowly scoped to the requested behavior.
- Touch only the files and behavior required for the task; clean up only issues introduced by your own changes.
- Preserve the Docker-first deployment assumptions unless explicitly changing deployment behavior.
- Avoid committing generated build output, local databases, IDE metadata, or temporary package artifacts.
- Update README/config examples when changing user-facing configuration, ports, environment variables, or Docker behavior.
- Prefer explicit, conventional .NET configuration binding over ad-hoc environment variable parsing.
- Keep startup and plugin-loading behavior observable with clear logging where failures would otherwise be hard to diagnose.
- When adding packages, update `Directory.Packages.props` and keep project files versionless.

## Validation

Use the narrowest validation that exercises the change:

- For server changes: `dotnet build src/ElsaProServer/ElsaProServer.csproj`.
- For Studio changes: `dotnet build src/ElsaProStudio/ElsaProStudio.csproj`.
- For shared package or dependency changes: build affected projects or the solution.
- For Docker/runtime configuration changes: build the relevant image and/or run `docker compose up -d`.

If validation cannot be run, state why and identify the most relevant command for the user to run later.

## Git Hygiene

- Do not overwrite or revert unrelated local changes.
- Check worktree status before editing when the task may overlap with existing changes.
- Do not amend commits or run destructive git commands unless explicitly requested.
- Keep generated files out of commits unless they are intentionally versioned artifacts.
