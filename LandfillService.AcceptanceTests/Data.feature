Feature: Data
	I should be able to get data from the API.

Background: 
	Given login goodCredentials

Scenario: Get a list of projects
	Given getProjects
	Then match response (Ok 200)
	And not $ null response

Scenario: Get project data
	Given getData (Project 544)
	Then match response (Ok 200)
	And not $ null response 