Feature: CompactionSpeed
	 I should be able to request compaction speed data

######################################################## Speed Summary ##########################################################
Scenario Outline: Compaction Get Speed Summary - No Design Filter
	Given the Compaction service URI "/api/v2/compaction/speed/summary" for operation "SpeedSummary"
  And the result file "CompactionGetSpeedDataResponse.json"
	And projectUid "<ProjectUID>"
	When I request result
  Then the result should match the "<ResultName>" from the repository
	Examples: 
	| RequetsName     | ProjectUID                           | ResultName                |
	|                 | ff91dd40-1569-4765-a2bc-014321f76ace | NoDesignFilter_Summary    |
  | ProjectSettings | 3335311a-f0e2-4dbe-8acd-f21135bafee4 | NoDesignFilter_Summary_PS |
 