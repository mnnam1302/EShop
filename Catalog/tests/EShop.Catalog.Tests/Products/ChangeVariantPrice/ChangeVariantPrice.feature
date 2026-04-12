Feature: ChangeVariantPrice
    As a catalog manager
    I want to change the price of a product variant
    So that pricing can be updated without modifying other properties

Background:
    Given System user with following permissions
        | PermissionId             |
        | Catalog_ManageProducts   |
        | Catalog_ManageCategories |
    And all features are available for System User
    And System User has created the following category
        | Name        | Reference | Slug        |
        | Electronics | ELEC001   | electronics |

Scenario: Change the price of the default variant
    Given System User has created the following product under category 'ELEC001'
        | Name       | Description       | Price | DiscountPrice | Slug       | Tags     | Images                          |
        | Laptop Pro | A powerful laptop |   500 |        459.99 | laptop-pro | portable | https://img.example.com/lp1.jpg |
    When System user changes the price of the default variant of product 'Laptop Pro'
        | Price | DiscountPrice |
        |   599 |        549.99 |
    Then the product 'Laptop Pro' has the following variants
        | Price | DiscountPrice | IsDefault |
        |   599 |        549.99 | true      |
