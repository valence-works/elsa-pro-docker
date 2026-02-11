# Elsa Pro Docker

A production-ready Docker deployment for Elsa Workflows Pro & Enterprise edition server with ASP.NET Core.

## Overview

This repository provides a containerized Elsa Workflows server implementation with:
- ASP.NET Core Web API hosting
- Entity Framework Core with SQLite persistence
- Workflow management and runtime APIs
- HTTP activities support
- JavaScript and Liquid expression support
- Built-in authentication and identity management
- Health check endpoints
- Docker deployment ready

## Features

### Core Capabilities
- **Workflow Management**: Create, edit, and manage workflows through REST APIs
- **Workflow Runtime**: Execute workflows with full runtime support
- **HTTP Activities**: Build HTTP-triggered workflows and webhooks
- **Expression Languages**: Support for JavaScript and Liquid templating
- **Identity & Security**: Built-in user management and role-based access control
- **Persistence**: SQLite database with Entity Framework Core
- **Health Monitoring**: Health check endpoints for container orchestration

### Pro & Enterprise Tiers
This deployment is compatible with Elsa Workflows Pro and Enterprise editions, which provide additional features:

**Elsa Pro Features:**
- Advanced workflow designer UI
- Enhanced debugging capabilities
- Priority support
- Additional activity libraries
- Advanced scheduling options

**Elsa Enterprise Features:**
- AI Assistant for workflow development
- Multi-tenancy support
- Advanced integrations (SAP, Salesforce, etc.)
- High-availability configurations
- Enterprise-grade SLA
- Dedicated support team

For more information about Pro and Enterprise tiers, visit [https://elsa-workflows.github.io/elsa-core/](https://elsa-workflows.github.io/elsa-core/)

## Quick Start

### Prerequisites
- Docker 20.10 or later
- .NET 10.0 SDK (for local development)

### Running with Docker

1. **Clone the repository:**
```bash
git clone https://github.com/valence-works/elsa-pro-docker.git
cd elsa-pro-docker
```

2. **Build the Docker image:**
```bash
docker build -t elsa-pro-server .
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

**Note:** Admin credentials are logged at startup for reference. You'll need to create the admin user via the Identity API after the server starts.

### Creating the Super Admin User

The environment variables `ELSA_ADMIN_EMAIL` and `ELSA_ADMIN_PASSWORD` are logged at startup for reference. After the server starts, you can create the admin user using the Elsa Identity management features.

**Note:** Admin user creation via the Identity API requires the Elsa Pro or Enterprise license which includes the full management API. For community edition deployments, you can connect a separate Elsa Studio or Designer application to manage workflows and users.

For Pro/Enterprise deployments with the full API enabled, you would create users via:
- The Elsa Studio web application
- Direct API calls to identity endpoints
- Custom initialization code using Elsa's identity services

Example workflow:
1. Start the server with admin credentials in environment variables
2. The credentials are logged for your reference
3. Use an Elsa Studio instance to connect to this server
4. Create the admin user through the Studio UI

Alternatively, for custom implementations, you can extend the `Program.cs` startup code to programmatically create admin users using Elsa's `IUserProvider` and `IRoleProvider` services.

### Connection Strings

Configure the database connection in `appsettings.json` or via environment variable:

```bash
ConnectionStrings__Elsa="Data Source=/app/data/elsa.db"
```

### Identity Signing Key

For production deployments, set a secure signing key:

```bash
Elsa__Identity__SigningKey="your-secure-256-bit-signing-key-here"
```

## Docker Compose Example

Create a `docker-compose.yml` file:

```yaml
version: '3.8'

services:
  elsa-server:
    build: .
    ports:
      - "8080:8080"
    environment:
      - ELSA_ADMIN_EMAIL=admin@example.com
      - ELSA_ADMIN_PASSWORD=SecurePassword123!
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__Elsa=Data Source=/app/data/elsa.db
      - Elsa__Identity__SigningKey=change-this-to-a-secure-key-in-production
    volumes:
      - elsa-data:/app/data
    restart: unless-stopped
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:8080/health"]
      interval: 30s
      timeout: 10s
      retries: 3
      start_period: 40s

volumes:
  elsa-data:
```

Run with:
```bash
docker-compose up -d
```

## API Documentation

Once running, the Elsa server provides the following endpoints:
- Base URL: `http://localhost:8080`
- Health endpoint: `http://localhost:8080/health`

For full API documentation and workflow management, connect an Elsa Studio application or use the Pro/Enterprise management APIs. The server provides HTTP workflow triggers and can execute workflows programmatically.

## Security Best Practices

1. **Change default credentials**: Always set strong passwords for the admin account
2. **Use environment variables**: Never commit credentials in code
3. **Secure signing key**: Generate a strong signing key for JWT tokens
4. **HTTPS in production**: Use a reverse proxy (nginx, Traefik) with TLS
5. **Network isolation**: Run in a private network when possible
6. **Regular updates**: Keep the Elsa packages and base images updated

## Data Persistence

The SQLite database is stored in `/app/data/elsa.db` by default. To persist data across container restarts:
- Use a Docker volume (recommended)
- Mount a host directory
- Use a different database provider (PostgreSQL, SQL Server, etc.)

## Troubleshooting

### Container won't start
Check logs:
```bash
docker logs elsa-server
```

### Database locked errors
Ensure only one container is accessing the SQLite database, or switch to a client-server database like PostgreSQL.

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

- **Community Edition**: [GitHub Issues](https://github.com/elsa-workflows/elsa-core/issues)
- **Pro Edition**: Priority email support
- **Enterprise Edition**: Dedicated support team with SLA

## Links

- [Elsa Workflows Official Documentation](https://elsa-workflows.github.io/elsa-core/)
- [Elsa Workflows GitHub](https://github.com/elsa-workflows/elsa-core)
- [Community Forum](https://github.com/elsa-workflows/elsa-core/discussions)

