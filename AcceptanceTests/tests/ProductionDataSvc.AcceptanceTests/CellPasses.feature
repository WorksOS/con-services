Feature: CellPasses
  I should be able to request Production Data Cell Passes.

Scenario Outline: CellPasses - Good Request
  Given the service route "/api/v1/productiondata/cells/passes" request repo "CellPassesRequest.json" and result repo "CellPassesResponse.json"
  When I POST with parameter "<ParameterName>" I expect response code <HttpCode>
  Then the response should match "<ResultName>" from the repository
  Examples: 
  | ParameterName         | ResultName            | HttpCode |
  | All                   | All                   | 200      |
  | CCV                   | CCV                   | 200      |
  | MDP                   | MDP                   | 200      |
  | LiftDetectionTypeNone | LiftDetectionTypeNone | 200      |
  | GpsModeStoreWheel     | GpsModeStoreWheel     | 200      |
  | GpsModeStoreTrack     | GpsModeStoreTrack     | 200      |

Scenario Outline: CellPasses - Bad Request
  Given the service route "/api/v1/productiondata/cells/passes" request repo "CellPassesRequest.json" and result repo "CellPassesResponse.json"
  When I POST with parameter "<ParameterName>" I expect response code <HttpCode>
  Then the response should contain code "<ErrorCode>"
  Examples: 
  | ParameterName      | HttpCode | ErrorCode |
  | NullProjectId      | 400      | -1        |
