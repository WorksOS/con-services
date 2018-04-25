Feature: CellPasses
	I should be able to request Production Data Cell Passes.

Background: 
	Given the CellPass service URI "/api/v1/productiondata/cells/passes", request repo "CellPassesRequest.json" and result repo "CellPassesResponse.json"

Scenario Outline: CellPasses - Good Request
	When I request Production Data Cell Passes supplying "<ParameterName>" paramters from the repository
	Then the Production Data Cell Passes response should match "<ResultName>" result from the repository
	Examples: 
	| ParameterName         | ResultName            |
	| All                   | All                   |
	| CCV                   | CCV                   |
	| MDP                   | MDP                   |
	| LiftDetectionTypeNone | LiftDetectionTypeNone |
	| GpsModeStoreWheel     | GpsModeStoreWheel     |
	| GpsModeStoreTrack     | GpsModeStoreTrack     |

Scenario Outline: CellPasses - Bad Request
	When I request Cell Passes supplying "<ParameterName>" paramters from the repository expecting http error code <httpCode>
	Then the response should contain error code <errorCode>
	Examples: 
	| ParameterName			| httpCode | errorCode |
	| NullProjectId			| 400      | -1        |
