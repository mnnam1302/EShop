# EShop Kodi

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
AgencyContactDetailUpdated : UserEvents {
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


## Sale

### Ubiquitous language
Agency - Consume from Organization
SPU
SKU

### Aggregates

### Integration Events