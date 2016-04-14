Feature: MasterData

Scenario: Create customer
	Given I inject 'CreateCustomerEvent' into Kafka
	Then a new 'Customer' is created

Scenario: Update customer
	Given I inject 'CreateCustomerEvent' into Kafka
		And a new 'Customer' is created
	When I inject 'UpdateCustomerEvent' into Kafka
	Then the new 'Customer' is updated

Scenario: Delete customer
	Given I inject 'CreateCustomerEvent' into Kafka
		And a new 'Customer' is created
	When I inject 'DeleteCustomerEvent' into Kafka
	Then the new 'Customer' is deleted

Scenario: Associate user customer
	Given I inject 'CreateCustomerEvent' into Kafka
		And I inject 'AssociateCustomerUserEvent' into Kafka
	Then user and customer are associated

Scenario: Integration associate project subscription
	Given I inject 'CreateProjectEvent' into Kafka
		And I inject 'CreateProjectSubscriptionEvent' into Kafka
		And I inject 'AssociateProjectCustomer' into Kafka
		And I inject 'AssociateProjectSubscriptionEvent' into Kafka
	When I make a Web API request for a list of projects
	Then the created project is in the list
		And the number of days to subscription expiry is correct

Scenario: Integration update project subscription
	Given I inject 'CreateProjectEvent' into Kafka
		And I inject 'CreateProjectSubscriptionEvent' into Kafka
		And I inject 'AssociateProjectCustomer' into Kafka
		And I inject 'AssociateProjectSubscriptionEvent' into Kafka
		And I inject 'UpdateProjectSubscriptionEvent' into Kafka
	When I make a Web API request for a list of projects
	Then the created project is in the list
		And the updated number of days to subscription expiry is correct

Scenario: Integration update project details
	Given I inject 'CreateProjectEvent' into Kafka
		And I inject 'CreateProjectSubscriptionEvent' into Kafka
		And I inject 'AssociateProjectCustomer' into Kafka
		And I inject 'AssociateProjectSubscriptionEvent' into Kafka
		And I make a Web API request for a list of projects
		And the created project is in the list
		And I inject 'UpdateProjectEvent' into Kafka
	When I make a Web API request for a list of projects
	Then the project details are updated

Scenario: Integration delete project
	Given I inject 'CreateProjectEvent' into Kafka
		And I inject 'CreateProjectSubscriptionEvent' into Kafka
		And I inject 'AssociateProjectCustomer' into Kafka
		And I inject 'AssociateProjectSubscriptionEvent' into Kafka
		And I make a Web API request for a list of projects
		And the created project is in the list
		And I inject 'DeleteProjectEvent' into Kafka
	When I make a Web API request for a list of projects
	Then the created project is not in the list

@ignore @notImplemented
Scenario: Integration disassociate project subscription
	Given I inject 'CreateProjectEvent' into Kafka
		And I inject 'CreateProjectSubscriptionEvent' into Kafka
		And I inject 'AssociateProjectCustomer' into Kafka
		And I inject 'AssociateProjectSubscriptionEvent' into Kafka
		And I make a Web API request for a list of projects
		And the created project is in the list
		And I inject 'DissociateProjectSubscriptionEvent' into Kafka
	When I make a Web API request for a list of projects
	Then the created project is not in the list