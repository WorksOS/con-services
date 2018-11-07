Feature: ElevationStatistics
  I should be able to request Elevation Statistics for a project.

Scenario Outline: ElevationStatistics - Good Request
  Given the service route "/api/v1/statistics/elevation" request repo "ElevationStatsRequest.json" and result repo "ElevationStatsResponse.json"
  When I POST with parameter "<ParameterName>" I expect response code <HttpCode>
  Then the response should match "<ResultName>" from the repository
  Examples: 
  | ParameterName       | ResultName          | HttpCode |
  | NoFilter            | NoFilter            | 200      |
  | FiveCells           | FiveCells           | 200      |
  | FiveCellsOneMachine | FiveCellsOneMachine | 200      |