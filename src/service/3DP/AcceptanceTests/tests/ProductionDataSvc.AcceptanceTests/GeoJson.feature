Feature: GeoJson

Scenario Outline: GeoJson
  Given the service route "/api/v2/geojson/polyline/reducepoints" request repo "GeoJson/GeoJsonRequest.json" and result repo "GeoJson/GeoJsonResponse.json"
  When I POST with parameter "<ParameterName>" I expect response code <HttpCode>
  Then the response should match "<ResultName>" from the repository
  Examples: 
  | ParameterName             | ResultName           | HttpCode |
  | TooFewBoundaryPoints      | TooFewBoundaryPoints | 400      |
  | TooManyBoundaryPoints     | 47BoundaryPoints     | 200      |
  | NoMaxPointsBoundaryPoints | 47BoundaryPoints     | 200      |
  | 47BoundaryPoints          | 5BoundaryPoints      | 200      |