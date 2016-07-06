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

Scenario: Machine lifts - between specific dates
	When I try to get machine lift details for project 'PPG Track Wheel' between dates '2016-05-03&2015-05-03'
	Then the following machine lift details are returned
		"""
		[
		  {
			"assetId": 1000001,
			"machineName": "BLUE TRUCK",
			"isJohnDoe": true,
			"lifts": [
			  {
				"layerId": 1,
				"endTime": "2016-05-03T04:53:53.977"
			  }
			]
		  }
		]
		"""

Scenario: Machine lifts - two years
	When I try to get machine lift details for project 'PPG Track Wheel' between dates 'NotSpecified'
	Then the following machine lift details are returned
		"""
		[
		  {
			"assetId": 1000001,
			"machineName": "BLUE TRUCK",
			"isJohnDoe": true,
			"lifts": [
			  {
				"layerId": 1,
				"endTime": "2016-05-03T04:53:53.977"
			  }
			]
		  }
		]
		"""

Scenario: Gets volume and time summary for a landfill project
	When I try to get volume and time summary for project 'Casella-Stanley Landfill'
	Then the remaining volume is 98745.9204653804 and the remaining time is 271.14651230890672