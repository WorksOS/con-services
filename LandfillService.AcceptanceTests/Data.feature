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
	When adding a random weight for project (1384) five days ago
	Then match response (Ok 200)
	And check the random weight has been added to the project (1384) for five days ago

Scenario: Add five weight entries for a five days
	When adding five random weights for project (1384) ten days ago
	Then match response (Ok 200)
	And check the five random weights has been added each day to the project (1384) 

Scenario: Check the density for a specific date (2015-04-04)  
	Given Get project data for project (1384)
	Then match response (Ok 200)
	And check the density is (1582.91241710074) for the date (2015-04-04) 

Scenario: Update the weight for a specific date (2015-04-06) 
	When updating a weight (6666) tonnes for project (1384) for date (2015-04-06) 
	Then match response (Ok 200)
	And check the density is calculated with a volume of (3129.9334339477587) for the date (2015-04-06) 

# Add some range tests negative tests at calling the web service
Scenario: Add a weight entry for a yesterday
	When adding a random weight for project (1384) yesterday
	Then match response (Ok 200)
	And check the random weight has been added to the project (1384) for yesterday

Scenario: Add a weight entry for a today
	When adding a random weight for project (1384) today
	Then match response (Ok 200)
#	And check the random weight has been added to the project (1384) for today

Scenario: Add a weight entry for a tomorrow
	When adding a random weight for project (1384) tomorrow
	Then match response (Ok 200)
#	And check the random weight has been added to the project (1384) for tomorrow
