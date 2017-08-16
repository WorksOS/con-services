Feature: CompactionMdp
	 I should be able to request compaction MDP data

######################################################## MDP Summary ############################################################
Scenario Outline: Compaction Get MDP Summary - No Design Filter
	Given the Compaction service URI "/api/v2/compaction/mdp/summary"
  And the result file "CompactionGetMDPDataResponse.json"
	And projectUid "<ProjectUID>"
	When I request result
  Then the result should match the "<ResultName>" from the repository
	Examples: 
	| RequetsName     | ProjectUID                           | ResultName        |
	|                 | ff91dd40-1569-4765-a2bc-014321f76ace | NoDesignFilter    |
  | ProjectSettings | 3335311a-f0e2-4dbe-8acd-f21135bafee4 | NoDesignFilter_PS |


Scenario Outline: Compaction Get MDP Summary
  Given the Compaction service URI "/api/v2/compaction/mdp/summary"
  And the result file "CompactionGetMDPDataResponse.json"
  And projectUid "<ProjectUID>"
	And filterUid "<FilterUID>"
	When I request result
	Then the result should match the "<ResultName>" from the repository
	Examples: 
#	| RequetsName      | ProjectUID                           | DesignUID                            | ResultName       |
#	| DesignOutside    | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | 220e12e5-ce92-4645-8f01-1942a2d5a57f | DesignOutside    |
# | DesignIntersepts | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff | DesignIntersepts |
	| RequetsName      | ProjectUID                           | FilterUID                            | ResultName       |
	| DesignOutside    | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | 3d9086f2-3c04-4d92-9141-5134932b1523 | DesignOutside    |
  | DesignIntersepts | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | 81422acc-9b0c-401c-9987-0aedbf153f1d | DesignIntersepts |
