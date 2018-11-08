Feature: CoordinateSystemGet
  I should be able to get coordinate system

Scenario Outline: CoordinateSystemGet - Good Request
  Given the service route "/api/v1/projects/<ProjectId>/coordsystem" and result repo "CoordSysResponse.json"
  When I send the GET request I expect response code <HttpCode>
  Then the response should match "<ResultName>" from the repository
  Examples: 
  | ProjectId | ResultName          | HttpCode |
  | 1001152   | GetCoordinateSystem | 200      |

Scenario Outline: CoordinateSystemGet - Bad Request
  Given the service route "/api/v1/projects/<ProjectId>/coordsystem" and result repo "CoordSysResponse.json"
  When I send the GET request I expect response code <HttpCode>
  Then the response should contain code <ErrorCode>
  Examples: 
  | ProjectId | HttpCode | ErrorCode |
  | 0         | 400      | -1        |
  | 1099999   | 400      | -4        |
