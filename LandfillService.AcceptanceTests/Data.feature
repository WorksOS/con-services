Feature: Data
	I should be able to get data from the API.

Background: 
	Given login goodCredentials

Scenario: Get a list of projects
	Given Get a list of all projects
	Then match response (Ok 200)
	And check the (Project 1384) is in the list

Scenario: Get project data
	Given Get project data for project (Project 1384)
	Then match response (Ok 200)
	And check there is 729 days worth of data for project (Project 1384)

Scenario: Add a weight entry for a day
	When adding a (weight 12345 tonnes) for project (Project 1384) five days ago
	Then match response (Ok 200)
	And check the (weight 12345 tonnes) has been added to the project (Project 1384) for five days ago