Feature: CompactionSummaryVolumes
I should be able to request Summary Volumes.

Scenario Outline: Compaction Get Summary volumes
Given the Compaction service URI "/api/v2/compaction/volumes/summary"
And the result file "CompactionSummaryVolumeResponse.json"
And projectUid "<ProjectUID>"
And filterUid "<FilterUID>"
And volumeCalcType "<volumeCalcType>" and baseFilterUid "<baseFilterUid>" and topFilterUid "<topFilterUid>"
When I request result
Then the result should match the "<ResultName>" from the repository
Examples: 
| RequestName         | ProjectUID                           | FilterUID | volumeCalcType | baseFilterUid                        | topFilterUid                         | ResultName          |
| SimpleVolumeSummary | ff91dd40-1569-4765-a2bc-014321f76ace |           | 4              | F07ED071-F8A1-42C3-804A-1BDE7A78BE5B | A40814AA-9CDB-4981-9A21-96EA30FFECDD | SimpleVolumeSummary |
