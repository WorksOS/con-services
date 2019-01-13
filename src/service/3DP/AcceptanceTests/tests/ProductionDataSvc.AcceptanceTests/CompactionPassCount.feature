Feature: CompactionPassCount
I should be able to request Pass Count compaction data

######################################################## Pass Count Summary #####################################################
Scenario Outline: Compaction Get Passcount Summary - No Design Filter
  Given the service route "/api/v2/passcounts/summary" and result repo "CompactionGetPassCountDataResponse.json"
  And with parameter "projectUid" with value "<ProjectUID>"
  When I send the GET request I expect response code <HttpCode>
  Then the response should match to 2 decimal places "<ResultName>" from the repository
  Examples: 
  | ProjectUID                           | ResultName                | HttpCode |
  | ff91dd40-1569-4765-a2bc-014321f76ace | NoDesignFilter_Summary    | 200      |
  | 3335311a-f0e2-4dbe-8acd-f21135bafee4 | NoDesignFilter_Summary_PS | 200      |

Scenario Outline: Compaction Get Passcount Summary
  Given the service route "/api/v2/passcounts/summary" and result repo "CompactionGetPassCountDataResponse.json"
  And with parameter "projectUid" with value "<ProjectUID>"
  And with parameter "filterUid" with value "<FilterUID>"
  When I send the GET request I expect response code <HttpCode>
  Then the response should match to 2 decimal places "<ResultName>" from the repository
  Examples: 
  | ProjectUID                           | FilterUID                            | ResultName                | HttpCode |
  | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | 1cf81668-1739-42d5-b068-ea025588796a | DesignOutside_Summary     | 200      |
  | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | 3d9086f2-3c04-4d92-9141-5134932b1523 | DesignIntersects_Summary  | 200      |
  | ff91dd40-1569-4765-a2bc-014321f76ace | a37f3008-65e5-44a8-b406-9a078ec62ece | BoundaryFilter_Summary    | 200      |
  | ff91dd40-1569-4765-a2bc-014321f76ace | c638018c-5026-44be-af0b-006ecad65462 | BoundaryFilter_Summary    | 200      |
  | ff91dd40-1569-4765-a2bc-014321f76ace | a8405aca-71f1-463d-8821-c2415d67e78c | AsAtCustom_Summary        | 200      |
  | ff91dd40-1569-4765-a2bc-014321f76ace | 2811c7c3-d270-4d63-97e2-fc3340bf6c7a | AlignmentFilter_Summary   | 200      |
  | ff91dd40-1569-4765-a2bc-014321f76ace | 1980fc8b-c892-4f9f-b673-bc09827bf2b5 | TemperatureFilter_Summary | 200      |
  | ff91dd40-1569-4765-a2bc-014321f76ace | c5590172-a1bb-440a-bc7d-6c35ecc75724 | PassCountFilter_Summary   | 200      |


######################################################## Pass Count Details #####################################################
Scenario Outline: Compaction Get Passcount Details - No Design Filter
  Given the service route "/api/v2/passcounts/details" and result repo "CompactionGetPassCountDataResponse.json"
  And with parameter "projectUid" with value "<ProjectUID>"
  When I send the GET request I expect response code <HttpCode>
  Then the response should match to 2 decimal places "<ResultName>" from the repository
  Examples: 
  | ProjectUID                           | ResultName                | HttpCode |
  | ff91dd40-1569-4765-a2bc-014321f76ace | NoDesignFilter_Details    | 200      |
  | 3335311a-f0e2-4dbe-8acd-f21135bafee4 | NoDesignFilter_Details_PS | 200      |

Scenario Outline: Compaction Get Passcount Details
  Given the service route "/api/v2/passcounts/details" and result repo "CompactionGetPassCountDataResponse.json"
  And with parameter "projectUid" with value "<ProjectUID>"
  And with parameter "filterUid" with value "<FilterUID>"
  When I send the GET request I expect response code <HttpCode>
  Then the response should match to 2 decimal places "<ResultName>" from the repository
  Examples: 
  | ProjectUID                           | FilterUID                            | ResultName                     | HttpCode |
  | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | 1cf81668-1739-42d5-b068-ea025588796a | DesignOutside_Details          | 200      |
  | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | 3d9086f2-3c04-4d92-9141-5134932b1523 | DesignIntersects_Details       | 200      |
  | ff91dd40-1569-4765-a2bc-014321f76ace | 154470b6-15ae-4cca-b281-eae8ac1efa6c | BoundaryFilterPCDetails        | 200      |
  | ff91dd40-1569-4765-a2bc-014321f76ace | 3c836562-bcd5-4a35-99a5-cb5655572be7 | BoundaryFilterPCDetails        | 200      |
  | ff91dd40-1569-4765-a2bc-014321f76ace | a8405aca-71f1-463d-8821-c2415d67e78c | AsAtCustomPCDetails            | 200      |
  | ff91dd40-1569-4765-a2bc-014321f76ace | 9c27697f-ea6d-478a-a168-ed20d6cd9a20 | BoundaryMachineFilterPCDetails | 200      |
  | ff91dd40-1569-4765-a2bc-014321f76ace | 2811c7c3-d270-4d63-97e2-fc3340bf6c6b | AlignmentFilter_Details        | 200      |
  | ff91dd40-1569-4765-a2bc-014321f76ace | 1980fc8b-c892-4f9f-b673-bc09827bf2b5 | TemperatureFilter_Details      | 200      |
  | ff91dd40-1569-4765-a2bc-014321f76ace | 3c0b76b6-8e35-4729-ab83-f976732d999b | TempBoundaryFilter_Details     | 200      |
  | ff91dd40-1569-4765-a2bc-014321f76ace | c5590172-a1bb-440a-bc7d-6c35ecc75724 | PassCountFilter_Details        | 200      |
  | ff91dd40-1569-4765-a2bc-014321f76ace | 887f90a6-56b9-4266-9d62-ff99e7d346f0 | AutomaticsFilter_Details       | 200      |
