# PeFi Dynamic DNS

A .NET 9 microservice that automatically manages DNS records by monitoring public IP address changes and responding to service lifecycle events via a RabbitMQ message broker.

## Overview

PeFi Dynamic DNS performs two main functions:

1. **IP Address Monitoring** – Polls the public IP address every 2 minutes and updates a DNS A record whenever the IP changes.
2. **Service Event Handling** – Listens for service creation and deletion events on a RabbitMQ topic and creates or removes corresponding DNS CNAME records.

The service uses [DNSimple](https://dnsimple.com/) as the DNS provider.

## Architecture

```
┌─────────────────────────────────────────────────────┐
│                  pefi.dynamicdns                    │
│                                                     │
│  ScheduledIpCheck ──► ipify.org ──► DNSimple API    │
│                                                     │
│  EventListener ──► RabbitMQ ──► DNSimple API        │
│                       │                             │
│                 ServiceManagerClient                │
└─────────────────────────────────────────────────────┘
```

| Component | Description |
|-----------|-------------|
| `ScheduledIpCheck` | Background service; checks public IP every 2 minutes and updates the DNS A record when changed |
| `EventListener` | Background service; subscribes to RabbitMQ for `events.service.created` and `events.service.deleted` events |
| `DNSimpleClient` | DNS provider implementation using the DNSimple API |
| `ServiceManagerClient` | Auto-generated HTTP client for fetching service details from the Service Manager API |

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- A [DNSimple](https://dnsimple.com/) account with an OAuth token
- A running [RabbitMQ](https://www.rabbitmq.com/) instance
- Access to the `pefi.messaging.rabbit`, `pefi.http`, and `pefi.observability` NuGet packages hosted on GitHub Packages (requires a GitHub personal access token)

## Configuration

The service is configured through environment variables:

| Variable | Description |
|----------|-------------|
| `Messaging__Username` | RabbitMQ username |
| `Messaging__Password` | RabbitMQ password |
| `Messaging__Address` | RabbitMQ broker address (e.g. `amqp://rabbitmq:5672`) |
| `ServiceManager__baseurl` | Base URL of the Service Manager API |

## Building

### Local Build

```bash
# Add the private NuGet source (first time only)
dotnet nuget add source \
  --username <github_username> \
  --password <github_token> \
  --store-password-in-clear-text \
  --name petefield \
  "https://nuget.pkg.github.com/petefield/index.json"

cd pefi.dynamicdns
dotnet restore
dotnet build -c Release
```

### Docker Build

The Docker build uses [BuildKit secrets](https://docs.docker.com/build/building/secrets/) to securely pass a GitHub token for accessing private NuGet packages.

```bash
docker buildx build \
  --platform linux/amd64,linux/arm64 \
  --build-arg BUILD_CONFIGURATION=Release \
  --secret id=github_token,src=<path-to-token-file> \
  -t ghcr.io/petefield/pefi.dynamicdns:latest \
  ./pefi.dynamicdns
```

## Running

### Locally

```bash
cd pefi.dynamicdns
dotnet run
```

### Docker

```bash
docker run \
  -e Messaging__Username=<user> \
  -e Messaging__Password=<pass> \
  -e Messaging__Address=amqp://rabbitmq:5672 \
  -e ServiceManager__baseurl=http://service-manager:5000 \
  ghcr.io/petefield/pefi.dynamicdns:latest
```

## Running Tests

Unit tests are located in the `pefi.dynamicdns.Tests` project. They require access to the private NuGet packages, so you must configure the GitHub Packages source before running them.

```bash
# Add the private NuGet source (first time only)
dotnet nuget add source \
  --username <github_username> \
  --password <github_token> \
  --store-password-in-clear-text \
  --name petefield \
  "https://nuget.pkg.github.com/petefield/index.json"

# Run all tests
dotnet test pefi.dynamicdns.Tests/pefi.dynamicdns.Tests.csproj
```

Tests cover:

| Test Class | What it Tests |
|------------|---------------|
| `ScheduledIpCheckTests` | IP change detection, DNS update triggering, cancellation handling |
| `IpAddressLookupTests` | HTTP response parsing, error handling |
| `DnsMessageHandlerTests` | CNAME record creation on service created, DNS deletion on service deleted |
| `ModelTests` | Data model construction and equality |

## CI/CD

The repository includes a GitHub Actions workflow that:
1. Builds and runs all unit tests on every push and pull request
2. Builds and publishes a multi-platform Docker image (`linux/amd64` and `linux/arm64`) to the GitHub Container Registry on every push to `main`.

## License

This project is private and maintained by [@petefield](https://github.com/petefield).
