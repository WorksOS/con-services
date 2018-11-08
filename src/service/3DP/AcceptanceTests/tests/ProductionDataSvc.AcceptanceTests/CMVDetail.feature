Feature: CMVDetail
  I should be able to request CMV Details.

Scenario Outline: CMVDetail - Good Request
  Given the service route "/api/v1/compaction/cmv/detailed" request repo "CMVDetailRequest.json" and result repo "CMVDetailResponse.json"
  When I POST with parameter "<ParameterName>" I expect response code <HttpCode>
  Then the response should match "<ResultName>" from the repository
  Examples: 
  | ParameterName              | ResultName                 | HttpCode |
  | OverrideTargetAllLayer     | OverrideTargetAllLayer     | 200      |
  | OverrideTargetTopLayerOnly | OverrideTargetTopLayerOnly | 200      |
  | NotOverrideTarget          | NotOverrideTarget          | 200      |
  
Scenario Outline: CMVDetail - Bad Request
  Given the service route "/api/v1/compaction/cmv/detailed" request repo "CMVDetailRequest.json" and result repo "CMVDetailResponse.json"
  When I POST with parameter "<ParameterName>" I expect response code <HttpCode>
  Then the response should contain code "<ErrorCode>"
  Examples: 
  | ParameterName          | HttpCode | ErrorCode |
  | MinCmvTooSmall         | 400      | -1        |
  | MaxCmvTooLarge         | 400      | -1        |
  | MinCmvLargerThanMaxCmv | 400      | -1        |
  | CmvTargetOutOfBound    | 400      | -1        |