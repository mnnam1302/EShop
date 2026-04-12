Feature: AddVariationDimension
    As a catalog manager
    I want to add variation dimensions to a product
    So that variants can be differentiated by attributes such as color or size

Background:
    Given System user with following permissions
        | PermissionId             |
        | Catalog_ManageProducts   |
        | Catalog_ManageCategories |
    And all features are available for System User
    And System User has created the following category
        | Name        | Reference | Slug        |
        | Electronics | ELEC001   | electronics |

Scenario: Add a variation dimension to a draft product
    Given System User has created the following product under category 'ELEC001'
        | Name       | Description       | Price | DiscountPrice | Slug       | Tags     | Images                          |
        | Laptop Pro | A powerful laptop |   500 |        459.99 | laptop-pro | portable | https://img.example.com/lp1.jpg |
    When System user adds a variation dimension to the product 'Laptop Pro'
        | Name  | DisplayName | Values   | DisplayStyle |
        | Color | Color       | Red,Blue | Text         |
    Then the product 'Laptop Pro' has the following variation dimensions
        | Name  | DisplayName | DisplayStyle |
        | Color | Color       | Text         |
