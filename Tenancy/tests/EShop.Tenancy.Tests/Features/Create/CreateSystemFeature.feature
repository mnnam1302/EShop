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

Scenario: System User creates a new system feature with state Enabled
	Given System User has registered tenants with following details
		| Id          | Name       | OwnerUserName | OwnerEmail           | PhoneNumber |
		| test-tenant | Test Owner | test-owner    | test-owner@eshop.com | '123456789' |
	When System User creates a new system feature with following details
		| Id           | Name       | Description               | State   | Module |
		| test-feature | NewFeature | A new feature for testing | Enabled | Core   |
	Then the system feature 'NewFeature' has following details
		| Id           | Name       | Description               | State   | Module |
		| test-feature | NewFeature | A new feature for testing | Enabled | Core   |
	#And the tenant 'test-tenant' has following features
	#	| Feature    | State   |
	#	| NewFeature | Enabled |
