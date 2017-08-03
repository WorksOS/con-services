Feature: SummarySpeed
	I should be able to request Summary Speed.

Background: 
	Given the Summary Speed service URI "/api/v1/speed/summary", request repo "SummarySpeedRequest.json" and result repo "SummarySpeedResponse.json"

Scenario Outline: SummarySpeed - Good Request 
	When I request Summary Speed supplying "<ParameterName>" paramters from the repository
	Then the response should match "<ResultName>" result from the repository
	Examples: 
	| ParameterName          | ResultName             |
	| NoFilterAtAll          | NoFilterAtAll          |
	| ExcludeSupercededLifts | ExcludeSupercededLifts |
	| IncludeSupercededLifts | IncludeSupercededLifts |

Scenario Outline: SummarySpeed - Bad Request
	When I request Summary Speed supplying "<ParameterName>" paramters from the repository expecting BadRequest
	Then the response body should contain Code <ErrorCode> and Message "<ErrorMessage>"
	Examples:
	| ParameterName                          | ErrorCode | ErrorMessage                                                |
	| MissingSpeedTarget                     | -1        | Target speed must be specified for the request.             |
	| MinSpeedTargetLargerThanMaxSpeedTarget | -1        | Target speed minimum must be less than target speed maximum |