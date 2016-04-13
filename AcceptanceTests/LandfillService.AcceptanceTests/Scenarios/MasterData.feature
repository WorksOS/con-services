Feature: MasterData

Scenario: Master Data association
	Given I inject 'CreateProjectEvent' into Kafka
		And I inject 'CreateProjectSubscriptionEvent' into Kafka
		And I inject 'AssociateProjectCustomer' into Kafka
		And I inject 'AssociateProjectSubscriptionEvent' into Kafka
	When I make a Web API request for a list of projects
	Then the created project is in the list
		And the number of days to subscription expiry is correct