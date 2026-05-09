Feature: PublishProduct
    As a catalog manager
    I want to publish a product
    So that it becomes available to customers

Background:
    Given System user with following permissions
        | PermissionId             |
        | Catalog_ManageProducts   |
        | Catalog_ManageCategories |
    And all features are available for System User
    And System User has created the following category
        | Name        | Reference | Slug        |
        | Electronics | ELEC001   | electronics |

Scenario: Publish a draft product
    Given System User has created the following product under category 'ELEC001'
        | Name       | Description       | Slug       | Tags     | Images                          |
        | Laptop Pro | A powerful laptop | laptop-pro | portable | https://img.example.com/lp1.jpg |
    And System user has added a variation dimension to the product 'Laptop Pro'
        | Name  | DisplayName | Values   | DisplayStyle |
        | Color | Color       | Red,Blue | Text         |
    And System user has added a variant to the product 'Laptop Pro'
        | Name       | Sku     | Price | DiscountPrice | Color |
        | Red Laptop | SKU-RED |   500 |             0 | Red   |
    When System user publishes the product 'Laptop Pro'
    Then the product 'Laptop Pro' has following details
        | Name       | State     |
        | Laptop Pro | Published |
