Feature: CompactionMdp
	I should be able to request compaction MDP data

######################################################## MDP Summary ############################################################
Scenario Outline: Compaction Get MDP Summary - No Design Filter
Given the Compaction service URI "/api/v2/mdp/summary"
And the result file "CompactionGetMDPDataResponse.json"
And projectUid "<ProjectUID>"
When I request result
Then the result should match the "<ResultName>" from the repository
Examples: 
| RequestName     | ProjectUID                           | ResultName        |
|                 | ff91dd40-1569-4765-a2bc-014321f76ace | NoDesignFilter    |
| ProjectSettings | 3335311a-f0e2-4dbe-8acd-f21135bafee4 | NoDesignFilter_PS |


Scenario Outline: Compaction Get MDP Summary
Given the Compaction service URI "/api/v2/mdp/summary"
And the result file "CompactionGetMDPDataResponse.json"
And projectUid "<ProjectUID>"
And filterUid "<FilterUID>"
When I request result
Then the result should match the "<ResultName>" from the repository
Examples: 
| RequestName       | ProjectUID                           | FilterUID                            | ResultName        |
| DesignOutside     | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | 3d9086f2-3c04-4d92-9141-5134932b1523 | DesignOutside     |
| DesignIntersects  | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | 81422acc-9b0c-401c-9987-0aedbf153f1d | DesignIntersects  |
| FilterArea        | ff91dd40-1569-4765-a2bc-014321f76ace | 3ef41e3c-d1f5-40cd-b012-99d11ff432ef | BoundaryMDPFilter |
| TemperatureFilter | ff91dd40-1569-4765-a2bc-014321f76ace | 1980fc8b-c892-4f9f-b673-bc09827bf2b5 | TemperatureFilter |
#| PassCountFilter   | ff91dd40-1569-4765-a2bc-014321f76ace | c5590172-a1bb-440a-bc7d-6c35ecc75724 | PassCountFilter   |

