Feature: CreateRole
	As user with role Owner of the tenantA 
	I'd like to create new role in my tenant

Scenario: Create new role for tenant
	Given There is a new role with the following data
		| Name			| Description		| PhoneNumber	|
		| Role test		| description test	| +8469957900	|
	When Create new role
	Then A new role created with following data
		| Name			| Description		| PhoneNumber	|
		| Role test		| description test	| +8469957900	|
