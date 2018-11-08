Feature: CMVChange
  I should be able to request CMV change summary.

Scenario Outline: CMVChange - Good Request
  Given the service route "/api/v1/cmvchange/summary" request repo "CMVChangeRequest.json" and result repo "CMVChangeResponse.json"
  When I POST with parameter "<ParameterName>" I expect response code <HttpCode>
  Then the response should match "<ResultName>" from the repository
  Examples:
  | ParameterName                   | ResultName                      | HttpCode |
  | AllLayersNoFilterAtAll          | AllLayersNoFilterAtAll          | 200      |
  | AllLayersExcludeSupersededLifts | AllLayersExcludeSupersededLifts | 200      |
  | AllLayersIncludeSupersededLifts | AllLayersIncludeSupersededLifts | 200      |
  | TopLayerIncludeSupersededLifts  | TopLayerIncludeSupersededLifts  | 200      |
  | TopLayerNoFilterAtAll           | TopLayerNoFilterAtAll           | 200      |
  | NoneLiftDetection               | NoneLiftDetection               | 200      |
  | NoneLiftDetectionNoFilterAtAll  | NoneLiftDetectionNoFilterAtAll  | 200      |

Scenario Outline: CMVChange - Bad Request
  Given the service route "/api/v1/cmvchange/summary" request repo "CMVChangeRequest.json" and result repo "CMVChangeResponse.json"
  When I POST with parameter "<ParameterName>" I expect response code <HttpCode>
  Then the response should contain message "<ErrorMessage>" and code "<ErrorCode>"
  Examples:
  | ParameterName           | HttpCode | ErrorCode | ErrorMessage                                         |
  | DecendingBoundaryValues | 400      | -1        | CMVChangeSummaryValues should be in ascending order. |
