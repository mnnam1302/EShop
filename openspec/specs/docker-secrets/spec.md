## ADDED Requirements

### Requirement: Secret files stored in Deployment directory
The system SHALL store all Docker secret files in `Deployment/secrets/` directory with `.txt` extension.

#### Scenario: Secret file location
- **WHEN** a new secret is needed for a service
- **THEN** the secret file SHALL be created at `Deployment/secrets/<secret-name>.txt`

### Requirement: Secret files excluded from version control
The system SHALL exclude secret files from git by adding `Deployment/secrets/*.txt` to `.gitignore`.

#### Scenario: Git ignore pattern
- **WHEN** `Deployment/secrets/` contains `.txt` files
- **THEN** git status SHALL NOT show these files as untracked

### Requirement: Docker secrets defined in compose
The system SHALL define all secrets in the `secrets:` section of docker-compose.yml with file references.

#### Scenario: Secret definition format
- **WHEN** a secret is defined in docker-compose.yml
- **THEN** it SHALL use the format `file: ./Deployment/secrets/<name>.txt`

### Requirement: PostgreSQL uses secret file environment variable
PostgreSQL SHALL read the password from a file using `POSTGRES_PASSWORD_FILE` environment variable.

#### Scenario: PostgreSQL password from secret
- **WHEN** PostgreSQL container starts
- **THEN** it SHALL read password from `/run/secrets/postgres_password`

### Requirement: Service secrets mounted read-only
Services requiring secrets SHALL declare them in their `secrets:` array for read-only mounting at `/run/secrets/`.

#### Scenario: Secret mounting
- **WHEN** a service declares a secret dependency
- **THEN** the secret SHALL be available at `/run/secrets/<secret-name>` inside the container

### Requirement: Required secret files
The system SHALL have secret files for: postgres_password, tenancy_db_password, authorization_db_password, rabbitmq_password, jwt_secret.

#### Scenario: Complete secret set
- **WHEN** docker-compose up is executed
- **THEN** all five secret files MUST exist or the startup SHALL fail
