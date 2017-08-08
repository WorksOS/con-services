Feature: CMVSummary
	I should be able to request CMV summary.

Background: 
	Given the CMV Summary service URI "/api/v1/compaction/cmv/summary", request repo "CMVSummaryRequest.json" and result repo "CMVSummaryResponse.json"

Scenario Outline: CMVSummary - Good Request
	When I request CMV Summary supplying "<ParameterName>" paramters from the repository
	Then the CMV Summary response should match "<ResultName>" result from the repository
	Examples: 
	| ParameterName                 | ResultName                    |
	| OverrideTargetAllLayers       | OverrideTargetAllLayers       |
	| OverrideTargetTopLayerOnly    | OverrideTargetTopLayerOnly    |
	| NotOverrideTargetAllLayers    | NotOverrideTargetAllLayers    |
	| NotOverrideTargetTopLayerOnly | NotOverrideTargetTopLayerOnly |

Scenario Outline: CMVSummary - Bad Request
	When I request CMV Summary supplying "<ParameterName>" paramters from the repository expecting http error code <httpCode>
	Then the response should contain error code <errorCode>
	Examples: 
	| ParameterName                     | httpCode | errorCode |
	| MinCmvPercentTooSmall             | 400      | -1        |
	| MaxCmvPercentTooLarge             | 400      | -1        |
	| MinCmvPercentLargerThanMaxPercent | 400      | -1        |