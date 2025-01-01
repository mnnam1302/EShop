Feature: CreateRole
	In order to define the functional boundary for users across the system
	As an authorized user
	I want to be able to create roles

Background:
	Given following tenant users added to the system
		| TenantName | Username     | DisplayName   | Email              |
		| Tenant1    | tenantOwner1 | Tenant Owner1 | owner1@tenant1.com |
		#| Tenant2    | tenantOwner2 | Tenant Ownwe2 | ownwe2@tenant2.com |

Scenario: Create new role for tenant
	When user 'tenantOwner1' create role 'NewRole'
	Then there are following Roles in the system
		| Name    | TenantId |
		| Owner   | Tenant1  |
		| NewRole | Tenant1  |
