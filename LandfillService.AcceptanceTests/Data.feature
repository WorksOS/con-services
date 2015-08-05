Feature: Data
	I should be able to request and post data to the landfill service / Web API.

Background: 
	Given login goodCredentials

Scenario: Get a list of projects
	Given Get a list of all projects
	Then match response (Ok 200)
	And check the (Project 1384) is in the list

Scenario: Get project data
	Given Get project data for project (1384)
	Then match response (Ok 200)
	And check there is 729 days worth of data for project (1384)

Scenario: Add a weight entry for a day, five days ago
	When adding a random weight for project (1384) five days ago in timezone 'America/Chicago'
	Then match response (Ok 200)
	And check the random weight has been added to the project (1384) for five days ago in timezone 'America/Chicago'

Scenario: Add five random weights entries for ten days ago
	When adding five random weights for project (1384) ten days ago in timezone 'America/Chicago'
	Then match response (Ok 200)
	And check the five random weights has been added each day to the project (1384) in timezone 'America/Chicago'

Scenario: Check the density for a specific date (2015-04-10)  
	Given Get project data for project (1384)
	Then match response (Ok 200)
	And check the density is (866.3804) for the date (2015-04-10) 

Scenario: Update the weight for a specific date (2015-04-06) 
	When updating a weight (3000) tonnes for project (1384) for date (2015-04-06) 
	Then match response (Ok 200)
	And check the density is re-calculated as (821.618681856186) for the date (2015-04-06) 

Scenario: Update the weight for a specific date (2015-04-06) back to original
	When updating a weight (3239.58) tonnes for project (1384) for date (2015-04-06) 
	Then match response (Ok 200)
	And check the density is re-calculated as (887.233149789221) for the date (2015-04-06) 

Scenario: Add a weight entry for a yesterday
	When adding a random weight for project (1384) yesterday in timezone 'America/Chicago'
	Then match response (Ok 200)
	And check the random weight has been added to the project (1384) for yesterday in timezone 'America/Chicago'
# Add some range tests negative tests at calling the web service
Scenario: Add a weight entry for a today
	When adding a random weight for project (1384) today in timezone 'America/Chicago'
	Then match response (Ok 200)

Scenario: Add a weight entry for a tomorrow
	When adding a random weight for project (1384) tomorrow in timezone 'America/Chicago'
	Then match response (Ok 200)

