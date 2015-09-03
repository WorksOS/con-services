Feature: Data
	I should be able to request and post data to the landfill service / Web API.

Background: 
	Given login goodCredentials
	And Get Project data for 'Casella-Stanley Landfill'

#look at the project data
Scenario: Get a list of projects
	Given Get a list of all projects
	Then match response (Ok 200)
	And check the project 'Casella-Stanley Landfill' is in the list

Scenario: Get project data
	Given Get Project data for 'Casella-Stanley Landfill'
	Then match response (Ok 200)
	And check there is 729 days worth of data

Scenario: Get subscription expiry days left
	Given Get Project data for 'Casella-Stanley Landfill'
	Then compare the subscription expiry days left to mySql database

#Checking the density values
Scenario: Check the density for a specific date (2015-04-10)  
	Given Get Project data for 'Casella-Stanley Landfill'
	Then match response (Ok 200)
	And check the calculated density is correct for the date (2015-04-10) 

Scenario: Update the weight for a specific date (2015-04-06) 
	When updating a weight (3000) tonnes for date (2015-04-06) 
	Then match response (Ok 200)
	And check the calculated density is correct for the date (2015-04-06) 

Scenario: Update the weight for a specific date (2015-04-06) back to original
	When updating a weight (3239.58) tonnes for date (2015-04-06) 
	Then match response (Ok 200)
	And check the calculated density is correct for the date (2015-04-06) 

#Add a weights and verify 
Scenario: Add a weight entry for a yesterday
	When adding a random weight for yesterday
	Then match response (Ok 200)
	And check the random weight has been added for yesterday

Scenario: Add a weight entry for a day, five days ago
	When adding a random weight for five days ago
	Then match response (Ok 200)
	And check the random weight has been added for five days ago

Scenario: Add five random weights entries for ten days ago
	When adding five random weights for ten days ago
	Then match response (Ok 200)
	And check the five random weights has been added each day

#Add some range tests negative tests at calling the web service
Scenario: Try to add a weight entry for a today
	When adding a random weight for today
	Then match response (Ok 200)

Scenario: Try to add a weight entry for a tomorrow
	When adding a random weight for tomorrow
	Then match response (Ok 200)

