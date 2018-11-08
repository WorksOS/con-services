Feature: CoordinateSystemPostValidate
  If I have a coordinate system file then I should be able to post it for a validation

Scenario Outline: CoordSystemPostValidate - Good Request
  Given the service route "/api/v1/coordsystem/validation" request repo "CoordSysValidationRequest.json" and result repo "CoordSysValidationResponse.json"
  When I POST with parameter "<ParameterName>" I expect response code <HttpCode>
  Then the response should match "<ResultName>" from the repository
  Examples: 
  | ParameterName             | ResultName                | HttpCode |
  | CoordinateSystem          | CoordinateSystem          | 200      |
  | CoordinateSystemWithGeoid | CoordinateSystemWithGeoid | 200      |

Scenario Outline: CoordinateSystemPostValidate - Bad Request
  Given the service route "/api/v1/coordsystem/validation" request repo "CoordSysValidationRequest.json" and result repo "CoordSysValidationResponse.json"
  When I POST with parameter "<ParameterName>" I expect response code <HttpCode>
  Then the response should contain code "<ErrorCode>"
  Examples: 
  | ParameterName     | HttpCode | ErrorCode |
  | NullFileContents  | 400      | -1        |
  | NullFileName      | 400      | -1        |
  | FileNameTooLong   | 400      | -1        |
  | EmptyFileContents | 400      | -4        |
