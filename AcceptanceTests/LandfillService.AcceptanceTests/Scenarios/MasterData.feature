Feature: MasterData

Scenario: Create customer
	Given I inject 'CreateCustomerEvent' into Kafka
	Then a new customer is created

Scenario: Associate project subscription
	Given I inject 'CreateProjectEvent' into Kafka
		And I inject 'CreateProjectSubscriptionEvent' into Kafka
		And I inject 'AssociateProjectCustomer' into Kafka
		And I inject 'AssociateProjectSubscriptionEvent' into Kafka
	When I make a Web API request for a list of projects
	Then the created project is in the list
		And the number of days to subscription expiry is correct

Scenario: Update project subscription
	Given I inject 'CreateProjectEvent' into Kafka
		And I inject 'CreateProjectSubscriptionEvent' into Kafka
		And I inject 'AssociateProjectCustomer' into Kafka
		And I inject 'AssociateProjectSubscriptionEvent' into Kafka
		And I inject 'UpdateProjectSubscriptionEvent' into Kafka
	When I make a Web API request for a list of projects
	Then the created project is in the list
		And the updated number of days to subscription expiry is correct

Scenario: Update project details
	Given I inject 'CreateProjectEvent' into Kafka
		And I inject 'CreateProjectSubscriptionEvent' into Kafka
		And I inject 'AssociateProjectCustomer' into Kafka
		And I inject 'AssociateProjectSubscriptionEvent' into Kafka
		And I make a Web API request for a list of projects
		And the created project is in the list
		And I inject 'UpdateProjectEvent' into Kafka
	When I make a Web API request for a list of projects
	Then the project details are updated

@ignore @notImplemented
Scenario: Disassociate project subscription
	Given I inject 'CreateProjectEvent' into Kafka
		And I inject 'CreateProjectSubscriptionEvent' into Kafka
		And I inject 'AssociateProjectCustomer' into Kafka
		And I inject 'AssociateProjectSubscriptionEvent' into Kafka
		And I make a Web API request for a list of projects
		And the created project is in the list
		And I inject 'DissociateProjectSubscriptionEvent' into Kafka
	When I make a Web API request for a list of projects
	Then the created project is not in the list