Feature: User Logout
    As an authenticated user
    I want to log out of the system
    So that my session is terminated and tokens are invalidated

Background:
    Given Tenancy service has provisioned a new tenant with following details
        | TenantId    | TenantName  | OwnerUsername          | OwnerDisplayName | OwnerEmail             |
        | test-tenant | Test Tenant | test-owner@test-tenant | Test Owner       | test-owner@test-tenant |
    And all standard features were turned on for 'test-tenant'
    And user 'test-owner@test-tenant' has password 'Password123!'

Scenario: Successfully logout
    Given user 'test-owner@test-tenant' has logged in successfully
    When user 'test-owner@test-tenant' logs out
    Then the logout should succeed

Scenario: Tokens are invalidated after logout
    Given user 'test-owner@test-tenant' has logged in successfully
    When user 'test-owner@test-tenant' logs out
    Then user 'test-owner@test-tenant' cannot use the previous refresh token
