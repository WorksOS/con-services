Feature: Projects

Background: 
	Given I am logged in with good credentials

Scenario: Project list
	When I try to get a list of all projects
	Then the project 'Casella-Stanley Landfill' is in the list

Scenario: Project data
	When I try to get data for project 'Casella-Stanley Landfill'
	Then the response contains data for the past two years
