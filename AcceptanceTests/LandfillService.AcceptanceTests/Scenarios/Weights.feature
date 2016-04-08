Feature: Weights

Background: 
	Given I am logged in with good credentials

Scenario: Add weight yesterday
	When I add a weight for yesterday to project 'Pegasus'
	Then the weight is added for yesterday to project 'Pegasus'

Scenario: Add weight multi days
	When I add weights for the past 3 days to project 'Pegasus'
	Then the weights are added for the past 3 days to project 'Pegasus'

Scenario: Add weight specific date
	When I add weight for '2016-04-01' to project 'Casella-Stanley Landfill'
	Then the weight is added for '2016-04-01' to project 'Casella-Stanley Landfill'