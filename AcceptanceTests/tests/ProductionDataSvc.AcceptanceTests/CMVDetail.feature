Feature: CMVDetail
	I should be able to request CMV Details.

Background: 
	Given the CMV Details service URI "/api/v1/compaction/cmv/detailed", request repo "CMVDetailRequest.json" and result repo "CMVDetailResponse.json"

Scenario Outline: CMVDetail - Good Request
	When I request CMV Details supplying "<ParameterName>" paramters from the repository
	Then the CMV Details response should match "<ResultName>" result from the repository
	Examples: 
	| ParameterName              | ResultName                 |
	| OverrideTargetAllLayer     | OverrideTargetAllLayer     |
	| OverrideTargetTopLayerOnly | OverrideTargetTopLayerOnly |
	| NotOverrideTarget          | NotOverrideTarget          |
	
Scenario Outline: CMVDetail - Bad Request
	When I request CMV Details supplying "<ParameterName>" paramters from the repository expecting http error code <httpCode>
	Then the response should contain error code <errorCode>
	Examples: 
	| ParameterName          | httpCode | errorCode |
	| MinCmvTooSmall         | 400      | -1        |
	| MaxCmvTooLarge         | 400      | -1        |
	| MinCmvLargerThanMaxCmv | 400      | -1        |
	| CmvTargetOutOfBound    | 400      | -1        |