Feature: CoordinateSystemPost
	If I have a coordinate system then I should be able to post it.

Background: 
	Given the Coordinate service URI "/api/v1/coordsystem", request repo "CoordSysRequest.json" and result repo "CoordSysResponse.json"

Scenario Outline: CoordinateSystemPost - Good Request
	When I Post CoordinateSystem supplying "<ParameterName>" paramters from the repository
	Then the CoordinateSystem response should match "<ResultName>" result from the repository
	Examples: 
	| ParameterName             | ResultName                |
	| CoordinateSystem          | CoordinateSystem          |
	| CoordinateSystemWithGeoid | CoordinateSystemWithGeoid |

Scenario Outline: CoordinateSystemPost - Bad Request
	When I Post CoordinateSystem supplying "<ParameterName>" paramters from the repository expecting http error code <httpCode>
	Then the response should contain error code <errorCode>
	Examples: 
	| ParameterName     | httpCode | errorCode |
	| NullFileContents  | 400      | -1        |
	| NullFileName      | 400      | -1        |
	| InvalidProjectId  | 400      | -1        |
	| FileNameTooLong   | 400      | -1        |
  | NullRequest       | 400      | -1        |
