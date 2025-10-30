Feature: TenancyCreation
	As a System user 
	I want to configure some settings at the tenant level
	So that users with permission can view and edit tenant settings

Background:
	Given System user with following permissions
		| PermissionId               |
		| Users_ViewSystemSettings   |
		| Users_ManageSystemSettings |

Scenario: System user register a new tenant successfully
	Given all features are available for System User
	And user of group 'eshop-support' logged in
	When System user registers a new tenant with following details
		| Attribute     | Value         |
		| Id            | test-tenant   |
		| Name          | Test Owner    |
		| OwnerUsername | test-owner    |
		| OwnerEmail    | mail@test.com |
		| PhoneNumber   | '123456789'   |
	Then the tenant 'test-tenant' has following details
		| TenantId    | TenantName | OwnerUsername | OwnerEmail    | PhoneNumber |
		| test-tenant | Test Owner | test-owner    | mail@test.com | '123456789' |

