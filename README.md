# EShop SaaS - Microservices Architecture

## Setup infrastructure local
> Run docker-compose to build Infrastructure such as Redis, MSSQL Server, RabbitMQ, Seq for development environment.
```
docker-compose -f docker-compose.Development.Infrastructure.yaml up -d
```

## Design
Draw.io tool: https://app.diagrams.net/#G1098H9qGbXbgYVJYgcyvB-nhbhj61Z_aA#%7B%22pageId%22%3A%22UFtXiaEUb5W2klNUkZGj%22%7D

### Domain Driven Design

### Event Storming

### Event Sourcing

## Best pratices and supports
### Clean Architecture

### Cross Cutting Concern
| STT     | Name                             |
|---------|----------------------------------|
| 1       | Authentication and Authorization |
| 2       | Logging and tracing              |
| 3       | Exception Handling               |
| 4       | Validation                       |
| 5       | Caching                          |

### Multi-tenancy

### Unit testing and Behavior driven design

### Trace Logs for Audit and Business

## Central Package Management to enhance migration
```
# Powershell script. Scan all .csproj files and aggregate unique package versions
$packages = Get-ChildItem -Filter *.csproj -Recurse |
    Get-Content |
    Select-String -Pattern '<PackageReference Include="([^"]+)" Version="([^"]+)"' -AllMatches |
    ForEach-Object { $_.Matches } |
    Group-Object { $_.Groups[1].Value } |
    ForEach-Object { @{
        Name = $_.Name
        Versions = $_.Group.ForEach({ $_.Groups[2].Value }) | Select-Object -Unique
    }} |
    Sort-Object { $_.Name }

# Display results
$packages | ForEach-Object {
    "$($_.Name) versions:"
    $_.Versions | ForEach-Object { "  $_" }
}

# Centralized package management to ensure consistent library versions across entry projects.
```

## Tenancy Service

## Authorization

## Catalog
