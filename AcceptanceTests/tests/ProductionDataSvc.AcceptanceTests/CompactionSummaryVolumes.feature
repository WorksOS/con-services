Feature: CompactionSummaryVolumes
I should be able to request Summary Volumes.

#Background: 
#Given the Summary Volumes service URI "/api/v2/volumes/summary", request repo "CompactionSummaryVolumeRequest.json" and result repo "CompactionSummaryVolumeResponse.json"
#
#Scenario Outline: SummaryVolumes - Good Request
#And the result file "CompactionSummaryVolumeResponse.json"
#When I request Summary Volumes supplying "<ParameterName>" paramters from the repository
#Then the response should match "<ResultName>" result from the repository
#Examples: 
#| RequestName       | ProjectUid                           | VolumeCalcType | BaseFilterUid                        | TopFilterUid                         | BaseDesignUid                                           | TopDesignUid                           |
#| SuccessNoDesigns  | ff91dd40-1569-4765-a2bc-014321f76ace | 4              | F07ED071-F8A1-42C3-804A-1BDE7A78BE5B | A40814AA-9CDB-4981-9A21-96EA30FFECDD |                                                         |                                        |


Scenario Outline: Compaction Get Summary volumes
Given the Compaction service URI "/api/v2/compaction/volumes/summary"
And the result file "CompactionSummaryVolumeResponse.json"
And projectUid "<ProjectUID>"
When I request result
Then the result should match the "<ResultName>" from the repository
Examples: 
| RequestName         | ProjectUID                           | ResultName          |
| SimpleVolumeSummary | ff91dd40-1569-4765-a2bc-014321f76ace | SimpleVolumeSummary |


#Scenario Outline: Compaction Get Summary volumes with filters
#Given the Compaction service URI "/api/v2/compaction/volumes/summary"
#And the result file "CompactionGetVolumesSummaryDataResponse.json"
#And projectUid "<ProjectUID>"
#And filterUid "<FilterUID>"
#When I request result
#Then the result should match the "<ResultName>" from the repository
#Examples: 
#| RequestName      | ProjectUID                           | FilterUID                            | ResultName        |
#| DesignOutside    | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | 3d9086f2-3c04-4d92-9141-5134932b1523 | DesignOutside     |
#| DesignIntersects | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | 81422acc-9b0c-401c-9987-0aedbf153f1d | DesignIntersects  |
#| FilterArea       | ff91dd40-1569-4765-a2bc-014321f76ace | 3ef41e3c-d1f5-40cd-b012-99d11ff432ef | BoundaryMDPFilter |