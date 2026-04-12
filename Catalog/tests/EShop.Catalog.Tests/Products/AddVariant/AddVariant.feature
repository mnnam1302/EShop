Feature: AddVariant
    As a catalog manager
    I want to add variants to a product
    So that customers can choose from different attribute combinations

Background:
    Given System user with following permissions
        | PermissionId             |
        | Catalog_ManageProducts   |
        | Catalog_ManageCategories |
    And all features are available for System User
    And System User has created the following category
        | Name        | Reference | Slug        |
        | Electronics | ELEC001   | electronics |

Scenario: Add a variant with dimension values
    Given System User has created the following product under category 'ELEC001'
        | Name       | Description       | Price | DiscountPrice | Slug       | Tags     | Images                          |
        | Laptop Pro | A powerful laptop |   500 |        459.99 | laptop-pro | portable | https://img.example.com/lp1.jpg |
    And System user has added a variation dimension to the product 'Laptop Pro'
        | Name  | DisplayName | Values   | DisplayStyle |
        | Color | Color       | Red,Blue | Text         |
    When System user adds a variant to the product 'Laptop Pro'
        | Name       | Sku     | Price | DiscountPrice | Color |
        | Red Laptop | SKU-RED |   550 |           500 | Red   |
    Then the product 'Laptop Pro' has the following variants
        | Name       | Sku     | Price | DiscountPrice | IsDefault |
        |            |         |   500 |        459.99 | true      |
        | Red Laptop | SKU-RED |   550 |           500 | false     |
