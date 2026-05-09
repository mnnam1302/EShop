Feature: CreateProduct
	As a catalog manager
	I want to create new products in the catalog
	So that customers can browse and purchase items

Background:
	Given System user with following permissions
		| PermissionId             |
		| Catalog_ManageProducts   |
		| Catalog_ManageCategories |
	And all features are available for System User
	And System User has created the following category
		| Name        | Reference | Slug        |
		| Electronics | ELEC001   | electronics |

Scenario: Create a new product
	When System user creates a new product under category 'ELEC001'
		| Name       | Description       | Slug       | Tags            | Images                          |
		| Laptop Pro | A powerful laptop | laptop-pro | laptop,portable | https://img.example.com/lp1.jpg |
	Then the product 'Laptop Pro' has following details
		| Name       | Description       | Slug       | State | TenantId    |
		| Laptop Pro | A powerful laptop | laptop-pro | Draft | TEST-TENANT |
