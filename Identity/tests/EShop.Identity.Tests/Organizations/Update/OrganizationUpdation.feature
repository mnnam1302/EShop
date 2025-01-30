Feature: Organization Creation
	As a tenant user
	I want to update an organization

Background:
	Given Admin user with all permissions
	And all standard features were turned on for test tenant

Scenario: Update an existing organization
	Given Admin user creates a new organization with the following
	| Field              | Value                                    |
	| Name               | test-organization                        |
	| OrganizationNumber | 22000                                    |
	| PhoneNumber        | +477311593200                            |
	| Email              | organization@test.com                    |
	| Address            | Oslo                                     |
	| City               | Oslo                                     |
	| Postcode           | 0105                                     |
	| Description        | Marine services provider based in Norway |
	When Admin user updates the organization 'test-organization' with the following
	| Field              | Value                                    |
	| Name               | test-organization                        |
	| OrganizationNumber | 20000                                    |
	| PhoneNumber        | +477311593222                            |
	| Email              | organization@test.com                    |
	| Address            | Oslo                                     |
	| City               | Oslo                                     |
	| Postcode           | 0105                                     |
	| Description        | Marine services provider based in Norway |
	Then organization 'test-organization' has the following details
	| Field              | Value                                    |
	| Name               | test-organization                        |
	| OrganizationNumber | 20000                                    |
	| PhoneNumber        | +477311593222                            |
	| Email              | organization@test.com                    |
	| Address            | Oslo                                     |
	| City               | Oslo                                     |
	| Postcode           | 0105                                     |
	| Description        | Marine services provider based in Norway |

