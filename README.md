# Elsa Pro Docker

A premium, production-ready Docker deployment for Elsa Workflows with hardened infrastructure and enterprise-grade tooling.

## Overview

This repository provides a containerized Elsa Workflows deployment built on .NET 10, including:
- **Elsa Workflows 3.8 preview** runtime and management APIs
- **Elsa Studio UI** for visual workflow design, available as Blazor Server or Blazor WebAssembly
- **Combined server + Studio host** for single-container deployments
- **Configurable persistence** through CShells shell features and Nuplane-loaded packages
- **CShells** multi-shell architecture
- **Nuplane** runtime plugin system — add capabilities (databases, message buses, schedulers, etc.) by configuring NuGet feeds and packages
- **OpenTelemetry** instrumentation (metrics, traces, logging)
- **Health check** endpoints for container orchestration
- **Docker Hub images** with automated CI/CD publishing

## Features

### What's in the Box

- **Workflow Runtime**: Execute workflows with HTTP triggers and JavaScript/Liquid expressions
- **Visual Designer**: Elsa Studio for building and managing workflows in the browser
- **Multi-Shell Support**: CShells architecture allows running multiple isolated workflow engines in a single host
- **Runtime Extensibility**: Nuplane loads NuGet packages at startup — add database providers, message buses, schedulers, and more without rebuilding the image
- **Identity & Security**: Built-in user management with role-based access control and per-shell admin provisioning
- **Observability**: OpenTelemetry integration for metrics, distributed traces, and structured logging
- **Health Monitoring**: `/health` and `/alive` endpoints for liveness and readiness probes

### Extending via Nuplane

The base image ships with a minimal feature set. Additional capabilities — PostgreSQL, SQL Server, RabbitMQ, Azure Service Bus, Quartz scheduling, and more — are installed at runtime as NuGet packages via Nuplane and enabled per shell through CShells feature configuration.

Configure a package feed and a list of packages in your mounted `config.json`, and Nuplane will download and load them on startup. Available packages and feed sources will be documented separately.

### Roadmap

The following capabilities are planned but not yet available:

- Hardened security defaults and container scanning
- Multi-tenancy support
- AI-assisted workflow development
- Enterprise integrations (SAP, Salesforce, etc.)
- High-availability deployment templates
- Reverse proxy configuration templates (nginx, Traefik)

## Quick Start

### Start the development stack

The fastest way to run everything is Docker Compose:

```bash
docker compose up -d
```

This starts:

| URL | Service | Purpose |
|---|---|---|
| `http://localhost:8080` | `elsa-server` | Elsa runtime and management API |
| `http://localhost:8081` | `elsa-studio` | Elsa Studio UI |
| `http://localhost:15672` | `rabbitmq` | RabbitMQ management UI |
| `http://localhost:3000` | `smtp4dev` | Development SMTP UI |

The checked-in Compose configuration runs the separate server and Studio containers. Studio uses `config/elsa-studio/config.json`, which defaults to `Studio:HostingModel = WebAssembly` and points browser traffic to `http://localhost:8080/elsa/api`.

### Choose an image

Pre-built images are published to Docker Hub under the `valenceworks` namespace:

| Image | Purpose | Contains Elsa API? | Contains Studio? | Default Studio mode |
|---|---|---:|---:|---|
| `valenceworks/elsa-pro-server` | Backend-only workflow engine for deployments where Studio is separate or not needed | Yes | No | N/A |
| `valenceworks/elsa-pro-studio` | Standalone Studio UI that connects to an Elsa API server | No | Yes | `WebAssembly` |
| `valenceworks/elsa-pro-combined` | Single-container deployment with backend API and Studio UI in one process | Yes | Yes | `WebAssembly` |

The Studio image is unified: set `Studio__HostingModel` to `WebAssembly` or `BlazorServer`.

### Run separate server and Studio containers

Use this model when you want to scale, secure, or deploy the API and UI separately.

```bash
docker pull valenceworks/elsa-pro-server:latest
docker pull valenceworks/elsa-pro-studio:latest
docker network create elsa

docker run -d \
  --network elsa \
  -p 8080:8080 \
  -e CShells__Shells__Default__Features__DefaultAdminUser__AdminUsername=admin \
  -e CShells__Shells__Default__Features__DefaultAdminUser__AdminPassword=YourSecurePassword123! \
  -e CShells__Shells__Default__Features__Identity__SigningKey=your-secure-256-bit-signing-key-here \
  -e Elsa__Cors__AllowedOrigins__0=http://localhost:8081 \
  --name elsa-server \
  valenceworks/elsa-pro-server:latest

docker run -d \
  --network elsa \
  -p 8081:8080 \
  -e Studio__HostingModel=WebAssembly \
  -e Backend__Url=http://localhost:8080/elsa/api \
  --name elsa-studio \
  valenceworks/elsa-pro-studio:latest
```

Open Studio at `http://localhost:8081`. In WebAssembly mode, the browser calls the Elsa API directly, so `Backend__Url` must be reachable from the browser. If the Studio and API are on different origins, configure CORS on the server with `Elsa__Cors__AllowedOrigins__0`.

To run the standalone Studio as Blazor Server instead, change the Studio container options:

```bash
-e Studio__HostingModel=BlazorServer \
-e Backend__Url=http://elsa-server:8080/elsa/api
```

In Blazor Server mode, the Studio container calls the API from inside the Docker network, so `Backend__Url` should use the API container name (`elsa-server`).

### Run the combined single-container image

Use this model when you want the API and Studio served from one container and one origin.

```bash
docker pull valenceworks/elsa-pro-combined:latest
docker run -d \
  -p 8080:8080 \
  -e CShells__Shells__Default__Features__DefaultAdminUser__AdminUsername=admin \
  -e CShells__Shells__Default__Features__DefaultAdminUser__AdminPassword=YourSecurePassword123! \
  -e CShells__Shells__Default__Features__Identity__SigningKey=your-secure-256-bit-signing-key-here \
  --name elsa-pro \
  valenceworks/elsa-pro-combined:latest
```

Open Studio at `http://localhost:8080`; the API is available at `http://localhost:8080/elsa/api`.

To run the combined image with Blazor Server Studio:

```bash
docker run -d \
  -p 8080:8080 \
  -e Studio__HostingModel=BlazorServer \
  -e Backend__Url=http://localhost:8080/elsa/api \
  -e CShells__Shells__Default__Features__DefaultAdminUser__AdminUsername=admin \
  -e CShells__Shells__Default__Features__DefaultAdminUser__AdminPassword=YourSecurePassword123! \
  -e CShells__Shells__Default__Features__Identity__SigningKey=your-secure-256-bit-signing-key-here \
  --name elsa-pro \
  valenceworks/elsa-pro-combined:latest
```

### Image versioning

Each image is published with multiple tags so you can pin to the level of stability you need:

| Tag pattern | Example | Description |
|---|---|---|
| `latest` | `latest` | Most recent build from `main` — always moving |
| `<version>-preview.<build>` | `1.0.0-preview.42` | Preview build from `main`, auto-increments per push |
| `<version>` | `1.0.0` | Stable release (from a git tag) |
| `<major>.<minor>` | `1.0` | Tracks the latest patch within a minor version |
| `<major>` | `1` | Tracks the latest minor+patch within a major version |
| `elsa-<elsa-version>` | `elsa-3.8.0-preview.4538` | Latest build targeting a specific Elsa version |
| `sha-<commit>` | `sha-07169a7` | Pinned to an exact commit |

### Choosing Blazor Server or Blazor WebAssembly Studio

Elsa Studio can run as either Blazor Server or Blazor WebAssembly. For separate server and Studio deployments, set `Studio__HostingModel` to `WebAssembly` or `BlazorServer` on the Studio container; the default is `WebAssembly`. Set `Backend__Url` to the appropriate Elsa API URL — browser-reachable for WebAssembly, server-reachable for Blazor Server.

| Hosting model | Use when | Image or Dockerfile | URL shape |
|---|---|---|---|
| Blazor Server | You want server-side Studio interactivity, either separately or in the combined image | `valenceworks/elsa-pro-studio` or `valenceworks/elsa-pro-combined` with `Studio__HostingModel=BlazorServer` | Separate Studio on `http://localhost:8081`, or combined API and Studio on `http://localhost:8080` |
| Blazor WebAssembly | You want browser-side Studio interactivity in either the standalone Studio or combined image | `valenceworks/elsa-pro-studio` or `valenceworks/elsa-pro-combined` with `Studio__HostingModel=WebAssembly` | Separate Studio on `http://localhost:8081` calling `Backend__Url`, or combined on `http://localhost:8080` |

### Prerequisites
- Docker 20.10 or later
- .NET 10.0 SDK (for local development)

### Building from source

1. **Clone the repository:**
```bash
git clone https://github.com/valence-works/elsa-pro-docker.git
cd elsa-pro-docker
```

2. **Build the Docker images you need:**

Backend-only server:

```bash
docker build -t elsa-pro-server -f src/ElsaProServer/Dockerfile .
```

Standalone Studio:

```bash
docker build -t elsa-pro-studio -f src/ElsaProStudio/Dockerfile .
```

Combined server + Studio:

```bash
docker build -t elsa-pro-combined -f src/ElsaProCombined/Dockerfile .
```

3. **Run separate server and Studio containers:**

```bash
docker network create elsa

docker run -d \
  --network elsa \
  -p 8080:8080 \
  -e CShells__Shells__Default__Features__DefaultAdminUser__AdminUsername=admin \
  -e CShells__Shells__Default__Features__DefaultAdminUser__AdminPassword=YourSecurePassword123! \
  -e CShells__Shells__Default__Features__Identity__SigningKey=your-secure-256-bit-signing-key-here \
  --name elsa-server \
  elsa-pro-server

docker run -d \
  --network elsa \
  -p 8081:8080 \
  -e Studio__HostingModel=WebAssembly \
  -e Backend__Url=http://localhost:8080/elsa/api \
  --name elsa-studio \
  elsa-pro-studio
```

To run the combined image instead:

```bash
docker run -d \
  -p 8080:8080 \
  -e CShells__Shells__Default__Features__DefaultAdminUser__AdminUsername=admin \
  -e CShells__Shells__Default__Features__DefaultAdminUser__AdminPassword=YourSecurePassword123! \
  -e CShells__Shells__Default__Features__Identity__SigningKey=your-secure-256-bit-signing-key-here \
  --name elsa-pro \
  elsa-pro-combined
```

4. **Access the server and Studio:**
- Separate server API: `http://localhost:8080/elsa/api`
- Separate Studio UI: `http://localhost:8081`
- Combined Studio UI: `http://localhost:8080`
- Combined API: `http://localhost:8080/elsa/api`
- Health check: `http://localhost:8080/health`

### Running Locally (Development)

1. **Restore dependencies:**
```bash
dotnet restore
```

2. **Optionally override the default shell admin user and signing key:**
```bash
export CShells__Shells__Default__Features__DefaultAdminUser__AdminUsername=admin
export CShells__Shells__Default__Features__DefaultAdminUser__AdminPassword=YourSecurePassword123!
export CShells__Shells__Default__Features__Identity__SigningKey=your-secure-256-bit-signing-key-here
```

3. **Run a project:**

Backend-only server:

```bash
cd src/ElsaProServer
dotnet run
```

Standalone Studio:

```bash
cd src/ElsaProStudio
dotnet run
```

Combined server + Studio:

```bash
cd src/ElsaProCombined
dotnet run
```

## Configuration

### Environment Variables

| Variable | Description | Default | Required |
|----------|-------------|---------|----------|
| `CShells__Shells__Default__Features__DefaultAdminUser__AdminUsername` | Default shell admin username | `admin` | Recommended override |
| `CShells__Shells__Default__Features__DefaultAdminUser__AdminPassword` | Default shell admin password | `password` | Recommended override |
| `CShells__Shells__Default__Features__DefaultAdminUser__AdminRoleName` | Default shell admin role name | `admin` | No |
| `CShells__Shells__Default__Features__Identity__SigningKey` | Default shell identity signing key | Placeholder | Yes for production |
| `Studio__HostingModel` | Studio hosting model: `WebAssembly` or `BlazorServer` | `WebAssembly` | No |
| `Backend__Url` | Elsa API URL — browser-reachable for WebAssembly, server-reachable for Blazor Server | appsettings value | Required for standalone Studio |
| `Elsa__Cors__AllowedOrigins__0` | First allowed CORS origin | appsettings value | No |
| `ASPNETCORE_ENVIRONMENT` | ASP.NET Core environment | Production | No |
| `ASPNETCORE_URLS` | Server URLs | http://+:8080 | No |

### Admin User Provisioning

Admin users are configured per shell. The default shell uses the `DefaultAdminUser` feature:

```json
{
  "CShells": {
    "Shells": {
      "Default": {
        "Features": {
          "DefaultAdminUser": {
            "AdminUsername": "admin",
            "AdminPassword": "YourSecurePassword123!",
            "AdminRoleName": "admin",
            "AdminRolePermissions": [
              "*"
            ]
          }
        }
      }
    }
  }
}
```

You can also manage users through:
- Direct API calls to identity endpoints

Coming:

- Manage users, roles and permissions from the Elsa Studio web application

### Mounted configuration file (`/config/config.json`)

All application images load an optional JSON file from `/config/config.json` inside the container. This is the recommended way to supply configuration in Docker environments — it avoids long lists of `-e` flags and keeps secrets out of the process environment.

**Configuration precedence (last-wins):**
1. `appsettings.json` (baked into the image)
2. `appsettings.{Environment}.json` (baked into the image)
3. `/config/config.json` (your mount)
4. Environment variables (highest precedence)

**Docker run:**
```bash
docker run ... -v $(pwd)/config.json:/config/config.json elsa-pro-server
```

**Docker Compose:**
```yaml
volumes:
  - ./config.json:/config/config.json
```

A fully annotated template is provided at [`config.example.json`](config.example.json). Copy it, remove the comments, and mount it:
```bash
cp config.example.json config.json
# edit config.json with your values
```

### Connection Strings

Configure database providers per shell through feature configuration. The checked-in Docker Compose configuration enables PostgreSQL workflow and identity persistence for the default shell:

```json
{
  "CShells": {
    "Shells": {
      "Default": {
        "Features": {
          "PostgreSqlWorkflowPersistence": {
            "ConnectionString": "Host=postgres;Port=5432;Database=elsa;Username=elsa;Password=elsa"
          },
          "PostgreSqlIdentityPersistence": {
            "ConnectionString": "Host=postgres;Port=5432;Database=elsa;Username=elsa;Password=elsa"
          }
        }
      }
    }
  }
}
```

To use a different database, install or load the corresponding package and enable the matching shell feature with its connection string.

### Identity Signing Key

For production deployments, set a secure signing key:

```bash
CShells__Shells__Default__Features__Identity__SigningKey="your-secure-256-bit-signing-key-here"
```

## Docker Compose

The included `docker-compose.yml` brings up separate server and Studio containers with supporting infrastructure for development:

| Service | Port | Purpose |
|---------|------|---------|
| `elsa-server` | 8080 | Elsa Workflows API server |
| `elsa-studio` | 8081 | Elsa Studio workflow designer |

The compose file starts the server and Studio plus local infrastructure services (PostgreSQL, SQL Server, MySQL, Oracle, MongoDB, RabbitMQ, Redis, SMTP4Dev) for development and testing. The checked-in server config mounted from `config/elsa-server/config.json` currently enables PostgreSQL persistence, RabbitMQ messaging, Quartz PostgreSQL scheduling, and the sample endpoint for the default shell.

In Docker Compose, Studio gets its hosting model and backend URL from `config/elsa-studio/config.json`. The checked-in config defaults to `WebAssembly` with `Backend:Url` set to `http://localhost:8080/elsa/api` (browser-reachable).

```bash
docker compose up -d
```

To switch Compose Studio to Blazor Server, set `Studio:HostingModel` to `BlazorServer` and change `Backend:Url` to `http://elsa-server:8080/elsa/api` (server-reachable via the Docker network).

## API Documentation

Once running, the Elsa server provides the following endpoints:
- Base URL: `http://localhost:8080`
- Health endpoint: `http://localhost:8080/health`

For full API documentation and workflow management, connect the Elsa Studio at `http://localhost:8081`.

## Security Best Practices

1. **Generate a secure signing key**: The default shell's `CShells__Shells__Default__Features__Identity__SigningKey` must be set to a secure randomly generated value (256-bit minimum). Never use the placeholder value in production.
2. **Configure CORS properly**: CORS origins are read from `Elsa:Cors:AllowedOrigins`. Use `*` only for development; in production, set specific trusted domains only.
3. **Change default credentials**: Always override the default shell admin username and password.
4. **Use environment variables**: Never commit credentials or keys in code or configuration files
5. **HTTPS in production**: Use a reverse proxy (nginx, Traefik) with TLS certificates
6. **Network isolation**: Run in a private network when possible
7. **Regular updates**: Keep the Elsa packages and base images updated
8. **Database security**: Use connection strings with authentication for production databases

## Data Persistence

The active persistence location depends on the shell persistence feature you enable. With the checked-in Docker Compose configuration, workflow and identity data are stored in PostgreSQL. If you configure SQLite with `Data Source=/data/elsa.db`, persist it across container restarts with:
- Use a Docker volume (recommended): `-v elsa-data:/data`
- Mount a host directory: `-v ./data:/data`

For production workloads, install a database provider package via Nuplane (PostgreSQL, SQL Server, etc.) and configure the connection string accordingly.

## Troubleshooting

### Container won't start
Check logs:
```bash
docker logs elsa-server
```

### Database errors
Ensure the configured shell persistence provider is loaded, enabled, and reachable. For the checked-in Compose setup, verify PostgreSQL is running and the default shell connection strings point to `Host=postgres`.

### Super admin not created
Verify the default shell's `DefaultAdminUser` feature is configured in the mounted config file or environment:
```bash
docker exec elsa-server printenv | grep 'CShells__Shells__Default__Features__DefaultAdminUser'
```

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Support

- [GitHub Issues](https://github.com/valence-works/elsa-pro-docker/issues) — bug reports and feature requests
- [GitHub Discussions](https://github.com/valence-works/elsa-pro-docker/discussions) — questions and community help

## Links

- [Elsa Workflows Official Documentation](https://elsa-workflows.github.io/elsa-core/)
- [Elsa Workflows GitHub](https://github.com/elsa-workflows/elsa-core)
- [Community Forum](https://github.com/elsa-workflows/elsa-core/discussions)
