Feature: Compaction Cell Passes
  I should be able to request Production Data Cell Passes.

Scenario Outline: Good Requests
  Given the service route "/api/v2/productiondata/cells/passes" and result repo "CompactionCellPassesResponse.json"
  And with parameter "projectUid" with value "<ProjectUID>"
  And with parameter "filterUid" with value "<FilterUID>"
  And with parameter "lat" with value "<Lat>"
  And with parameter "lon" with value "<Lon>"
  When I send the GET request I expect response code <HttpCode>
  Then the array response should match "<ResultName>" from the repository
  Examples: 
  | ProjectUID                           | FilterUID                            | Lat       | Lon         | HttpCode | ResultName    |
  | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | 5e089924-98cb-49a6-8323-19537dc6d665 | 36.207351 | -115.019772 | 200      | OnePassCount  |
  | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | 5e089924-98cb-49a6-8323-19537dc6d665 | 36.207587 | -115.020130 | 200      | TwoPassCounts |
  | 7925f179-013d-4aaf-aff4-7b9833bb06d6 |                                      | 36.207587 | -115.020130 | 200      | TwoPassCounts |
