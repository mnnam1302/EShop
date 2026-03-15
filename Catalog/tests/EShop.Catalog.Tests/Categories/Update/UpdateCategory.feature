Feature: UpdateCategory
	As a catalog manager
	I want to update existing categories in the catalog
	So that category information stays current and accurate

Background:
	Given System user with following permissions
		| PermissionId             |
		| Catalog_ManageCategories |
		| Catalog_ViewCategories   |
	And all features are available for System User

Scenario: Update an existing category's details
	When System user creates a new category
		| Name        | Reference | Slug        | ParentId |
		| Electronics | ELEC123   | electronics |          |
	When System user updates category with reference 'ELEC123'
		| Name                 | Reference | Slug                 |
		| Consumer Electronics | ELEC123   | consumer-electronics |
	Then the category 'ELEC123' has following details
		| Name                 | Reference | Slug                 | ParentId |
		| Consumer Electronics | ELEC123   | consumer-electronics |          |
