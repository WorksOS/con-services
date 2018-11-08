Feature: PassCountSummary
  I should be able to request Pass Count Summary.

Scenario Outline: PassCountSummary - Good Request
  Given the service route "/api/v1/compaction/passcounts/summary" request repo "PassCountSummaryRequest.json" and result repo "PassCountSummaryResponse.json"
  When I POST with parameter "<ParameterName>" I expect response code <HttpCode>
  Then the response should match "<ResultName>" from the repository
  Examples: 
  | ParameterName                    | ResultName                       | HttpCode |
  | PassCountSummary_SingleTarget    | PassCountSummary_SingleTarget    | 200      |
  | PassCountSummary_LowRangeTarget  | PassCountSummary_LowRangeTarget  | 200      |
  | PassCountSummary_HighRangeTarget | PassCountSummary_HighRangeTarget | 200      |

Scenario Outline: PassCountSummary - Bad Request
  Given the service route "/api/v1/compaction/passcounts/summary" request repo "PassCountSummaryRequest.json" and result repo "PassCountSummaryResponse.json"
  When I POST with parameter "<ParameterName>" I expect response code <HttpCode>
  Then the response should contain code "<ErrorCode>"
  Examples: 
  | ParameterName                     | HttpCode | ErrorCode |
  | PassCountSummary_NullProjectId    | 400      | -1        |
  | PassCountSummary_InvalidProjectId | 400      | -1        |
  | PassCountSummary_BadRange         | 400      | -1        |
