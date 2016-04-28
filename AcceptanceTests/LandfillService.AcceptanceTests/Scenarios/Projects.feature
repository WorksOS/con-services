Feature: Projects

Background: 
	Given I am logged in with good credentials

Scenario: Project list
	When I try to get a list of all projects
	Then the project 'Casella-Stanley Landfill' is in the list with details
	| UID                                  | TimezoneName    | CurrentGenTimezoneName |
	| 06A92E4F-FAA2-E511-80E5-0050568821E6 | America/Chicago | Central Standard Time  |

Scenario: Project data
	When I try to get data for project 'Casella-Stanley Landfill'
	Then the response contains data for the past two years
