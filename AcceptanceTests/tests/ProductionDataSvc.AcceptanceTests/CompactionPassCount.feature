Feature: CompactionPassCount
I should be able to request Pass Count compaction data

######################################################## Pass Count Summary #####################################################
Scenario Outline: Compaction Get Passcount Summary - No Design Filter
Given the Compaction service URI "/api/v2/passcounts/summary" for operation "PassCountSummary"
And the result file "CompactionGetPassCountDataResponse.json"
And projectUid "<ProjectUID>"
When I request result
Then the result should match the "<ResultName>" from the repository
Examples: 
| RequestName     | ProjectUID                           | ResultName                |
|                 | ff91dd40-1569-4765-a2bc-014321f76ace | NoDesignFilter_Summary    |
| ProjectSettings | 3335311a-f0e2-4dbe-8acd-f21135bafee4 | NoDesignFilter_Summary_PS |

Scenario Outline: Compaction Get Passcount Summary
Given the Compaction service URI "/api/v2/passcounts/summary" for operation "PassCountSummary"
And the result file "CompactionGetPassCountDataResponse.json"
And projectUid "<ProjectUID>"
And filterUid "<FilterUID>"
When I request result
Then the result should match the "<ResultName>" from the repository
Examples: 
| RequestName       | ProjectUID                           | FilterUID                            | ResultName                |
| DesignOutside     | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | 1cf81668-1739-42d5-b068-ea025588796a | DesignOutside_Summary     |
| DesignIntersects  | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | 3d9086f2-3c04-4d92-9141-5134932b1523 | DesignIntersects_Summary  |
| FilterArea        | ff91dd40-1569-4765-a2bc-014321f76ace | a37f3008-65e5-44a8-b406-9a078ec62ece | BoundaryFilter_Summary    |
| AsAtToday         | ff91dd40-1569-4765-a2bc-014321f76ace | c638018c-5026-44be-af0b-006ecad65462 | BoundaryFilter_Summary    |
| AsAtCustom        | ff91dd40-1569-4765-a2bc-014321f76ace | a8405aca-71f1-463d-8821-c2415d67e78c | AsAtCustom_Summary        |
| AlignmentFilter   | ff91dd40-1569-4765-a2bc-014321f76ace | 2811c7c3-d270-4d63-97e2-fc3340bf6c7a | AlignmentFilter_Summary   |
| TemperatureFilter | ff91dd40-1569-4765-a2bc-014321f76ace | 1980fc8b-c892-4f9f-b673-bc09827bf2b5 | TemperatureFilter_Summary |
#| PassCountFilter   | ff91dd40-1569-4765-a2bc-014321f76ace | c5590172-a1bb-440a-bc7d-6c35ecc75724 | PassCountFilter_Summary   |


######################################################## Pass Count Details #####################################################
Scenario Outline: Compaction Get Passcount Details - No Design Filter
Given the Compaction service URI "/api/v2/passcounts/details" for operation "PassCountDetails"
And the result file "CompactionGetPassCountDataResponse.json"
And projectUid "<ProjectUID>"
When I request result
Then the result should match the "<ResultName>" from the repository
Examples: 
| RequestName     | ProjectUID                           | ResultName                |
|                 | ff91dd40-1569-4765-a2bc-014321f76ace | NoDesignFilter_Details    |
| ProjectSettings | 3335311a-f0e2-4dbe-8acd-f21135bafee4 | NoDesignFilter_Details_PS |

Scenario Outline: Compaction Get Passcount Details
Given the Compaction service URI "/api/v2/passcounts/details" for operation "PassCountDetails"
And the result file "CompactionGetPassCountDataResponse.json"
And projectUid "<ProjectUID>"
And filterUid "<FilterUID>"
When I request result
Then the result should match the "<ResultName>" from the repository
Examples: 
| RequestName        | ProjectUID                           | FilterUID                            | ResultName                     |
| DesignOutside      | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | 1cf81668-1739-42d5-b068-ea025588796a | DesignOutside_Details          |
| DesignIntersects   | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | 3d9086f2-3c04-4d92-9141-5134932b1523 | DesignIntersects_Details       |
| FilterArea         | ff91dd40-1569-4765-a2bc-014321f76ace | 154470b6-15ae-4cca-b281-eae8ac1efa6c | BoundaryFilterPCDetails        |
| AsAtToday          | ff91dd40-1569-4765-a2bc-014321f76ace | 3c836562-bcd5-4a35-99a5-cb5655572be7 | BoundaryFilterPCDetails        |
| AsAtCustom         | ff91dd40-1569-4765-a2bc-014321f76ace | a8405aca-71f1-463d-8821-c2415d67e78c | AsAtCustomPCDetails            |
| FilterAreaMachine  | ff91dd40-1569-4765-a2bc-014321f76ace | 9c27697f-ea6d-478a-a168-ed20d6cd9a20 | BoundaryMachineFilterPCDetails |
| AlignmentFilter    | ff91dd40-1569-4765-a2bc-014321f76ace | 2811c7c3-d270-4d63-97e2-fc3340bf6c6b | AlignmentFilter_Details        |
| TemperatureFilter  | ff91dd40-1569-4765-a2bc-014321f76ace | 1980fc8b-c892-4f9f-b673-bc09827bf2b5 | TemperatureFilter_Details      |
| TempBoundaryFilter | ff91dd40-1569-4765-a2bc-014321f76ace | 3c0b76b6-8e35-4729-ab83-f976732d999b | TempBoundaryFilter_Details     |
#| PassCountFilter    | ff91dd40-1569-4765-a2bc-014321f76ace | c5590172-a1bb-440a-bc7d-6c35ecc75724 | PassCountFilter_Details        |
| AutomaticsFilter   | ff91dd40-1569-4765-a2bc-014321f76ace | 887f90a6-56b9-4266-9d62-ff99e7d346f0 | AutomaticsFilter_Details       |



