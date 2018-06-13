Feature: Patch
	I should be able to request Production Data Patch

Background: 
	Given the Patch service URI "/api/v1/productiondata/patches", request repo "PatchRequest.json" and result repo "PatchResponse.json"

@ignore
Scenario Outline: Patch - Good Request
	When I request Production Data Patch supplying "<ParameterName>" paramters from the repository
	Then the Production Data Patch response should match "<ResultName>" result from the repository
	Examples: 
	| ParameterName              | ResultName                 |
	| HeightNoFilter   | HeightNoFilter   |
	| HeightAreaFilter | HeightAreaFilter |

Scenario Outline: Patch - Bad Request
	When I request Patch supplying "<ParameterName>" paramters from the repository expecting http error code <httpCode>
	Then the response cell should contain error code <errorCode>
	Examples: 
	| ParameterName			| httpCode | errorCode |
	| NullProjectId			| 400      | -1        |
