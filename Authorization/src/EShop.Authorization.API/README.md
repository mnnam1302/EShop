# Authorization Service

## Overview

This service provides RSA-based JWT token generation and validation for multi-tenant scenarios. Each tenant has a unique RSA key pair, supporting secure, isolated authentication.

## Add new migration
```bash
dotnet ef migrations add <MigrationName> -p ../EShop.Authorization.Infrastructure --startup-project .
```

## Feature Login

### Endpoint
`POST /api/auth/login`

### Request Body
```json
{
  "username": "string",
  "password": "string",
}
```

### CQRS: LoginQuery - LoginQueryHandler