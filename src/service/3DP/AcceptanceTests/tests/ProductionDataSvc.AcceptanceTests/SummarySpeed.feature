Feature: SummarySpeed
  I should be able to request Summary Speed.

Scenario Outline: SummarySpeed - Good Request 
  Given the service route "/api/v1/speed/summary" request repo "SummarySpeedRequest.json" and result repo "SummarySpeedResponse.json"
  When I POST with parameter "<ParameterName>" I expect response code <HttpCode>
  Then the response should match "<ResultName>" from the repository
  Examples: 
  | ParameterName          | ResultName             | HttpCode |
  | NoFilterAtAll          | NoFilterAtAll          | 200      |
  | ExcludeSupercededLifts | ExcludeSupercededLifts | 200      |
  | IncludeSupercededLifts | IncludeSupercededLifts | 200      |

Scenario Outline: SummarySpeed - Bad Request
  Given the service route "/api/v1/speed/summary" request repo "SummarySpeedRequest.json" and result repo "SummarySpeedResponse.json"
  When I POST with parameter "<ParameterName>" I expect response code <HttpCode>
  Then the response should contain message "<ErrorMessage>" and code "<ErrorCode>"
  Examples:
  | ParameterName                          | ErrorCode | ErrorMessage                                                | HttpCode |
  | MissingSpeedTarget                     | -1        | Target speed must be specified for the request.             | 400      |
  | MinSpeedTargetLargerThanMaxSpeedTarget | -1        | Target speed minimum must be less than target speed maximum | 400      |
