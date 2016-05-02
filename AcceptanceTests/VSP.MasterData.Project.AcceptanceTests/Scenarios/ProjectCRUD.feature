Feature: ProjectCRUD

Scenario: Create project
	Given I inject 'CreateCustomerEvent' into Kafka
		And I inject 'AssociateCustomerUserEvent' into Kafka
	When I 'Create' a project via Web API as the user for the customer
		And I associate the project with the customer via Web API
		And I try to get all projects for the customer via Web API
	Then the created project is in the list returned by the Web API