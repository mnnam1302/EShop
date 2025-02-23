# EShop Kodi Project

## Setup infrastructure local
> Run docker-compose to build Infrastructure such as Redis, MSSQL Server, RabbitMQ, Seq for development environment.
```
docker-compose -f docker-compose.Dev.Infrastructure.yml up -d
```

## Standard Command
### Integration Events using interface

### Cross Cutting Concern
| STT     | Name		                     |
|---------|----------------------------------|
| 1       | Authentication and Authorization |
| 2       | Logging and tracing              |
| 3       | Exception Handling               |
| 4       | Validation                       |
| 5       | Caching                          |

### Multi-tenancy

### Unit testing and Behavior driven design

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

## Identity

### Ubiquitous language
Organization:
User:
Role:
Permission:

### Aggregates

### Integration Events
UserEvents : Integration

- Organization integration events
```
OrganizaitionCreated : UserEvents {
	Name
	OrganizationId		// important - unique
	OrganizationNumber  // important - unique
	PhoneNumber
	Email
	Address
	City
	Postcode
}
```


- User integration events
```
UserCreated : UserEvents { }

UserUpdated : UserEvents { }
```


## Product

### Ubiquitous language
Agency - Consume from Organization
SPU
SKU

### Aggregates

### Integration Events