Feature: CoordinateConversion
	I should be able to convert coordinates from LL to NE or vice versa.

Background: 
	Given the Coordinate Conversion service URI "/api/v1/coordinateconversion"

Scenario: CoordinateConversion - LL to NE
	Given a project id 1001158 
	And the coordinate conversion type "LatLonToNorthEast"
	And these coordinates
		| x            | y           |
		| -2.007453062 | 0.631935272 |
		| -2.007483867 | 0.631929809 |
	When I request the coordinate conversion
	Then the result should be these
		| x        | y        |
		| 2884.667 | 1193.966 |
		| 2725.931 | 1159.225 |

Scenario: CoordinateConversion - NE to LL
	Given a project id 1001158 
		And the coordinate conversion type "NorthEastToLatLon"
		And these coordinates
			| x        | y        |
			| 2884.667 | 1193.966 |
			| 2725.931 | 1159.225 |
	When I request the coordinate conversion
	Then the result should be these
		| x            | y           |
		| -2.007453062 | 0.631935272 |
		| -2.007483867 | 0.631929809 |

Scenario Outline: CoordinateConversion - Bad Request (Bad LL)
	Given a project id 1001158
		And the coordinate conversion type "LatLonToNorthEast"
		And these coordinates
			| x            | y           |
			| -5.007453062 | 0.631935272 |
			| -2.007483867 | 5.631929809 |
	When I request the coordinate conversion expecting <httpCode>
	Then the response should contain error code <errorCode>
	Examples: 
	| httpCode | errorCode |
	| 400      | -1        |
