Feature: CreateRole
	In order to define the functional boundary for users across the system
	As an authorized user
	I want to be able to create roles

#Background: 
#	Given following tenants added to the system
#		| TenantName | Username     | DisplayName   | Email              |
#		| Tenant1    | tenantOwner1 | Tenant Owner1 | owner1@tenant1.com |
#		| Tenant2    | tenantOwner2 | Tenant Ownwe2 | ownwe2@tenant2.com |

Scenario: Create new role for tenant
	Given There is a new role with the following data
		| Name			| Description		| PhoneNumber	|
		| Role test		| description test	| +8469957900	|
	When Create new role
	Then A new role created with following data
		| Name			| Description		| PhoneNumber	|
		| Role test		| description test	| +8469957900	|
