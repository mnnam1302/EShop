Feature: AddChildOrganization
    As an Admin User
    I want to group users
    So that I can reflect real world organization structure these employees belong to and corresponding access level

Scenario: Adding a child organization under the root organization
    Given Tenancy service has provisioned a new tenant with following details
        | TenantId | TenantName | OwnerUsername         | OwnerDisplayName | OwnerEmail        |
        | tenant-1 | Tenant 1   | ownerTenant1@tenant-1 | Owner Tenant1    | owner@tenant1.com |
    And all standard features were turned on for 'tenant-1'
    And the following users are set up
        | Username              | TenantId |
        | ownerTenant1@tenant-1 | tenant-1 |
    And user 'ownerTenant1@tenant-1' has the following permissions
        | PermissionId                      |
        | Authorization_ManageOrganizations |
    And user 'ownerTenant1@tenant-1' logs in to the system
    When user 'ownerTenant1@tenant-1' adds a child organization under the organization 'tenant-1' with the following details
        | Id        | Name                       | OrganizationNumber | Email                   | Description        |
        | child-org | Organization child of root |             500000 | child-org@app.ecommerce | Child organization |
    Then user 'ownerTenant1@tenant-1' retrieves organizations
        | Id        | Name                       | Description        | Email                   | OrganizationNumber | ParentOrganizationId |
        | tenant-1  | Tenant 1                   | Root Organization  |                         |                    |                      |
        | child-org | Organization child of root | Child organization | child-org@app.ecommerce |             500000 | tenant-1             |
