Feature: CoordinateSystemPostValidate
	If I have a coordinate system file then I should be able to post it for a validation

Background: 
	Given the Coordinate service URI "/api/v1/coordsystem/validation", request repo "CoordSysValidationRequest.json" and result repo "CoordSysValidationResponse.json"

@mytag
Scenario Outline: CoordSystemPostValidate - Good Request
	When I Post CoordinateSystemValidation supplying "<ParameterName>" paramters from the repository
	Then the CoordinateSystemValidation response should match "<ResultName>" result from the repository
	Examples: 
	| ParameterName             | ResultName                |
	| CoordinateSystem          | CoordinateSystem          |
	| CoordinateSystemWithGeoid | CoordinateSystemWithGeoid |

Scenario Outline: CoordinateSystemPostValidate - Bad Request
	When I Post CoordinateSystemValidation supplying "<ParameterName>" paramters from the repository expecting http error code <httpCode>
	Then the response should contain error code <errorCode>
	Examples: 
	| ParameterName     | httpCode | errorCode |
	| NullFileContents  | 400      | -1        |
	| NullFileName      | 400      | -1        |
	| FileNameTooLong   | 400      | -1        |
	| EmptyFileContents | 400      | -4        |
