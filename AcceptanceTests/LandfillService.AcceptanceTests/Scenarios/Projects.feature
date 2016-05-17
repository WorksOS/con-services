Feature: Projects

Scenario: Project list
	When I try to get a list of all projects
	Then the project 'Casella-Stanley Landfill' is in the list with details
		| UID                                  | TimezoneName    | LegacyTimezoneName    |
		| 06A92E4F-FAA2-E511-80E5-0050568821E6 | America/Chicago | Central Standard Time |

Scenario: Project data - one day
	When I try to get data for
		| ProjectName              | GeofenceUID                          | DateRange |
		| Casella-Stanley Landfill | fb4f0e9d-12f4-11e5-b129-0050568838e5 | OneDay    |
	Then the response contains data for 'OneDay'

Scenario: Project data - three days
	When I try to get data for
		| ProjectName              | GeofenceUID                          | DateRange |
		| Casella-Stanley Landfill | fb4f0e9d-12f4-11e5-b129-0050568838e5 | ThreeDays |
	Then the response contains data for 'ThreeDays'

Scenario: Project data - two years
	When I try to get data for
		| ProjectName              | GeofenceUID                          | DateRange |
		| Casella-Stanley Landfill | fb4f0e9d-12f4-11e5-b129-0050568838e5 | TwoYears  |
	Then the response contains data for 'TwoYears'