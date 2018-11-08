Feature: Patch
  I should be able to request Production Data Patch

@ignore
Scenario Outline: Patch - Good Request
  Given the service route "/api/v1/productiondata/patches" request repo "PatchRequest.json" and result repo "PatchResponse.json"
  When I POST with parameter "<ParameterName>" I expect response code <HttpCode>
  Then the response should match "<ResultName>" from the repository
  Examples: 
  | ParameterName    | ResultName       | HttpCode |
  | HeightNoFilter   | HeightNoFilter   | 200      |
  | HeightAreaFilter | HeightAreaFilter | 200      |

Scenario Outline: Patch - Bad Request
  Given the service route "/api/v1/productiondata/patches" request repo "PatchRequest.json" and result repo "PatchResponse.json"
  When I POST with parameter "<ParameterName>" I expect response code <HttpCode>
  Then the response should contain code "<ErrorCode>"
  Examples: 
  | ParameterName | HttpCode | ErrorCode |
  | NullProjectId | 400      | -1        |
