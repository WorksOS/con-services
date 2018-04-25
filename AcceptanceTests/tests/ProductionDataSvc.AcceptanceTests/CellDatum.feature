Feature: CellDatum
I should be able to request Production Data Cell Datum.

Background: 
	Given the CellDatum service URI "/api/v1/productiondata/cells/datum", request repo "CellDatumRequest.json" and result repo "CellDatumResponse.json"

Scenario Outline: CellDatum - Good Request
	When I request Production Data Cell Datum supplying "<ParameterName>" paramters from the repository
	Then the Production Data Cell Datum response should match "<ResultName>" result from the repository
	Examples: 
	| ParameterName             | ResultName                |
	| Height                    | Height                    |
	| PassCount                 | PassCount                 |
	| Temperature               | Temperature               |
	| HeightFilterByLayerNumber | HeightFilterByLayerNumber |
	| HeightDefaultToLatestPass | HeightDefaultToLatestPass |
	| HeightFilterToBlade       | HeightFilterToBlade       |
	| HeightFilterToTrack       | HeightFilterToTrack       |
	| HeightFilterToBladeTrack  | HeightFilterToBladeTrack  |
	| HeightFilterToWheel       | HeightFilterToWheel       |
	| HeightFilterToBladeWheel  | HeightFilterToBladeWheel  |

Scenario Outline: CellDatum - Bad Request
	When I request Cell Datum supplying "<ParameterName>" paramters from the repository expecting http error code <httpCode>
	Then the response should contain error code <errorCode>
	Examples: 
	| ParameterName			| httpCode | errorCode |
	| NullProjectId			| 400      | -1        |