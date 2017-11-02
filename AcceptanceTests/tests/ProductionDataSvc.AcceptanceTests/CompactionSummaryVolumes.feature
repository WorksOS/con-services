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
| RequestName         | ProjectUid                           | DesignUid                            | volumeCalcType | FilterUid                            | FilterUid2                           | ResultName          |
| SimpleVolumeSummary | ff91dd40-1569-4765-a2bc-014321f76ace |                                      | 4              | F07ED071-F8A1-42C3-804A-1BDE7A78BE5B | F07ED071-F8A1-42C3-804A-1BDE7A78BE5B | SimpleVolumeSummary |
| GroundToGround      | ff91dd40-1569-4765-a2bc-014321f76ace |                                      | 4              | a37f3008-65e5-44a8-b406-9a078ec62ece | a37f3008-65e5-44a8-b406-9a078ec62ece | GroundToGround      |
| FilterAndDesign     | ff91dd40-1569-4765-a2bc-014321f76ace | dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff | 5              | 9c27697f-ea6d-478a-a168-ed20d6cd9a20 |                                      | FilterAndDesign     |
