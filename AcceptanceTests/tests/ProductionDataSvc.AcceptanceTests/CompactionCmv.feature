Feature: CompactionCmv
I should be able to request compaction CMV data

######################################################## CMV Summary ############################################################
Scenario Outline: Compaction Get CMV Summary - No Design Filter
	Given the Compaction service URI "/api/v2/compaction/cmv/summary" for operation "CMVSummary"
  And the result file "CompactionGetCMVDataResponse.json"
	And projectUid "<ProjectUID>"
	When I request result
  Then the result should match the "<ResultName>" from the repository
	Examples: 
	| RequetsName     | ProjectUID                           | ResultName                |
	|                 | ff91dd40-1569-4765-a2bc-014321f76ace | NoDesignFilter_Summary    |
  | ProjectSettings | 3335311a-f0e2-4dbe-8acd-f21135bafee4 | NoDesignFilter_Summary_PS |

######################################################## CMV Details ############################################################
Scenario Outline: Compaction Get CMV Details - No Design Filter
	Given the Compaction service URI "/api/v2/compaction/cmv/details" for operation "CMVDetails"
  And the result file "CompactionGetCMVDataResponse.json"
	And projectUid "<ProjectUID>"
	When I request result
  Then the result should match the "<ResultName>" from the repository
	Examples: 
	| RequetsName     | ProjectUID                           | ResultName                |
	|                 | ff91dd40-1569-4765-a2bc-014321f76ace | NoDesignFilter_Details    |
  | ProjectSettings | 3335311a-f0e2-4dbe-8acd-f21135bafee4 | NoDesignFilter_Details_PS |

######################################################## CMV % Change Summary ###################################################
Scenario Outline: Compaction Get CMV % Change Summary - No Design Filter
	Given the Compaction service URI "/api/v2/compaction/cmv/percentchange" for operation "CMVPercentChangeSummary"
  And the result file "CompactionGetCMVDataResponse.json"
	And projectUid "<ProjectUID>"
	When I request result
  Then the result should match the "<ResultName>" from the repository
	Examples: 
	| RequetsName     | ProjectUID                           | ResultName                      |
	|                 | ff91dd40-1569-4765-a2bc-014321f76ace | NoDesignFilter_PercentChange    |
  | ProjectSettings | 3335311a-f0e2-4dbe-8acd-f21135bafee4 | NoDesignFilter_PercentChange_PS |
