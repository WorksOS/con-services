Feature: LineworkFile

Scenario Outline: LineworkFile - Bad Request
  Given the service route "/api/v2/linework/boundaries" and result repo "LineworkFileResponse.json"
  And with property DxfUnits with value "<DxfUnits>"
  And with property MaxBoundariesToProcess with value "<MaxBoundariesToProcess>"
  And with property DxfFile with value "<DxfFile>"
  And with property CoordinateSystemFile with value "<CoordinateSystemFile>"
  When I POST the multipart request I expect response code <HttpCode>
  Then the response should match "<ResultName>" from the repository
  Examples: 
  | DxfUnits | MaxBoundariesToProcess | DxfFile             | CoordinateSystemFile | ResultName                               | HttpCode |
  | 2        | 1                      | Null.dxf            | Dimensions_2012.dc   | NullDxfFileData                          | 400      |
  | 2        | 1                      | Dimensions_2012.dxf | Null.dc              | NullDCFileData                           | 400      |
  | 2        | 1                      | Dimensions_2012.dc  | Dimensions_2012.dc   | ErrorFailedToReadLineworkBoundaryFile    | 400      |
  | 2        | 1                      | Dimensions_2012.dxf | Dimensions_2012.dxf  | ErrorFailedToPerformCoordinateConversion | 400      |
  | 3        | 1                      | Dimensions_2012.dxf | Dimensions_2012.dc   | OutOfRangeDxfUnit                        | 400      |
  | -1       | 1                      | Dimensions_2012.dxf | Dimensions_2012.dc   | NotSuppliedDxfUnit                       | 400      |
  | 2        | 1                      | 0_boundaries.dxf    | Dimensions_2012.dc   | ErrorNoBoundariesInFile                  | 400      |

Scenario Outline: LineworkFile - Good Request
  Given the service route "/api/v2/linework/boundaries" and result repo "LineworkFileResponse.json"
  And with property DxfUnits with value "<DxfUnits>"
  And with property MaxBoundariesToProcess with value "<MaxBoundariesToProcess>"
  And with property ConvertLineStringCoordsToPolygon with value "<ConvertLineStringCoordsToPolygon>"
  And with property DxfFile with value "<DxfFile>"
  And with property CoordinateSystemFile with value "<CoordinateSystemFile>"
  When I POST the multipart request I expect response code <HttpCode>
  Then the response should match "<ResultName>" from the repository
  Examples: 
  | DxfUnits | MaxBoundariesToProcess | ConvertLineStringCoordsToPolygon | DxfFile                        | CoordinateSystemFile | ResultName              | HttpCode |
  | 2        | 21                     |                                  | Dimensions_2012.dxf            | Dimensions_2012.dc   | 21BoundariesDXF         | 200      |
  | 2        | 1                      |                                  | Dimensions_2012.dxf            | Dimensions_2012.dc   | 1BoundaryDXF            | 200      |
  | 2        | 0                      |                                  | Dimensions_2012_LineString.dxf | Dimensions_2012.dc   | 1LineStringDXF          | 200      |
  | 2        | 0                      | true                             | Dimensions_2012_LineString.dxf | Dimensions_2012.dc   | 1LineStringToPolygonDXF | 200      |
