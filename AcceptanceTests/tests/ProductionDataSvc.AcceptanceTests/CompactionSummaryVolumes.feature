Feature: CompactionSummaryVolumes
I should be able to request Summary Volumes.

Scenario Outline: Compaction Get Summary volumes
Given the Compaction service URI "/api/v2/compaction/volumes/summary"
And the result file "CompactionSummaryVolumeResponse.json"
And project "<ProjectUid>"
And filter "<FilterUid>"
And design "<DesignUid>" 
And to filter "<FilterUid2>"
When I request result
Then the result should match the "<ResultName>" from the repository
Examples: 
| RequestName               | ProjectUid                           | DesignUid                            | FilterUid                            | FilterUid2                           | ResultName                |
| SimpleVolumeSummary       | ff91dd40-1569-4765-a2bc-014321f76ace |                                      | F07ED071-F8A1-42C3-804A-1BDE7A78BE5B | F07ED071-F8A1-42C3-804A-1BDE7A78BE5B | SimpleVolumeSummary       |
| GroundToGround            | ff91dd40-1569-4765-a2bc-014321f76ace |                                      | a37f3008-65e5-44a8-b406-9a078ec62ece | a37f3008-65e5-44a8-b406-9a078ec62ece | GroundToGround            |
| FilterAndDesign           | ff91dd40-1569-4765-a2bc-014321f76ace | dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff | 9c27697f-ea6d-478a-a168-ed20d6cd9a20 |                                      | FilterAndDesign           |
| CustomBulkingAndShrinkage | 3335311a-f0e2-4dbe-8acd-f21135bafee4 |                                      | F07ED071-F8A1-42C3-804A-1BDE7A78BE5B | A40814AA-9CDB-4981-9A21-96EA30FFECDD | CustomBulkingAndShrinkage |
# Ignored until we get some sort of date mocking service added for testing.
#| VolumeSummaryToday        | ff91dd40-1569-4765-a2bc-014321f76ace |                                      | A54E5945-1AAA-4921-9CC1-C9D8C0A343D3 | 3E2A21B2-D66E-44D4-A590-4F4B7C7FBA7B | VolumeSummaryToday        |
#| VolumeSummaryYesterday    | ff91dd40-1569-4765-a2bc-014321f76ace |                                      | A325F48A-3F3D-489A-976A-B4780EF84045 | A5FD6B6F-CA88-42F2-8AD8-F37E0635CF80 | VolumeSummaryYesterday    |