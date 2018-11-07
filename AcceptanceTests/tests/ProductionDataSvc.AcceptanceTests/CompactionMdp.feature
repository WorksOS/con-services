Feature: CompactionMdp
  I should be able to request compaction MDP data

Scenario Outline: Compaction Get MDP Summary - No Design Filter
  Given the service route "/api/v2/mdp/summary" and result repo "CompactionGetMDPDataResponse.json"
  And with parameter "projectUid" with value "<ProjectUID>"
  When I send the GET request I expect response code <HttpCode>
  Then the response should match "<ResultName>" from the repository
  Examples: 
  | RequestName     | ProjectUID                           | ResultName        | HttpCode |
  |                 | ff91dd40-1569-4765-a2bc-014321f76ace | NoDesignFilter    | 200      |
  | ProjectSettings | 3335311a-f0e2-4dbe-8acd-f21135bafee4 | NoDesignFilter_PS | 200      |


Scenario Outline: Compaction Get MDP Summary
  Given the service route "/api/v2/mdp/summary" and result repo "CompactionGetMDPDataResponse.json"
  And with parameter "projectUid" with value "<ProjectUID>"
  And with parameter "filterUid" with value "<FilterUID>"
  When I send the GET request I expect response code <HttpCode>
  Then the response should match "<ResultName>" from the repository
  Examples: 
  | RequestName       | ProjectUID                           | FilterUID                            | ResultName        | HttpCode |
  | DesignOutside     | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | 3d9086f2-3c04-4d92-9141-5134932b1523 | DesignOutside     | 200      |
  | DesignIntersects  | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | 81422acc-9b0c-401c-9987-0aedbf153f1d | DesignIntersects  | 200      |
  | FilterArea        | ff91dd40-1569-4765-a2bc-014321f76ace | 3ef41e3c-d1f5-40cd-b012-99d11ff432ef | BoundaryMDPFilter | 200      |
  | TemperatureFilter | ff91dd40-1569-4765-a2bc-014321f76ace | 1980fc8b-c892-4f9f-b673-bc09827bf2b5 | TemperatureFilter | 200      |
  | PassCountFilter   | ff91dd40-1569-4765-a2bc-014321f76ace | c5590172-a1bb-440a-bc7d-6c35ecc75724 | PassCountFilter   | 200      |
