Feature: OrganizationCreation
    As an Admin user of the system
    I want to group users
    So that I can reflect real world organization structure these employees belong to and corresponding access level

Background:
	Given following tenants added to the system
		| TenantId | TenantName | OwnerUsername | OwnerDisplayName | OwnerEmail        |
		| tenant-1 | Tenant1    | tenantOwner1  | Tenant Owner1    | owner@tenant1.com |
		| tenant-2 | Tenant2    | tenantOwner2  | Tenant Owner2    | owner@tenant2.com |
	And Admin user with all permissions

Scenario: Successfully create a new organization under the root organization
	When User 'tenantOwner1' creates a new organization under the root organization with the following details
		| Id        | Name                       | OrganizationNumber | Email                     | Description        | ParentOrganizationId |
		| child-org | Organization child of root | 50000              | child-org@eshop.ecommerce | Child organization | tenant-1             |
	Then there are following organization
		| Id        | Name                       | OrganizationNumber | Email                     | Description        | ParentOrganizationId |
		| child-org | Organization child of root | 50000              | child-org@eshop.ecommerce | Child organization | tenant-1             |