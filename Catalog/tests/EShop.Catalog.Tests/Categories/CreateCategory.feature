Feature: CreateCategory
	As a catalog manager
	I want to create new categories in the catalog
	So that products can be organized effectively

Scenario: Create a new category successfully
	Given Authorizatin service has published organization created
	When system user creates a new category
		| Name        | Reference | Slug        | ParentId |
		| Electronics | ELEC123   | electronics |          |
	Then the category 'ELEC123' has following details
		| Name        | Reference | Slug        | ParentId |
		| Electronics | ELEC123   | electronics |          |
