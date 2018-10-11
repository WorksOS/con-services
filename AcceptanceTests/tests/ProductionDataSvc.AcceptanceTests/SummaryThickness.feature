Feature: SummaryThickness
	I should be able to request Summary Thickness.

Background: 
	Given the Summary Thickness service URI "/api/v1/thickness/summary", request repo "SummaryThicknessRequest.json" and result repo "SummaryThicknessResponse.json"

Scenario Outline: SummaryThickness - Good Request
	When I request Summary Thickness supplying "<ParameterName>" paramters from the repository
	Then the response should match "<ResultName>" result from the repository
	Examples: 
	| ParameterName    | ResultName       |
	| LatestToEarliest | LatestToEarliest |
	| LayerIdFiltered  | LayerIdFiltered  |
	| PartialOverlap   | PartialOverlap   |

Scenario Outline: SummaryThickness - Bad Request
	When I make invalid request for Summary Thickness supplying "<ParameterName>" paramters from the repository 
	Then the response body should contain Code <ErrorCode> and Message "<ErrorMessage>"
	Examples: 
	| ParameterName           | ErrorCode | ErrorMessage                                                    |
	| ParameterName           | ErrorCode | ErrorMessage                                                    |
	| NegativeThicknessTarget | -1        | Targte thickness settings must be positive.                     |
	| NonExistentBaseSurface  | -4        | Failed to get/update data requested by SummaryThicknessExecutor |