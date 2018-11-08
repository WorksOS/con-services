Feature: CompactionSpeed
  I should be able to request compaction speed data

######################################################## Speed Summary ##########################################################
Scenario Outline: Compaction Get Speed Summary - No Design Filter
  Given the service route "/api/v2/speed/summary" and result repo "CompactionGetSpeedDataResponse.json"
  And with parameter "projectUid" with value "<ProjectUID>"
  When I send the GET request I expect response code <HttpCode>
  Then the response should match "<ResultName>" from the repository
  Examples: 
  | RequestName     | ProjectUID                           | ResultName                | HttpCode |
  |                 | ff91dd40-1569-4765-a2bc-014321f76ace | NoDesignFilter_Summary    | 200      |
  | ProjectSettings | 3335311a-f0e2-4dbe-8acd-f21135bafee4 | NoDesignFilter_Summary_PS | 200      |
 
Scenario Outline: Compaction Get Speed Summary
  Given the service route "/api/v2/speed/summary" and result repo "CompactionGetSpeedDataResponse.json"
  And with parameter "projectUid" with value "<ProjectUID>"
  And with parameter "filterUid" with value "<FilterUID>"
  When I send the GET request I expect response code <HttpCode>
  Then the response should match "<ResultName>" from the repository
  Examples: 
  | RequestName       | ProjectUID                           | FilterUID                            | ResultName                 | HttpCode |
  | DesignOutside     | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | 1cf81668-1739-42d5-b068-ea025588796a | DesignOutside_Summary      | 200      |
  | DesignIntersects  | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | 3d9086f2-3c04-4d92-9141-5134932b1523 | DesignIntersects_Summary   | 200      |
  | FilterAreaMachine | ff91dd40-1569-4765-a2bc-014321f76ace | 9c27697f-ea6d-478a-a168-ed20d6cd9a20 | BoundaryMachineFilterSpeed | 200      |
  | TemperatureFilter | ff91dd40-1569-4765-a2bc-014321f76ace | 1980fc8b-c892-4f9f-b673-bc09827bf2b5 | TemperatureFilter_Summary  | 200      |
  | PassCountFilter   | ff91dd40-1569-4765-a2bc-014321f76ace | c5590172-a1bb-440a-bc7d-6c35ecc75724 | PassCountFilter_Summary    | 200      |
