Feature: Organization Creation
	As a Support or Admin user of the system
	I want to create an organization for specific tenant requirement

Background:
	Given Admin user with all permissions
	And all standard features were turned on for test tenant

Scenario: Creating a new organzaition
	When Admin user creates a new organization with the following
	| Name          | OrganizationNumber | PhoneNumber   | Email                   | Address | City | Postcode | Description                              |
	| nordic-marine | 22000              | +477311593200 | organization@test.gmail | Oslo    | Oslo | 0105     | Marine services provider based in Norway |
	Then there are following organization
	| Name          | OrganizationNumber | PhoneNumber   | Email                   | Address | City | Postcode | Description                              |
    | nordic-marine | 22000              | +477311593200 | organization@test.gmail | Oslo    | Oslo | 0105     | Marine services provider based in Norway |

