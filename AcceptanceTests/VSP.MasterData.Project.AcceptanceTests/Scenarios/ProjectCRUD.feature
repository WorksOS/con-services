﻿Feature: ProjectCRUD

Scenario: Create project
	Given I inject 'CreateCustomerEvent' into Kafka
		And I inject 'AssociateCustomerUserEvent' into Kafka
	When I 'Create' a project via Web API as the user for the customer
		And I 'Associate' the project with the customer via Web API
		And I try to get all projects for the customer via Web API
	Then the 'Created' project is in the list returned by the Web API

Scenario: Update project
	Given I inject 'CreateCustomerEvent' into Kafka
		And I inject 'AssociateCustomerUserEvent' into Kafka
	When I 'Create' a project via Web API as the user for the customer
		And I 'Associate' the project with the customer via Web API
		And I try to get all projects for the customer via Web API
		And the 'Created' project is in the list returned by the Web API
	And I 'Update' a project via Web API as the user for the customer
		And I try to get all projects for the customer via Web API
	Then the 'Updated' project is in the list returned by the Web API

Scenario: Delete project
	Given I inject 'CreateCustomerEvent' into Kafka
		And I inject 'AssociateCustomerUserEvent' into Kafka
	When I 'Create' a project via Web API as the user for the customer
		And I 'Associate' the project with the customer via Web API
		And I try to get all projects for the customer via Web API
		And the 'Created' project is in the list returned by the Web API
	And I 'Delete' a project via Web API as the user for the customer
		And I try to get all projects for the customer via Web API
	Then project is not in the list returned by the Web API

Scenario: Dissociate project
	Given I inject 'CreateCustomerEvent' into Kafka
		And I inject 'AssociateCustomerUserEvent' into Kafka
	When I 'Create' a project via Web API as the user for the customer
		And I 'Associate' the project with the customer via Web API
		And I try to get all projects for the customer via Web API
		And the 'Created' project is in the list returned by the Web API
	And I 'Dissociate' the project with the customer via Web API
		And I try to get all projects for the customer via Web API
	Then project is not in the list returned by the Web API