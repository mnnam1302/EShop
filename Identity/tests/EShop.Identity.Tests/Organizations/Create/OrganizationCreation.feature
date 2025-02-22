Feature: Organization Creation
	As a Support or Admin user of the system
	I want to create an organization for specific tenant requirement

Background:
	Given Admin user with all permissions
	And all standard features were turned on for test tenant

Scenario: Creating a new organzaition
	When Admin user creates a new organization with the following
		| Field              | Value                                    |
		| Name               | test-organization                        |
		| OrganizationNumber | 22000                                    |
		| PhoneNumber        | +477311593200                            |
		| Email              | organization@test.com                    |
		| Address            | Oslo                                     |
		| City               | Oslo                                     |
		| Postcode           | 0105                                     |
		| Description        | Marine services provider based in Norway |
	Then there are following organization
		| Field              | Value                                    |
		| Name               | test-organization                        |
		| OrganizationNumber | 22000                                    |
		| PhoneNumber        | +477311593200                            |
		| Email              | organization@test.com                    |
		| Address            | Oslo                                     |
		| City               | Oslo                                     |
		| Postcode           | 0105                                     |
		| Description        | Marine services provider based in Norway |

