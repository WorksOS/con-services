Feature: CellDatum
  I should be able to request Production Data Cell Datum.

Scenario Outline: CellDatum - Good Request
  Given the service route "/api/v1/productiondata/cells/datum" request repo "CellDatumRequest.json" and result repo "CellDatumResponse.json"
  When I POST with parameter "<ParameterName>" I expect response code <HttpCode>
  Then the response should match "<ResultName>" from the repository
  Examples: 
  | ParameterName             | ResultName                | HttpCode |
  | Height                    | Height                    | 200      |
  | PassCount                 | PassCount                 | 200      |
  | Temperature               | Temperature               | 200      |
  | HeightFilterByLayerNumber | HeightFilterByLayerNumber | 200      |
  | HeightDefaultToLatestPass | HeightDefaultToLatestPass | 200      |
  | HeightFilterToBlade       | HeightFilterToBlade       | 200      |
  | HeightFilterToTrack       | HeightFilterToTrack       | 200      |
  | HeightFilterToBladeTrack  | HeightFilterToBladeTrack  | 200      |
  | HeightFilterToWheel       | HeightFilterToWheel       | 200      |
  | HeightFilterToBladeWheel  | HeightFilterToBladeWheel  | 200      |

Scenario Outline: CellDatum - Bad Request
  Given the service route "/api/v1/productiondata/cells/datum" request repo "CellDatumRequest.json" and result repo "CellDatumResponse.json"
  When I POST with parameter "<ParameterName>" I expect response code <HttpCode>
  Then the response should contain code "<ErrorCode>"
  Examples: 
  | ParameterName      | HttpCode | ErrorCode |
  | NullProjectId      | 400      | -1        |
