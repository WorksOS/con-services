Feature: SummaryThickness
  I should be able to request Summary Thickness.

Scenario Outline: SummaryThickness - Good Request
  Given the service route "/api/v1/thickness/summary" request repo "SummaryThicknessRequest.json" and result repo "SummaryThicknessResponse.json"
  When I POST with parameter "<ParameterName>" I expect response code <HttpCode>
  Then the response should match "<ResultName>" from the repository
  Examples: 
  | ParameterName    | ResultName       | HttpCode |
  | LatestToEarliest | LatestToEarliest | 200      |
  | LayerIdFiltered  | LayerIdFiltered  | 200      |
  | PartialOverlap   | PartialOverlap   | 200      |

Scenario Outline: SummaryThickness - Bad Request
  Given the service route "/api/v1/thickness/summary" request repo "SummaryThicknessRequest.json" and result repo "SummaryThicknessResponse.json"
  When I POST with parameter "<ParameterName>" I expect response code <HttpCode>
  Then the response should contain message "<ErrorMessage>" and code "<ErrorCode>"
  Examples: 
  | ParameterName           | ErrorCode | ErrorMessage                                                    | HttpCode |
  | NegativeThicknessTarget | -1        | Targte thickness settings must be positive.                     | 400      |
  | NonExistentBaseSurface  | -4        | Failed to get/update data requested by SummaryThicknessExecutor | 400      |
