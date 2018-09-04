Feature: CoordinateSystemGet
  I should be able to get coordinate system

Background: 
  Given the Coordinate service URI "/api/v1/projects/{0}/coordsystem" and the request repo "CoordSysResponse.json"

Scenario Outline: CoordinateSystemGet - Good Request
  When I try to get Coordinate System for project <ProjectId>
  Then the CoordinateSystem response should match "<ResultName>" result from the repository
  Examples: 
  | ProjectId | ResultName          |
  | 1001152   | GetCoordinateSystem |

Scenario Outline: CoordinateSystemGet - Bad Request
  When I try to get Coordinate System for project <ProjectId> expecting http error code <httpCode>
  Then the response should contain error code <errorCode>
  Examples: 
  | ProjectId | httpCode | errorCode |
  | 0         | 401      | -5        |
  | 1099999   | 400      | -4        |
