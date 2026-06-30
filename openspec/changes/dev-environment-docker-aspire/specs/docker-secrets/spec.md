## ADDED Requirements

### Requirement: Committed secret templates for self-service onboarding
The system SHALL provide a committed `*.template` file for every required secret, so a fresh clone can create its local secret files by copying the templates.

#### Scenario: Template present per secret
- **WHEN** a secret file is required for local development
- **THEN** a committed `<secret-name>.txt.template` (or equivalent) SHALL exist alongside the gitignored secret location

#### Scenario: Onboarding by copying templates
- **WHEN** a developer follows the Getting Started instructions from a fresh clone
- **THEN** the only secret-setup step SHALL be copying the templates to their gitignored secret files

### Requirement: Non-credential secrets follow the secret-file convention
The system SHALL manage non-credential dev secrets (such as the OpenTelemetry Dashboard API key) using the same gitignored secret-file / environment convention as credential secrets, rather than hardcoding them in source.

#### Scenario: OTel dashboard key sourced from secret
- **WHEN** the OpenTelemetry Dashboard API key is needed by the Aspire AppHost or a compose service
- **THEN** it SHALL be read from a gitignored secret file or environment variable, not a committed literal

#### Scenario: Template provided for non-credential secret
- **WHEN** a non-credential secret is introduced
- **THEN** a committed template with a placeholder value SHALL be provided so a fresh clone can populate it
