Feature: User Login
    As a user
    I want to log in to the system
    So that I can access protected resources

Background:
    Given Tenancy service has provisioned a new tenant with following details
        | TenantId    | TenantName  | OwnerUsername          | OwnerDisplayName | OwnerEmail             |
        | test-tenant | Test Tenant | test-owner@test-tenant | Test Owner       | test-owner@test-tenant |
    And all standard features were turned on for 'test-tenant'

Scenario: Successful login with valid credentials
    Given user 'test-owner@test-tenant' has password 'Password123!'
    When user 'test-owner@test-tenant' attempts to log in with password 'Password123!'
    Then the login should succeed
    And the response should contain an access token
    And the response should contain a refresh token

Scenario: Failed login with invalid password
    Given user 'test-owner@test-tenant' has password 'Password123!'
    When user 'test-owner@test-tenant' attempts to log in with password 'WrongPassword!'
    Then the login should fail with error 'The provided credentials are invalid.'

Scenario: Failed login with non-existent user
    When user 'nonexistent@test-tenant' attempts to log in with password 'Password123!'
    Then the login should fail with error 'The provided credentials are invalid.'

Scenario: Account lockout after multiple failed attempts
    Given user 'test-owner@test-tenant' has password 'Password123!'
    When user 'test-owner@test-tenant' attempts to log in with password 'WrongPassword!' 5 times
    Then the login should fail with error 'The user account is locked out due to multiple failed login attempts.'

Scenario: Login blocked for pending verification user
    Given user 'test-owner@test-tenant' is in 'PendingVerification' state
    When user 'test-owner@test-tenant' attempts to log in with password 'Password123!'
    Then the login should fail with error 'The user account is pending verification.'
