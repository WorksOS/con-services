Feature: CompactionTemperature
	 I should be able to request compaction temperature data

 Scenario: Compaction Get Temperature Summary 
	Given the Compaction Temperature Summary service URI "/api/v2/compaction/temperature/summary"
	And a projectUid "ff91dd40-1569-4765-a2bc-014321f76ace"
	When I request Temperature summary
	Then the Temperature result should be
  """
  {
    "temperatureSummaryData": {
      "percentEqualsTarget": 0,
      "percentGreaterThanTarget": 0,
      "percentLessThanTarget": 100,
      "totalAreaCoveredSqMeters": 953.93120000000022,
      "temperatureTarget": {
        "minTemperatureMachineTarget": 90.0,
        "maxTemperatureMachineTarget": 143.0,
        "targetVaries": false
      }
    },
    "Code": 0,
    "Message": "success"
  }
	"""