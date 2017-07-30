Feature: CompactionCmv
  I should be able to request compaction CMV data

Scenario: Compaction Get CMV Summary 
	Given the Compaction CMV Summary service URI "/api/v2/compaction/cmv/summary"
	And a projectUid "ff91dd40-1569-4765-a2bc-014321f76ace"
	When I request CMV summary
	Then the CMV summary result should be
  """
	{
    "cmvSummaryData": {
    "percentEqualsTarget": 0.12620950778291964,
    "percentGreaterThanTarget": 0,
    "percentLessThanTarget": 99.873790492217083,
    "totalAreaCoveredSqMeters": 549.56240000000014,
    "cmvTarget": {
      "cmvMachineTarget": 70.0,
      "targetVaries": false
    },
    "minCMVPercent": 80,
    "maxCMVPercent": 130
    },
    "Code": 0,
    "Message": "success"
  }
	"""

	Scenario: Compaction Get CMV Details
		Given the Compaction CMV Details service URI "/api/v2/compaction/cmv/details"
		And a projectUid "ff91dd40-1569-4765-a2bc-014321f76ace"
		When I request CMV details
		Then the CMV details result should be
		"""
		{
			"percents": [
        0.0,
        100.0,
        0.0,
        0.0,
        0.0
      ],
			"Code": 0,
			"Message": "success"
		}
		"""

Scenario: Compaction Get CMV % Change Summary 
	Given the Compaction CMV % Change Summary service URI "/api/v2/compaction/cmv/percentchange"
	And a projectUid "ff91dd40-1569-4765-a2bc-014321f76ace"
	When I request CMV % change
	Then the CMV % Change result should be
  """
  {
    "cmvChangeData": {
      "percents": [
        4.9875827814569531,
        12.603476821192054,
        20.695364238410598,
        61.7135761589404
      ],
      "totalAreaCoveredSqMeters": 558.57920000000013
    },
    "Code": 0,
    "Message": "success"
  }
	"""
