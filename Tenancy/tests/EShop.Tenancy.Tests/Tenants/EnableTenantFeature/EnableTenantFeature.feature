Feature: EnableTenantFeatures
	As System User
	I want to enable specific features for a tenant
	So that the tenant can access those features

Background: 
	Given System user with following permissions
		| PermissionId               |
		| Users_ViewSystemSettings   |
		| Users_ManageSystemSettings |
	And all features are available for System User

Scenario: System User enables a feature for a tenant
	Given System User has registered tenants with following details
		| Id          | Name       | OwnerUserName | OwnerEmail           | PhoneNumber |
		| test-tenant | Test Owner | test-owner    | test-owner@eshop.com | '123456789' |
	And System User has created a new system feature with following details
		| Id         | Name         | Description               | State    | Module |
		| feature-id | Test Feature | A new feature for testing | Disabled | Core   |
	When System User enables the feature 'feature-id' for tenant 'test-tenant'
	Then the tenant 'test-tenant' has following features
		| FeatureId  | State   |
		| feature-id | Enabled |

Scenario: System User cannot enable a non-existing feature
	Given System User has registered tenants with following details
		| Id          | Name       | OwnerUserName | OwnerEmail           | PhoneNumber |
		| test-tenant | Test Owner | test-owner    | test-owner@eshop.com | '123456789' |
	And System User has created a new system feature with following details
		| Id         | Name         | Description               | State   | Module |
		| feature-id | Test Feature | A new feature for testing | Enabled | Core   |
	When System User enables the feature 'test-feature-id' for tenant 'test-tenant'
	Then the system raise an error with message 'Feature is not found for the tenant.'

Scenario: System User cannot enable an enabled feature
	Given System User has registered tenants with following details
		| Id          | Name       | OwnerUserName | OwnerEmail           | PhoneNumber |
		| test-tenant | Test Owner | test-owner    | test-owner@eshop.com | '123456789' |
	And System User has created a new system feature with following details
		| Id         | Name         | Description               | State    | Module |
		| feature-id | Test Feature | A new feature for testing | Enabled | Core   |
	When System User enables the feature 'feature-id' for tenant 'test-tenant'
	Then the system raise an error with message 'Feature is already enabled for the tenant.'
