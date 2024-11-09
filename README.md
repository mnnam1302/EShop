# EShop Kodi

## Infrastructure local
docker-compose -f docker-compose.Dev.Infrastructure.yml up -d

## Standard
Integration Events using interface

## Identity

### Ubiquitous language
Organization:
User:
Role:
Permission:

### Aggregates

### Integration Events
UserEvents : Integration

- Organization
AgencyContactDetailUpdated : UserEvent {
	Name
	OrganizationId		// important - unique
	OrganizationNumber  // important - unique
	PhoneNumber
	Email
	Address
	City
	Postcode
}

- User
UserCreated
UserUpdated


## Sale

### Ubiquitous language
Agency - Consume from Organization
SPU
SKU

### Aggregates

### Integration Events