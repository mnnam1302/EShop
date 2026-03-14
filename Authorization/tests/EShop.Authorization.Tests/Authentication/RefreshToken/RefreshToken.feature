Feature: Refresh Token
    As an authenticated user
    I want to refresh my access token
    So that I can continue accessing protected resources without re-logging in

Background:
    Given Tenancy service has provisioned a new tenant with following details
        | TenantId    | TenantName  | OwnerUsername          | OwnerDisplayName | OwnerEmail             |
        | test-tenant | Test Tenant | test-owner@test-tenant | Test Owner       | test-owner@test-tenant |
    And all standard features were turned on for 'test-tenant'
    And user 'test-owner@test-tenant' has password 'Password123!'

Scenario: Successfully refresh token with valid refresh token
    Given user 'test-owner@test-tenant' has logged in successfully
    When user 'test-owner@test-tenant' refreshes the token with their current refresh token
    Then the token refresh should succeed
    And the response should contain a new access token
    And the response should contain a new refresh token

Scenario: Failed refresh token with invalid refresh token
    Given user 'test-owner@test-tenant' has logged in successfully
    When user 'test-owner@test-tenant' refreshes the token with an invalid refresh token
    Then the token refresh should fail

Scenario: Refresh token invalidated after logout
    Given user 'test-owner@test-tenant' has logged in successfully
    And user 'test-owner@test-tenant' has logged out
    When user 'test-owner@test-tenant' refreshes the token with their previous refresh token
    Then the token refresh should fail
