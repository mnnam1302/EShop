Feature: CreateRootOrganization
    As an Admin User
    I want to create a root organization when tenant provisioned
    So that I can reflect real world organization structure these employees belong to and corresponding access level

Background:
    Given System user with following permissions
        | PermissionId               |
        | Users_ViewSystemSettings   |
        | Users_ManageSystemSettings |
    And all features are available for System User

Scenario: System creates a root organization
    When Tenancy service has provisioned a new tenant with following details
        | TenantId    | TenantName  | OwnerUsername | OwnerDisplayName | OwnerEmail             |
        | test-tenant | Test Tenant | test-owner    | Test Owner       | test-owner@test-tenant |
    Then there are following organizations created
        | Id       | Name              | OrganizationNumber | Email                | Description       | ParentOrganizationId |
        | root-org | Root Organization | '10000'            | root-org@test-tenant | Root organization | <no value>           |
