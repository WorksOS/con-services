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
| GroundToGroundEarliestToLatest   | 7925f179-013d-4aaf-aff4-7b9833bb06d6 |                                      | f4e9b4dd-e8c4-4edb-b9aa-59a209c17de7 | 7730ea54-6c6f-4450-ae94-1933471d7961 | GroundToGroundEarliestToLatest   | 200      |
#| GroundToGroundLatestToEarliest   | 7925f179-013d-4aaf-aff4-7b9833bb06d6 |                                      | 7730ea54-6c6f-4450-ae94-1933471d7961 | f4e9b4dd-e8c4-4edb-b9aa-59a209c17de7 | GroundToGroundLatestToEarliest   | 200      |
| SummaryVolumesFilterNull20121101 | ff91dd40-1569-4765-a2bc-014321f76ace | e2c7381d-1a2e-4dc7-8c0e-45df2f92ba0e | dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff |                                      | SummaryVolumesFilterNull20121101 | 200      |
| GroundToGroundNoData             | 7925f179-013d-4aaf-aff4-7b9833bb06d6 |                                      | ce4497d9-76d0-4477-aa23-2ee1acd8c4f0 | fe6065a7-21fe-4db0-8f47-3ea6c320dac7 | NoDataResponse                   | 200      |
| CustomBulkingAndShrinkage        | 3335311a-f0e2-4dbe-8acd-f21135bafee4 |                                      | 98f03939-e559-442b-b376-4dd25f86349e | 98f03939-e559-442b-b376-4dd25f86349e | CustomBulkingAndShrinkage        | 200      |
| DesignToLatestGround             | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | 7730ea54-6c6f-4450-ae94-1933471d7961 | 3d255208-8aa2-4172-9046-f97a36eff896 |                                      | DesignToLatestGround             | 200      |
| EarliestGroundToDesign           | 7925f179-013d-4aaf-aff4-7b9833bb06d6 |                                      | f4e9b4dd-e8c4-4edb-b9aa-59a209c17de7 | 3d255208-8aa2-4172-9046-f97a36eff896 | LatestGroundToLatestGround       | 200      |
| LatestGroundToLatestGround       | 7925f179-013d-4aaf-aff4-7b9833bb06d6 |                                      | 7730ea54-6c6f-4450-ae94-1933471d7961 | 7730ea54-6c6f-4450-ae94-1933471d7961 | LatestGroundToLatestGround       | 200      |
| GroundToGroundNullUid            | ff91dd40-1569-4765-a2bc-014321f76ace |                                      |                                      |                                      | FilterAndInvalidDesign           | 400      |
| FilterAndInvalidDesign           | ff91dd40-1569-4765-a2bc-014321f76ace | 00000000-0000-0000-0000-000000000000 | f07ed071-f8a1-42c3-804a-1bde7a78be5b |                                      | FilterAndInvalidDesign           | 400      |
| InvalidFilterAndDesign           | ff91dd40-1569-4765-a2bc-014321f76ace | 12e86a90-b301-446e-8e37-7879f1d8fd39 | 00000000-0000-0000-0000-000000000000 |                                      | FilterAndInvalidDesign           | 400      |
| PassCountRangeEarliestToLatest   | 7925f179-013d-4aaf-aff4-7b9833bb06d6 |                                      | 3507b523-9390-4e11-90e9-7a1263bb5cd9 | 3f91916b-7cfc-4c98-9e68-0e5307ffaba5 | PassCountRangeEarliestToLatest   | 200      |
| PassCountRangeDesignToLatest     | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | 3f91916b-7cfc-4c98-9e68-0e5307ffaba5 | 3d255208-8aa2-4172-9046-f97a36eff896 |                                      | PassCountRangeDesignToLatest     | 200      |
| PassCountRangeEarliestToDesign   | 7925f179-013d-4aaf-aff4-7b9833bb06d6 |                                      | 3507b523-9390-4e11-90e9-7a1263bb5cd9 | 3d255208-8aa2-4172-9046-f97a36eff896 | PassCountRangeEarliestToDesign   | 200      |



