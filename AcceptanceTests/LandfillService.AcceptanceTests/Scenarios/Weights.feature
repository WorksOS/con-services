Feature: Weights

Background: 
	Given I am logged in with good credentials

Scenario: Add weight yesterday
	When I add weights for the past 1 days to site '1863fb2e-fd25-e311-9e53-0050568824d7' of project 'Pegasus'
	Then the weights are added for the past 1 days to site '1863fb2e-fd25-e311-9e53-0050568824d7' of project 'Pegasus'

Scenario: Add weight multi days
	When I add weights for the past 3 days to site '1863fb2e-fd25-e311-9e53-0050568824d7' of project 'Pegasus'
	Then the weights are added for the past 3 days to site '1863fb2e-fd25-e311-9e53-0050568824d7' of project 'Pegasus'

Scenario: Add weight specific date
	When I add weight for '2016-04-01' to site '1863fb2e-fd25-e311-9e53-0050568824d7' of project 'Pegasus'
	Then the weight is added for '2016-04-01' to site '1863fb2e-fd25-e311-9e53-0050568824d7' of project 'Pegasus'