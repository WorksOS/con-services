Feature: PassCountSummary
	I should be able to request Pass Count Summary.

Background: 
Given the Pass Count Summary service URI "/api/v1/compaction/passcounts/summary", request repo "PassCountSummaryRequest.json" and result repo "PassCountSummaryResponse.json"

#  @ignore
Scenario Outline: PassCountSummary - Good Request
When I request Pass Count Summary supplying "<ParameterName>" paramters from the repository
Then the Pass Count Summary response should match "<ResultName>" result from the repository
Examples: 
| ParameterName                    | ResultName                       |
| PassCountSummary_SingleTarget    | PassCountSummary_SingleTarget    |
| PassCountSummary_LowRangeTarget  | PassCountSummary_LowRangeTarget  |
| PassCountSummary_HighRangeTarget | PassCountSummary_HighRangeTarget |  

Scenario Outline: PassCountSummary - Bad Request
When I request Pass Count Summary supplying "<ParameterName>" paramters from the repository expecting http error code <httpCode>
Then the response should contain error code <errorCode>
Examples: 
| ParameterName                     | httpCode | errorCode |
| PassCountSummary_NullProjectId    | 400      | -1        |
| PassCountSummary_InvalidProjectId | 400      | -1        |
| PassCountSummary_BadRange         | 400      | -1        |
