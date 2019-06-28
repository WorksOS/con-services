Feature: Compaction Cell Passes
  I should be able to request Production Data Cell Passes.

Scenario Outline: Bad Requests
  Given only the service route "/api/v2/productiondata/cells/passes"
  And with parameter "projectUid" with value "<ProjectUID>"
  And with parameter "filterUid" with value "<FilterUID>"
  And with parameter "lat" with value "<Lat>"
  And with parameter "lon" with value "<Lon>"
  When I send the GET request I expect response code <HttpCode>
  Then the response should contain message "<Message>" and code "<ErrorCode>"
  Examples: 
  | ProjectUID                           | FilterUID                            | Lat                | Lon                | HttpCode | ErrorCode | Message                                       |
  | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | 5e089924-98cb-49a6-8323-19537dc6d665 | 36.206985688803016 | -115.0201493239474 | 400      | -4        | No cell passes found                          |
  | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | 5e089924-98cb-49a6-8323-19537dc6d665 |                    | -115.0201493239474 | 400      | -4        | No cell passes found                          |
  | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | 5e089924-98cb-49a6-8323-19537dc6d665 | 36.206985688803016 |                    | 400      | -4        | No cell passes found                          |
  |                                      | 5e089924-98cb-49a6-8323-19537dc6d665 | 36.206985688803016 | -115.0201493239474 | 400      | -1        | ProjectId and ProjectUID cannot both be null. |
