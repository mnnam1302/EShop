Feature: UserInvitation
    As a User management
    I want to invite new users to the organization
    So that I can access the application with appropriate permissions

Scenario: Inviting a new user
    Given Tenancy service has provisied a new tenant with following details
        | TenantId    | TenantName  | OwnerUsername          | OwnerDisplayName | OwnerEmail             |
        | test-tenant | Test Tenant | test-owner@test-tenant | Test Owner       | test-owner@test-tenant |
    And all standard features were turned on for 'test-tenant'
    And the following users are set up
        | Username               | TenantId    |
        | test-owner@test-tenant | test-tenant |
    And user 'test-owner@test-tenant' logs in to the system
    And user 'test-owner@test-tenant' has the following permissions
        | PermissionId              |
        | Authorization_ViewRoles   |
        | Authorization_ManageUsers |
    When user invites a new user with role 'Role Owner' the following details
        | Username              | Email                 | DisplayName | PhoneNumber | OrganizationId |
        | test-user@test-tenant | test-user@test-tenant | Test User   |  0969900212 | test-tenant    |
    Then user 'test-user@test-tenant' has following details
        | Username              | Email                 | DisplayName | PhoneNumber | OrganizationId | TenantId    | CreatedByUserId        |
        | test-user@test-tenant | test-user@test-tenant | Test User   |  0969900212 | test-tenant    | test-tenant | test-owner@test-tenant |
