Feature: OrganizationCreation
    As an Admin user of the system
    I want to group users
    So that I can reflect real world organization structure these employees belong to and corresponding access level

Background:
    Given following tenants added to the system
        | TenantId | TenantName | OwnerUsername         | OwnerDisplayName | OwnerEmail        |
        | tenant-1 | Tenant1    | tenantOwner1@tenant-1 | Tenant Owner1    | owner@tenant1.com |
        | tenant-2 | Tenant2    | tenantOwner2@tenant-2 | Tenant Owner2    | owner@tenant2.com |
    And all standard features were turned on for 'tenant-1'
    And the following users are set up
        | Username              | TenantId |
        | tenantOwner1@tenant-1 | tenant-1 |
    And user 'tenantOwner1@tenant-1' has the following permissions
        | PermissionId                 |
        | Identity_ViewOrganizations   |
        | Identity_ManageOrganizations |
    And user 'tenantOwner1@tenant-1' logs in to the system

Scenario: Successfully create a new organization under the root organization
    When User 'tenantOwner1' creates a new organization under the root organization with the following details
        | Id        | Name                       | OrganizationNumber | Email                     | Description        | ParentOrganizationId |
        | child-org | Organization child of root | 50000              | child-org@eshop.ecommerce | Child organization | tenant-1             |
    Then organization 'child-org' has the following details
        | Id        | Name                       | OrganizationNumber | Email                     | Description        | ParentOrganizationId |
        | child-org | Organization child of root | 50000              | child-org@eshop.ecommerce | Child organization | tenant-1             |