Feature: CreateSystemFeature
	As an Admin User
	I want to create a new system feature
	So that I can manage system functionalities effectively

Background: 
	Given System user with following permissions
		| PermissionId               |
		| Users_ViewSystemSettings   |
		| Users_ManageSystemSettings |
	And all features are available for System User

Scenario Outline: System User creates a new system feature
	Given System User has registered tenants with following details
		| Id          | Name       | OwnerUserName | OwnerEmail           | PhoneNumber |
		| test-tenant | Test Owner | test-owner    | test-owner@eshop.com | '123456789' |
	When System User creates a new system feature with following details
		| Id         | Name         | Description                  | State   | Module |
		| feature-id | Test Feature | A new feature for testing | <state> | Core   |
	Then the feature 'feature-id' has following details
		| Id         | Name         | Description               | State   | Module |
		| feature-id | Test Feature | A new feature for testing | <state> | Core   |
	And the tenant 'test-tenant' has following features
		| FeatureId  | State   |
		| feature-id | <state> |

	Examples: 
		| case             | state    |
		| feature enabled  | Enabled  |
		| feature disabled | Disabled |
