V## Why

The current Docker Compose setup has critical bugs preventing successful deployment, and lacks production-ready best practices. The ApiGateway Dockerfile has a syntax error, PostgreSQL secret handling is broken, and the ApiGateway service definition is incomplete. Additionally, there's no separation between development and production configurations, no resource limits, and inconsistent healthchecks across services.

## What Changes

### Critical Fixes
- Fix ApiGateway Dockerfile syntax error (line 54: `FROM base AS final` concatenated with comment)
- Fix PostgreSQL secret handling (`POSTGRES_PASSWORD` → `POSTGRES_PASSWORD_FILE`)
- Complete the ApiGateway service definition in docker-compose.yml
- Add wget to Alpine-based Dockerfiles for healthcheck support

### Security Enhancements
- Use Docker secrets for all sensitive data (database passwords, JWT secret, RabbitMQ credentials)
- Create secret files for each service's database password
- Remove hardcoded credentials from environment variables

### Reliability Improvements
- Add consistent healthchecks to all services
- Standardize restart policy to `unless-stopped`
- Configure proper `depends_on` with health conditions
- Add logging driver configuration with rotation

### Resource Management
- Add memory and CPU limits for all services
- Define resource reservations for guaranteed resources

### Development Experience
- Create `docker-compose.override.yml` for local development configuration
- Separate port mappings and debug settings from base configuration

## Capabilities

### New Capabilities
- `docker-secrets`: Docker secrets management for all sensitive configuration (passwords, keys)
- `service-healthchecks`: Consistent healthcheck configuration across all services
- `resource-limits`: Memory and CPU constraints for container resource management
- `logging-config`: Standardized logging driver configuration with rotation
- `dev-override`: Development-specific Docker Compose override file

### Modified Capabilities
None - this is infrastructure configuration, no existing specs to modify.

## Impact

### Files Modified
- `docker-compose.yml` - Base service definitions with best practices
- `ReverseProxy/src/EShop.ApiGateway/Dockerfile` - Fix syntax error, add wget
- `Tenancy/src/EShop.Tenancy.API/Dockerfile` - Add wget for healthcheck
- `Authorization/src/EShop.Authorization.API/Dockerfile` - Add wget for healthcheck

### Files Created
- `docker-compose.override.yml` - Development-specific overrides
- `Deployment/secrets/tenancy_db_password.txt` - Tenancy DB password
- `Deployment/secrets/authorization_db_password.txt` - Authorization DB password
- `Deployment/secrets/rabbitmq_password.txt` - RabbitMQ password
- `Deployment/secrets/jwt_secret.txt` - JWT signing secret

### Services Affected
- **ApiGateway**: New complete service definition with routing to Tenancy/Authorization
- **Tenancy**: Updated healthcheck, secrets, resource limits
- **Authorization**: Updated healthcheck, secrets, resource limits
- **PostgreSQL**: Fixed secret handling
- **Redis**: Added resource limits
- **RabbitMQ**: Added secrets, resource limits
- **Dashboard**: Added resource limits
