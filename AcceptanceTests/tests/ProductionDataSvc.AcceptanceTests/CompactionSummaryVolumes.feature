Feature: CompactionSummaryVolumes
I should be able to request Summary Volumes.

Scenario Outline: Compaction Get Summary volumes
Given the Compaction service URI "/api/v2/compaction/volumes/summary"
And the result file "CompactionSummaryVolumeResponse.json"
And project "<ProjectUid>"
And filter "<FilterUid>"
And design "<DesignUid>" 
And to filter "<FilterUid2>"
When I request result "<httpCode>"
Then the result should match the "<ResultName>" from the repository
Examples: 
| RequestName                     | ProjectUid                           | DesignUid                            | FilterUid                            | FilterUid2                           | ResultName                      | httpCode |
| SimpleVolumeSummary             | ff91dd40-1569-4765-a2bc-014321f76ace |                                      | F07ED071-F8A1-42C3-804A-1BDE7A78BE5B | F07ED071-F8A1-42C3-804A-1BDE7A78BE5B | SimpleVolumeSummary             | 200      |
| GroundToGround                  | ff91dd40-1569-4765-a2bc-014321f76ace |                                      | a37f3008-65e5-44a8-b406-9a078ec62ece | a37f3008-65e5-44a8-b406-9a078ec62ece | GroundToGround                  | 200      |
| GroundToGroundNoData            | ff91dd40-1569-4765-a2bc-014321f76ace |                                      | A54E5945-1AAA-4921-9CC1-C9D8C0A343D3 | A54E5945-1AAA-4921-9CC1-C9D8C0A343D3 | EmptyJsonResponse               | 200      |
| GroundToGroundNullUid           | ff91dd40-1569-4765-a2bc-014321f76ace |                                      |                                      |                                      | FilterAndInvalidDesign          | 400      |
# (Aaron) Ignored until we can mock execution date. The following works if the runtime date is 20121103.
#| GroundToGroundNoLatLonToday     | ff91dd40-1569-4765-a2bc-014321f76ace |                                      | F9D55290-27F2-4B70-BC63-9FD23218E6E6 | F9D55290-27F2-4B70-BC63-9FD23218E6E6 | GroundToGroundNoLatLonToday     | 200      |
#| GroundToGroundNoLatLonYesterday | ff91dd40-1569-4765-a2bc-014321f76ace |                                      | D6B254A0-C047-4805-9CCD-F847FAB05B14 | D6B254A0-C047-4805-9CCD-F847FAB05B14 | GroundToGroundNoLatLonYesterday | 200      |
| FilterAndDesign                 | ff91dd40-1569-4765-a2bc-014321f76ace | dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff | 9c27697f-ea6d-478a-a168-ed20d6cd9a20 |                                      | FilterAndDesign                 | 200      |
| FilterAndInvalidDesign          | ff91dd40-1569-4765-a2bc-014321f76ace | 00000000-0000-0000-0000-000000000000 | 9c27697f-ea6d-478a-a168-ed20d6cd9a20 |                                      | FilterAndInvalidDesign          | 400      |
| InvalidFilterAndDesign          | ff91dd40-1569-4765-a2bc-014321f76ace | dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff | 00000000-0000-0000-0000-000000000000 |                                      | FilterAndInvalidDesign          | 400      |
| CustomBulkingAndShrinkage       | 3335311a-f0e2-4dbe-8acd-f21135bafee4 |                                      | F07ED071-F8A1-42C3-804A-1BDE7A78BE5B | A40814AA-9CDB-4981-9A21-96EA30FFECDD | CustomBulkingAndShrinkage       | 200      |
| DesignToDesign                  | ff91dd40-1569-4765-a2bc-014321f76ace | dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff | dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff |                                      | EmptyJsonResponse               | 200      |