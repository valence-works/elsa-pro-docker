# Elsa Pro Docker

A premium, production-ready Docker deployment for Elsa Workflows with hardened infrastructure and enterprise-grade tooling.

## Overview

This repository provides a containerized Elsa Workflows deployment built on .NET 10, including:
- **Elsa Workflows 3.8 preview** runtime and management APIs
- **Blazor Server Studio UI** for visual workflow design
- **Configurable persistence** through CShells shell features and Nuplane-loaded packages
- **CShells** multi-shell architecture
- **Nuplane** runtime plugin system — add capabilities (databases, message buses, schedulers, etc.) by configuring NuGet feeds and packages
- **OpenTelemetry** instrumentation (metrics, traces, logging)
- **Health check** endpoints for container orchestration
- **Docker Hub images** with automated CI/CD publishing

## Features

### What's in the Box

- **Workflow Runtime**: Execute workflows with HTTP triggers and JavaScript/Liquid expressions
- **Visual Designer**: Blazor Server Studio for building and managing workflows in the browser
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

### Using pre-built images from Docker Hub

Pre-built images are published to Docker Hub under the `valenceworks` namespace.

**Pull and run the server:**
```bash
docker pull valenceworks/elsa-pro-server:latest
docker run -d \
  -p 8080:8080 \
  -e CShells__Shells__0__Features__DefaultAdminUser__AdminUsername=admin \
  -e CShells__Shells__0__Features__DefaultAdminUser__AdminPassword=YourSecurePassword123! \
  -e CShells__Shells__0__Features__Identity__SigningKey=your-secure-256-bit-signing-key-here \
  --name elsa-server \
  valenceworks/elsa-pro-server:latest
```

**Pull and run the studio:**
```bash
docker pull valenceworks/elsa-pro-studio-blazorserver:latest
docker run -d \
  -p 8081:8080 \
  --name elsa-studio \
  valenceworks/elsa-pro-studio-blazorserver:latest
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

**Available images:**

| Image | Description |
|---|---|
| `valenceworks/elsa-pro-server` | Elsa Pro API server |
| `valenceworks/elsa-pro-studio-blazorserver` | Elsa Pro Studio (Blazor Server) |

### Prerequisites
- Docker 20.10 or later
- .NET 10.0 SDK (for local development)

### Building from source

1. **Clone the repository:**
```bash
git clone https://github.com/valence-works/elsa-pro-docker.git
cd elsa-pro-docker
```

2. **Build the Docker image:**
```bash
docker build -t elsa-pro-server -f src/ElsaProServer/Dockerfile .
```

3. **Run the container:**
```bash
docker run -d \
  -p 8080:8080 \
  -e CShells__Shells__0__Features__DefaultAdminUser__AdminUsername=admin \
  -e CShells__Shells__0__Features__DefaultAdminUser__AdminPassword=YourSecurePassword123! \
  -e CShells__Shells__0__Features__Identity__SigningKey=your-secure-256-bit-signing-key-here \
  --name elsa-server \
  elsa-pro-server
```

4. **Access the server:**
- Workflows API: `http://localhost:8080/elsa/api`
- Health check: `http://localhost:8080/health`

### Running Locally (Development)

1. **Restore dependencies:**
```bash
dotnet restore
```

2. **Optionally override the default shell admin user and signing key:**
```bash
export CShells__Shells__0__Features__DefaultAdminUser__AdminUsername=admin
export CShells__Shells__0__Features__DefaultAdminUser__AdminPassword=YourSecurePassword123!
export CShells__Shells__0__Features__Identity__SigningKey=your-secure-256-bit-signing-key-here
```

3. **Run the application:**
```bash
cd src/ElsaProServer
dotnet run
```

## Configuration

### Environment Variables

| Variable | Description | Default | Required |
|----------|-------------|---------|----------|
| `CShells__Shells__0__Features__DefaultAdminUser__AdminUsername` | Default shell admin username | `admin` | Recommended override |
| `CShells__Shells__0__Features__DefaultAdminUser__AdminPassword` | Default shell admin password | `password` | Recommended override |
| `CShells__Shells__0__Features__DefaultAdminUser__AdminRoleName` | Default shell admin role name | `admin` | No |
| `CShells__Shells__0__Features__Identity__SigningKey` | Default shell identity signing key | Placeholder | Yes for production |
| `Elsa__Cors__AllowedOrigins__0` | First allowed CORS origin | appsettings value | No |
| `ASPNETCORE_ENVIRONMENT` | ASP.NET Core environment | Production | No |
| `ASPNETCORE_URLS` | Server URLs | http://+:8080 | No |

### Admin User Provisioning

Admin users are configured per shell. The default shell uses the `DefaultAdminUser` feature:

```json
{
  "CShells": {
    "Shells": [
      {
        "Name": "Default",
        "Features": {
          "DefaultAdminUser": {
            "AdminUsername": "admin",
            "AdminPassword": "YourSecurePassword123!",
            "AdminRoleName": "admin",
            "AdminRolePermissions": ["*"]
          }
        }
      }
    ]
  }
}
```

The old `ELSA_ADMIN_EMAIL` variable is not used. There is also an `AdminUserFeature` implementation that reads `ELSA_ADMIN_USER` and `ELSA_ADMIN_PASSWORD`, but it only runs when that shell feature is enabled; the default configuration uses `DefaultAdminUser` instead.

You can also manage users through:
- The Elsa Studio web application
- Direct API calls to identity endpoints

### Mounted configuration file (`/config/config.json`)

Both services load an optional JSON file from `/config/config.json` inside the container. This is the recommended way to supply configuration in Docker environments — it avoids long lists of `-e` flags and keeps secrets out of the process environment.

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
    "Shells": [
      {
        "Name": "Default",
        "Features": {
          "PostgreSqlWorkflowPersistence": {
            "ConnectionString": "Host=postgres;Port=5432;Database=elsa;Username=elsa;Password=elsa"
          },
          "PostgreSqlIdentityPersistence": {
            "ConnectionString": "Host=postgres;Port=5432;Database=elsa;Username=elsa;Password=elsa"
          }
        }
      }
    ]
  }
}
```

To use a different database, install or load the corresponding Nuplane package and enable the matching shell feature with its connection string.

### Identity Signing Key

For production deployments, set a secure signing key:

```bash
CShells__Shells__0__Features__Identity__SigningKey="your-secure-256-bit-signing-key-here"
```

## Docker Compose

The included `docker-compose.yml` brings up the server and studio with supporting infrastructure for development:

| Service | Port | Purpose |
|---------|------|---------|
| `elsa-server` | 8080 | Elsa Workflows API server |
| `elsa-studio` | 8081 | Blazor Server workflow designer |

The compose file starts the server and Studio plus local infrastructure services (PostgreSQL, SQL Server, MySQL, Oracle, MongoDB, RabbitMQ, Redis, SMTP4Dev) for development and testing. The checked-in server config mounted from `config/elsa-server/config.json` currently enables PostgreSQL persistence, RabbitMQ messaging, Quartz PostgreSQL scheduling, and the sample endpoint for the default shell.

```bash
docker compose up -d
```

## API Documentation

Once running, the Elsa server provides the following endpoints:
- Base URL: `http://localhost:8080`
- Health endpoint: `http://localhost:8080/health`

For full API documentation and workflow management, connect the Elsa Studio at `http://localhost:8081`.

## Security Best Practices

1. **Generate a secure signing key**: The default shell's `CShells__Shells__0__Features__Identity__SigningKey` must be set to a secure randomly generated value (256-bit minimum). Never use the placeholder value in production.
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
docker exec elsa-server printenv | grep 'CShells__Shells__0__Features__DefaultAdminUser'
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
