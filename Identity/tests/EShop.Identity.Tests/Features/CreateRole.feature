Feature: CreateRole
	In order to define the functional boundary for users across the system
	As an authorized user
	I want to be able to create roles

Background:
	Given following tenant users added to the system
		| Username     | DisplayName   | Email              | TenantName |
		| tenantOwner1 | Tenant Owner1 | owner1@tenant1.com | Tenant1    |

#Scenario: Create new role for tenant
#	When user 'tenantOwner1' create role 'NewRole'
#	Then there are following Roles in the system
#		| Name    |
#		| Owner   |
#		| NewRole |
#		#| Name    | TenantId |
#		#| Owner   | Tenant1  |
#		#| NewRole | Tenant1  |
