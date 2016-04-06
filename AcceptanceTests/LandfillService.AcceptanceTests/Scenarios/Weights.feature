Feature: Weights

Background: 
	Given I am logged in with good credentials

Scenario: Add weight
	When I add a weight for yesterday to project 'Casella-Stanley Landfill'
	Then the weight is added for yesterday to project 'Casella-Stanley Landfill'
