Feature: CMVChange
	I should be able to request CMV change summary.

Background: 
	Given the CMV change summary service URI "/api/v1/cmvchange/summary", request repo "CMVChangeRequest.json" and result repo "CMVChangeResponse.json"

Scenario Outline: CMVChange - Good Request
	When I request CMV change summary supplying "<RequestName>" from the request repository
	Then the result should match "<ResultName>" from the result repository
	Examples:
	| RequestName                     | ResultName                      |
	| AllLayersNoFilterAtAll          | AllLayersNoFilterAtAll          |
	| AllLayersExcludeSupersededLifts | AllLayersExcludeSupersededLifts |
	| AllLayersIncludeSupersededLifts | AllLayersIncludeSupersededLifts |
	| TopLayerIncludeSupersededLifts  | TopLayerIncludeSupersededLifts  |
	| TopLayerNoFilterAtAll           | TopLayerNoFilterAtAll           |
	| NoneLiftDetection               | NoneLiftDetection               |
	| NoneLiftDetectionNoFilterAtAll  | NoneLiftDetectionNoFilterAtAll  |

Scenario Outline: CMVChange - Bad Request
	When I request CMV change summary supplying "<RequestName>" from the request repository expecting BadRequest
	Then the reuslt should contain error code <ErrorCode> and error message "<ErrorMessage>"
	Examples:
	| RequestName             | ErrorCode | ErrorMessage                                         |
	| DecendingBoundaryValues | -1        | CMVChangeSummaryValues should be in ascending order. |
