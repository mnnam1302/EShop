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

Scenario: Change the price of a variant
    Given System User has created the following product under category 'ELEC001'
        | Name       | Description       | Slug       | Tags     | Images                          |
        | Laptop Pro | A powerful laptop | laptop-pro | portable | https://img.example.com/lp1.jpg |
    And System user has added a variation dimension to the product 'Laptop Pro'
        | Name  | DisplayName | Values   | DisplayStyle |
        | Color | Color       | Red,Blue | Text         |
    And System user has added a variant to the product 'Laptop Pro'
        | Name       | Sku     | Price | DiscountPrice | Color |
        | Red Laptop | SKU-RED |   500 |        459.99 | Red   |
    When System user changes the price of variant 'SKU-RED' of product 'Laptop Pro'
        | Price | DiscountPrice |
        |   599 |        549.99 |
    Then the product 'Laptop Pro' has the following variants
        | Name       | Sku     | Price | DiscountPrice | IsDefault |
        | Red Laptop | SKU-RED | 599   | 549.99        | false     |
