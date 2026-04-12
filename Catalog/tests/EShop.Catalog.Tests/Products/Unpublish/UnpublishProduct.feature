Feature: UnpublishProduct
    As a catalog manager
    I want to unpublish a product
    So that it is no longer visible to customers

Background:
    Given System user with following permissions
        | PermissionId             |
        | Catalog_ManageProducts   |
        | Catalog_ManageCategories |
    And all features are available for System User
    And System User has created the following category
        | Name        | Reference | Slug        |
        | Electronics | ELEC001   | electronics |

Scenario: Unpublish a published product
    Given System User has created the following product under category 'ELEC001'
        | Name       | Description       | Price | DiscountPrice | Slug       | Tags     | Images                          |
        | Laptop Pro | A powerful laptop |   500 |        459.99 | laptop-pro | portable | https://img.example.com/lp1.jpg |
    And the product 'Laptop Pro' is published
    When System user unpublishes the product 'Laptop Pro'
    Then the product 'Laptop Pro' has following details
        | Name       | State       |
        | Laptop Pro | Unpublished |
