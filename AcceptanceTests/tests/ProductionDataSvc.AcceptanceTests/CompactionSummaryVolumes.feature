Feature: CompactionSummaryVolumes
I should be able to request Summary Volumes.

# Until we can mock execution dates the responses will not contain any volume data and are largely symbolic.
Scenario Outline: Compaction Get Summary volumes
Given the Compaction service URI "/api/v2/volumes/summary"
And the result file "CompactionSummaryVolumeResponse.json"
And project "<ProjectUid>"
And filter "<FilterUid>"
And design "<DesignUid>" 
And to filter "<FilterUid2>"
When I request result "<httpCode>"
Then the result should match the "<ResultName>" from the repository
Examples: 
| RequestName                      | ProjectUid                           | DesignUid                            | FilterUid                            | FilterUid2                           | ResultName                       | httpCode |
| SimpleVolumeSummary              | ff91dd40-1569-4765-a2bc-014321f76ace |                                      | f07ed071-f8a1-42c3-804a-1bde7a78be5b | f07ed071-f8a1-42c3-804a-1bde7a78be5b | SimpleVolumeSummary              | 200      |
| GroundToGround                   | ff91dd40-1569-4765-a2bc-014321f76ace |                                      | a37f3008-65e5-44a8-b406-9a078ec62ece | a37f3008-65e5-44a8-b406-9a078ec62ece | GroundToGround                   | 200      |
| SummaryVolumesFilterNull20121101 | ff91dd40-1569-4765-a2bc-014321f76ace | e2c7381d-1a2e-4dc7-8c0e-45df2f92ba0e | dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff |                                      | SummaryVolumesFilterNull20121101 | 200      |
| GroundToGroundNoData             | ff91dd40-1569-4765-a2bc-014321f76ace |                                      | a54e5945-1aaa-4921-9cc1-c9d8c0a343d3 | a54e5945-1aaa-4921-9cc1-c9d8c0a343d3 | EmptyJsonResponse                | 200      |
| GroundToGroundNullUid            | ff91dd40-1569-4765-a2bc-014321f76ace |                                      |                                      |                                      | FilterAndInvalidDesign           | 400      |
| FilterAndDesign                  | ff91dd40-1569-4765-a2bc-014321f76ace | ea89be4b-0efb-4b8f-ba33-03f0973bfc7b | f07ed071-f8a1-42c3-804a-1bde7a78be5b |                                      | FilterAndDesign                  | 200      |
| FilterAndInvalidDesign           | ff91dd40-1569-4765-a2bc-014321f76ace | 00000000-0000-0000-0000-000000000000 | f07ed071-f8a1-42c3-804a-1bde7a78be5b |                                      | FilterAndInvalidDesign           | 400      |
| InvalidFilterAndDesign           | ff91dd40-1569-4765-a2bc-014321f76ace | 12e86a90-b301-446e-8e37-7879f1d8fd39 | 00000000-0000-0000-0000-000000000000 |                                      | FilterAndInvalidDesign           | 400      |
| CustomBulkingAndShrinkage        | 3335311a-f0e2-4dbe-8acd-f21135bafee4 |                                      | A54E5945-1AAA-4921-9CC1-C9D8C0A343D3 | A54E5945-1AAA-4921-9CC1-C9D8C0A343D3 | CustomBulkingAndShrinkage        | 200      |
| DesignToDesign                   | ff91dd40-1569-4765-a2bc-014321f76ace |                                      | ea89be4b-0efb-4b8f-ba33-03f0973bfc7b | ea89be4b-0efb-4b8f-ba33-03f0973bfc7b | EmptyJsonResponse                | 200      |
| DesignToGround                   | ff91dd40-1569-4765-a2bc-014321f76ace | ea89be4b-0efb-4b8f-ba33-03f0973bfc7b | a54e5945-1aaa-4921-9cc1-c9d8c0a343d3 |                                      | EmptyJsonResponse                | 200      |