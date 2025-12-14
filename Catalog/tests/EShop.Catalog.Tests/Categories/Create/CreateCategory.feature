Feature: CreateCategory
	As a catalog manager
	I want to create new categories in the catalog
	So that products can be organized effectively

Background:
	Given System user with following permissions
		| PermissionId             |
		| Catalog_ManageCategories |
		| Catalog_ViewCategories   |
	And all features are available for System User

Scenario: Create a new category
	When System user creates a new category
		| Name        | Reference | Slug        | ParentId |
		| Electronics | ELEC123   | electronics |          |
	Then the category 'ELEC123' has following details
		| Name        | Reference | Slug        | ParentId |
		| Electronics | ELEC123   | electronics |          |
