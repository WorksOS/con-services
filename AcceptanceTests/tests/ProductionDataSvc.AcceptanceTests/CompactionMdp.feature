Feature: CompactionMdp
	 I should be able to request compaction MDP data

 Scenario: Compaction Get MDP Summary 
	Given the Compaction MDP Summary service URI "/api/v2/compaction/mdp/summary"
	And a projectUid "ff91dd40-1569-4765-a2bc-014321f76ace"
	When I request MDP summary
	Then the MDP result should be
  """
  {
    "mdpSummaryData": {
      "percentEqualsTarget": 79.3533176153587,
      "percentGreaterThanTarget": 0,
      "percentLessThanTarget": 20.646682384641295,
      "totalAreaCoveredSqMeters": 1029.6492000000003,
      "mdpTarget": {
        "mdpMachineTarget": 150.0,
        "targetVaries": false
      },
      "minMDPPercent": 80,
      "maxMDPPercent": 130
    },
    "Code": 0,
    "Message": "success"
  }
	"""