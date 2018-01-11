Feature: CompactionSpeed
I should be able to request compaction speed data

######################################################## Speed Summary ##########################################################
Scenario Outline: Compaction Get Speed Summary - No Design Filter
Given the Compaction service URI "/api/v2/speed/summary" for operation "SpeedSummary"
And the result file "CompactionGetSpeedDataResponse.json"
And projectUid "<ProjectUID>"
When I request result
Then the result should match the "<ResultName>" from the repository
Examples: 
| RequestName     | ProjectUID                           | ResultName                |
|                 | ff91dd40-1569-4765-a2bc-014321f76ace | NoDesignFilter_Summary    |
| ProjectSettings | 3335311a-f0e2-4dbe-8acd-f21135bafee4 | NoDesignFilter_Summary_PS |
 
Scenario Outline: Compaction Get Speed Summary
Given the Compaction service URI "/api/v2/speed/summary" for operation "SpeedSummary"
And the result file "CompactionGetSpeedDataResponse.json"
And projectUid "<ProjectUID>"
And filterUid "<FilterUID>"
When I request result
Then the result should match the "<ResultName>" from the repository
Examples: 
| RequestName       | ProjectUID                           | FilterUID                            | ResultName                 |
| DesignOutside     | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | 1cf81668-1739-42d5-b068-ea025588796a | DesignOutside_Summary      |
| DesignIntersects  | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | 3d9086f2-3c04-4d92-9141-5134932b1523 | DesignIntersects_Summary   |
| FilterAreaMachine | ff91dd40-1569-4765-a2bc-014321f76ace | 9c27697f-ea6d-478a-a168-ed20d6cd9a20 | BoundaryMachineFilterSpeed |