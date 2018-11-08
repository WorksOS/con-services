Feature: CoordinateSystemPost
  If I have a coordinate system then I should be able to post it.

Scenario Outline: CoordinateSystemPost - Good Request
  Given the service route "/api/v1/coordsystem" request repo "CoordSysRequest.json" and result repo "CoordSysResponse.json"
  When I POST with parameter "<ParameterName>" I expect response code <HttpCode>
  Then the response should match "<ResultName>" from the repository
  Examples: 
  | ParameterName             | ResultName                | HttpCode |
  | CoordinateSystem          | CoordinateSystem          | 200      |
  | CoordinateSystemWithGeoid | CoordinateSystemWithGeoid | 200      |

Scenario Outline: CoordinateSystemPost - Bad Request
  Given the service route "/api/v1/coordsystem" request repo "CoordSysRequest.json" and result repo "CoordSysResponse.json"
  When I POST with parameter "<ParameterName>" I expect response code <HttpCode>
  Then the response should contain code "<ErrorCode>"
  Examples: 
  | ParameterName    | HttpCode | ErrorCode |
  | NullFileContents | 400      | -1        |
  | NullFileName     | 400      | -1        |
  | InvalidProjectId | 400      | -1        |
  | FileNameTooLong  | 400      | -1        |
  | NullRequest      | 400      | -1        |
