## ADDED Requirements

### Requirement: All services have logging configuration
Every service in docker-compose.yml SHALL define a `logging` configuration block.

#### Scenario: Logging configuration presence
- **WHEN** a service is defined in docker-compose.yml
- **THEN** it SHALL include a `logging` block with driver and options

### Requirement: Use json-file logging driver
Services SHALL use the `json-file` logging driver for container logs.

#### Scenario: Logging driver type
- **WHEN** a service logging is configured
- **THEN** the driver SHALL be `json-file`

### Requirement: Log file size rotation
Log files SHALL be limited to 10MB maximum size with rotation.

#### Scenario: Log max size
- **WHEN** a container produces logs
- **THEN** individual log files SHALL not exceed 10MB (`max-size: "10m"`)

### Requirement: Log file count rotation
Services SHALL retain a maximum of 3 log files per container.

#### Scenario: Log file retention
- **WHEN** log rotation occurs
- **THEN** only 3 log files SHALL be retained (`max-file: "3"`)

### Requirement: Total log storage per service
Each service SHALL use maximum 30MB total log storage (10MB × 3 files).

#### Scenario: Total log storage
- **WHEN** all log files for a service are at maximum size
- **THEN** total log storage SHALL not exceed 30MB
