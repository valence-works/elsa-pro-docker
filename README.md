# Elsa Pro Docker

A premium, production-ready Docker deployment for Elsa Workflows with hardened infrastructure and enterprise-grade tooling.

## Overview

This repository provides a containerized Elsa Workflows deployment built on .NET 10, including:
- **Elsa Workflows 3.8** runtime and management APIs
- **Blazor Server Studio UI** for visual workflow design
- **SQLite** persistence out of the box
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
- **Identity & Security**: Built-in user management with role-based access control and automatic admin provisioning
- **Observability**: OpenTelemetry integration for metrics, distributed traces, and structured logging
- **Health Monitoring**: `/health` and `/alive` endpoints for liveness and readiness probes

### Extending via Nuplane

The base image ships with SQLite persistence and a minimal feature set. Additional capabilities — PostgreSQL, SQL Server, RabbitMQ, Azure Service Bus, Quartz scheduling, and more — are installed at runtime as NuGet packages via Nuplane.

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
  -e ELSA_ADMIN_EMAIL=admin@example.com \
  -e ELSA_ADMIN_PASSWORD=YourSecurePassword123! \
  -v elsa-data:/app/data \
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
  -e ELSA_ADMIN_EMAIL=admin@example.com \
  -e ELSA_ADMIN_PASSWORD=YourSecurePassword123! \
  -v elsa-data:/app/data \
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

2. **Set environment variables:**
```bash
export ELSA_ADMIN_EMAIL=admin@example.com
export ELSA_ADMIN_PASSWORD=YourSecurePassword123!
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
| `ELSA_ADMIN_EMAIL` | Super admin email/username | - | Recommended |
| `ELSA_ADMIN_PASSWORD` | Super admin password | - | Recommended |
| `ASPNETCORE_ENVIRONMENT` | ASP.NET Core environment | Production | No |
| `ASPNETCORE_URLS` | Server URLs | http://+:8080 | No |

### Admin User Provisioning

When `ELSA_ADMIN_EMAIL` and `ELSA_ADMIN_PASSWORD` are set, the server automatically creates an admin user at startup with full permissions. The email is logged at startup for reference (the password is not logged).

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

Configure the database connection via the mounted config file or an environment variable. The default is SQLite:

```bash
ConnectionStrings__Elsa="Data Source=/app/data/elsa.db"
```

To use a different database (PostgreSQL, SQL Server, etc.), install the corresponding Nuplane package and update the connection string accordingly.

### Identity Signing Key

For production deployments, set a secure signing key:

```bash
Elsa__Identity__SigningKey="your-secure-256-bit-signing-key-here"
```

## Docker Compose

The included `docker-compose.yml` brings up the server and studio with supporting infrastructure for development:

| Service | Port | Purpose |
|---------|------|---------|
| `elsa-server` | 8080 | Elsa Workflows API server |
| `elsa-studio` | 8081 | Blazor Server workflow designer |

The compose file also includes optional infrastructure services (PostgreSQL, SQL Server, MySQL, Oracle, MongoDB, RabbitMQ, Redis, SMTP4Dev) for local development and testing. These are not required for the base deployment — the server runs with SQLite out of the box.

```bash
docker compose up -d
```

## API Documentation

Once running, the Elsa server provides the following endpoints:
- Base URL: `http://localhost:8080`
- Health endpoint: `http://localhost:8080/health`

For full API documentation and workflow management, connect the Elsa Studio at `http://localhost:8081`.

## Security Best Practices

1. **Generate a secure signing key**: The `Elsa__Identity__SigningKey` must be set to a secure randomly generated value (256-bit minimum). Never use the placeholder value in production.
2. **Configure CORS properly**: By default, CORS allows all origins (*) for development convenience. In production, set `Elsa__Cors__AllowedOrigins` to specific trusted domains only.
3. **Change default credentials**: Always set strong passwords for the admin account
4. **Use environment variables**: Never commit credentials or keys in code or configuration files
5. **HTTPS in production**: Use a reverse proxy (nginx, Traefik) with TLS certificates
6. **Network isolation**: Run in a private network when possible
7. **Regular updates**: Keep the Elsa packages and base images updated
8. **Database security**: Use connection strings with authentication for production databases

## Data Persistence

The default database is SQLite, stored at `/app/data/elsa.db`. To persist data across container restarts:
- Use a Docker volume (recommended): `-v elsa-data:/app/data`
- Mount a host directory: `-v ./data:/app/data`

For production workloads, install a database provider package via Nuplane (PostgreSQL, SQL Server, etc.) and configure the connection string accordingly.

## Troubleshooting

### Container won't start
Check logs:
```bash
docker logs elsa-server
```

### Database errors
If using the default SQLite, ensure the `/app/data` volume is mounted and writable. If using an external database via Nuplane, ensure the database service is running and reachable.

### Super admin not created
Verify environment variables are set:
```bash
docker exec elsa-server printenv | grep ELSA
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
