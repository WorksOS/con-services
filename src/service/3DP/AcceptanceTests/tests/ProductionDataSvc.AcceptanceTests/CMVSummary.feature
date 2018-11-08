Feature: CMVSummary
  I should be able to request CMV summary.

Scenario Outline: CMVSummary - Good Request
  Given the service route "/api/v1/compaction/cmv/summary" request repo "CMVSummaryRequest.json" and result repo "CMVSummaryResponse.json"
  When I POST with parameter "<ParameterName>" I expect response code <HttpCode>
  Then the response should match "<ResultName>" from the repository
  Examples: 
  | ParameterName                 | ResultName                    | HttpCode |
  | OverrideTargetAllLayers       | OverrideTargetAllLayers       | 200      |
  | OverrideTargetTopLayerOnly    | OverrideTargetTopLayerOnly    | 200      |
  | NotOverrideTargetAllLayers    | NotOverrideTargetAllLayers    | 200      |
  | NotOverrideTargetTopLayerOnly | NotOverrideTargetTopLayerOnly | 200      |

Scenario Outline: CMVSummary - Bad Request
  Given the service route "/api/v1/compaction/cmv/summary" request repo "CMVSummaryRequest.json" and result repo "CMVSummaryResponse.json"
  When I POST with parameter "<ParameterName>" I expect response code <HttpCode>
  Then the response should contain code "<ErrorCode>"
  Examples: 
  | ParameterName                     | HttpCode | ErrorCode |
  | MinCmvPercentTooSmall             | 400      | -1        |
  | MaxCmvPercentTooLarge             | 400      | -1        |
  | MinCmvPercentLargerThanMaxPercent | 400      | -1        |