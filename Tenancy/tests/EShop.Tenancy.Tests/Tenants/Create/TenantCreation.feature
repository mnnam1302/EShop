Feature: TenancyCreation
	As a System user 
	I want to configure some settings at the tenant level
	So that users with permission can view and edit tenant settings

Background:
	Given System user with following permissions
		| PermissionId               |
		| Users_ViewSystemSettings   |
		| Users_ManageSystemSettings |
	And all features are available for System User

Scenario: System user register a new tenant successfully
	Given user of group 'eshop-support' logged in
	When System user registers a new tenant with following details
		| Attribute     | Value                |
		| Id            | test-tenant          |
		| Name          | Test Owner           |
		| OwnerUsername | test-owner           |
		| OwnerEmail    | test-owner@eshop.com |
		| PhoneNumber   | '123456789'          |
	Then the tenant 'test-tenant' has following details
		| Id          | Name       | OwnerUsername          | OwnerEmail           | PhoneNumber |
		| test-tenant | Test Owner | test-owner@test-tenant | test-owner@eshop.com | '123456789' |

