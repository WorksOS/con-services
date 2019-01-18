Feature: LineworkFile

Scenario Outline: LineworkFile - Bad Request
  Given the service route "/api/v2/linework/boundaries" request repo "LineworkFileRequest.json" and result repo "LineworkFileResponse.json"
  When I POST with parameter "<ParameterName>" I expect response code <HttpCode>
  Then the response should match "<ResultName>" from the repository
  Examples: 
  | ParameterName     | ResultName        | HttpCode |
  | NullFileData      | NullFileData      | 400      |
  | OutOfRangeDxfUnit | OutOfRangeDxfUnit | 400      |

Scenario Outline: LineworkFile - Good Request
  Given the service route "/api/v2/linework/boundaries" request repo "LineworkFileRequest.json" and result repo "LineworkFileResponse.json"
  When I POST with parameter "<ParameterName>" I expect response code <HttpCode>
  Then the response should match "<ResultName>" from the repository
  Examples: 
  | ParameterName   | ResultName      | HttpCode |
  | 21BoundariesDXF | 21BoundariesDXF | 200      |