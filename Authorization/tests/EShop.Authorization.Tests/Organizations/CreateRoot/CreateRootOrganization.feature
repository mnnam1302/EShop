Feature: CreateRootOrganization
    As an Admin User
    I want to create a root organization when tenant provisioned
    So that I can reflect real world organization structure these employees belong to and corresponding access level

Scenario: System creates a root organization
    When Tenancy service provision a new tenant with following details
        | TenantId    | TenantName  | OwnerUsername          | OwnerDisplayName | OwnerEmail             |
        | test-tenant | Test Tenant | test-owner@test-tenant | Test Owner       | test-owner@test-tenant |
    Then all standard features were turned on for 'test-tenant'
    And the following users are set up
        | Username               | TenantId    |
        | test-owner@test-tenant | test-tenant |
    And user 'test-owner@test-tenant' logs in to the system
    And user 'test-owner@test-tenant' has following permissions
        | PermissionId                      |
        | Authorization_ViewOrganizations   |
        | Authorization_ManageOrganizations |
    And user 'test-owner' retrieves organizations
        | Id          | Name        | Description       | TenantId    |
        | test-tenant | Test Tenant | Root Organization | test-tenant |
