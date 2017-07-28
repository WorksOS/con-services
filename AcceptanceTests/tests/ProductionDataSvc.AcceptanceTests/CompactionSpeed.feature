Feature: CompactionSpeed
	 I should be able to request compaction speed data

 Scenario: Compaction Get Speed Summary 
	Given the Compaction Speed Summary service URI "/api/v2/compaction/speed/summary"
	And a projectUid "ff91dd40-1569-4765-a2bc-014321f76ace"
	When I request Speed summary
	Then the Speed result should be
  """
  {
    "speedSummaryData": {
      "percentEqualsTarget": 36.9,
      "percentGreaterThanTarget": 39.3,
      "percentLessThanTarget": 23.8,
      "totalAreaCoveredSqMeters": 10636.7028,
      "minTarget": 5.0,
      "maxTarget": 10.0
    },
    "Code": 0,
    "Message": "success"
  }
	"""

 