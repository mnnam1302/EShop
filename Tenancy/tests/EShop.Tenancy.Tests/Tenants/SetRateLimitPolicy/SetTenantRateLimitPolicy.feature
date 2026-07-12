Feature: Set tenant rate limit policy
	As a System or Support user
	I want to set a tenant's rate-limit policy
	So that tenant request limits reflect their plan and support decisions

Background:
	Given System user with following permissions
		| PermissionId               |
		| Users_ViewSystemSettings   |
		| Users_ManageSystemSettings |
	And all features are available for System User

Scenario: System User sets a valid rate-limit policy for a tenant
	Given System User has registered tenants with following details
		| Id          | Name       | OwnerUserName | OwnerEmail           | PhoneNumber |
		| test-tenant | Test Owner | test-owner    | test-owner@eshop.com | '123456789' |
	When System User sets the rate-limit policy for tenant 'test-tenant' with following rules
		| Domain | Scope | Unit   | RequestsPerUnit | Burst |
		| *      | User  | Minute | 120             | 150   |
	Then the system responds with status 'NoContent'

Scenario: Tenant user cannot set the rate-limit policy
	Given System User has registered tenants with following details
		| Id          | Name       | OwnerUserName | OwnerEmail           | PhoneNumber |
		| test-tenant | Test Owner | test-owner    | test-owner@eshop.com | '123456789' |
	When a tenant user of 'test-tenant' sets the rate-limit policy with following rules
		| Domain | Scope | Unit   | RequestsPerUnit | Burst |
		| *      | User  | Minute | 120             | 150   |
	Then the system responds with status 'Forbidden'

Scenario: System User cannot set an invalid rate-limit policy
	Given System User has registered tenants with following details
		| Id          | Name       | OwnerUserName | OwnerEmail           | PhoneNumber |
		| test-tenant | Test Owner | test-owner    | test-owner@eshop.com | '123456789' |
	When System User sets the rate-limit policy for tenant 'test-tenant' with following rules
		| Domain | Scope | Unit   | RequestsPerUnit | Burst |
		| *      | User  | Minute | 0               |       |
	Then the system raise an error with message 'non-positive requestsPerUnit'
