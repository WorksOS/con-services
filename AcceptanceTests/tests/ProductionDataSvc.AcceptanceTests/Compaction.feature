Feature: Compaction
  I should be able to request compaction data


Scenario: Compaction Get CMV Summary 
	Given the Compaction CMV Summary service URI "/api/v2/compaction/cmv/summary"
	And a projectUid "ff91dd40-1569-4765-a2bc-014321f76ace"
	When I request CMV summary
	Then the CMV result should be
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
    "maxCMVPercent": 120
    },
    "Code": 0,
    "Message": "success"
  }
	"""

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
      "maxMDPPercent": 120
    },
    "Code": 0,
    "Message": "success"
  }
	"""

  Scenario: Compaction Get Passcount Summary 
	Given the Compaction Passcount Summary service URI "/api/v2/compaction/passcounts/summary"
	And a projectUid "ff91dd40-1569-4765-a2bc-014321f76ace"
	When I request Passcount summary
	Then the Passcount result should be
  """
  {
    "passCountSummaryData": {
      "percentEqualsTarget": 3.3255301569815479,
      "percentGreaterThanTarget": 45.311209033324154,
      "percentLessThanTarget": 51.3632608096943,
      "totalAreaCoveredSqMeters": 1678.9744000000003,
      "minTarget": 4,
      "maxTarget": 7
    },
    "Code": 0,
    "Message": "success"
  }
	"""

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

  Scenario: Compaction Get Speed Summary 
	Given the Compaction Speed Summary service URI "/api/v2/compaction/speed/summary"
	And a projectUid "ff91dd40-1569-4765-a2bc-014321f76ace"
	When I request Speed summary
	Then the Speed result should be
  """
  {
    "speedSummaryData": {
      "percentEqualsTarget": 0.088493588445472046,
      "percentGreaterThanTarget": 0.20186678525980817,
      "percentLessThanTarget": 0.64978058802207017,
      "totalAreaCoveredSqMeters": 10636.7028,
      "minTarget": 11.988,
      "maxTarget": 15.012
    },
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
    "cmvChangeData": [
      {
        "percentRange": [
          0,
          5
        ],
        "percentValue": 4.9875827814569531
      },
      {
        "percentRange": [
          5,
          20
        ],
        "percentValue": 12.603476821192054
      },
      {
        "percentRange": [
          20,
          50
        ],
        "percentValue": 20.695364238410598
      },
      {
        "percentRange": [
          50,
          100
        ],
        "percentValue": 61.7135761589404
      }
    ],
    "Code": 0,
    "Message": "success"
  }
	"""

  Scenario: Compaction Get Elevation Range 
	Given the Compaction Elevation Range service URI "/api/v2/compaction/elevationrange"
	And a projectUid "ff91dd40-1569-4765-a2bc-014321f76ace"
	When I request Elevation Range
	Then the Elevation Range result should be
  """
  {
    "boundingExtents": {
      "maxX": -115.01824137935459,
      "maxY": 36.2077397408003,
      "maxZ": 1e+308,
      "minX": -115.02513694938636,
      "minY": 36.206563325785218,
      "minZ": 1e+308
    },
    "minElevation": 591.9539794921875,
    "maxElevation": 624.0980224609375,
    "totalCoverageArea": 10637.396400000001,
    "Code": 0,
    "Message": "success"
  }
	"""

  Scenario: Compaction Get Project Statistics 
	Given the Compaction Project Statistics service URI "/api/v2/compaction/projectstatistics"
	And a projectUid "ff91dd40-1569-4765-a2bc-014321f76ace"
	When I request Project Statistics
	Then the Project Statistics result should be
  """
  {
    "startTime": "2012-10-30T00:12:09.109",
    "endTime": "2012-11-08T01:00:08.756",
    "cellSize": 0.34,
    "indexOriginOffset": 536870912,
    "extents": {
      "maxX": 2913.2900000000004,
      "maxY": 1250.69,
      "maxZ": 624.1365966796875,
      "minX": 2306.05,
      "minY": 1125.2300000000002,
      "minZ": 591.953857421875
    },
    "Code": 0,
    "Message": "success"
  }
	"""

  Scenario: Compaction Get Tiles 
	Given the Compaction Tiles service URI "/api/v2/compaction/tiles"
	And a projectUid "ff91dd40-1569-4765-a2bc-014321f76ace" 
  And a displayMode "0" and a bbox "0.631930733951056, -2.007483884109430, 0.631930594324716, -2.007483709576500" and a width "256" and a height "256"
	When I request a Tile
	Then the Tile result should be
  """
  {
    "tileData": "iVBORw0KGgoAAAANSUhEUgAAAQAAAAEACAIAAADTED8xAAAABnRSTlMA/wD/AP83WBt9AAACm0lEQVR42u3TQQEAMAgAIe1f0fdyLMhBB/beQNYKQJkApAlAmgCkCUCaAKQJQJoApAlAmgCkCUCaAKQJQJoApAlAmgCkCUCaAKQJQJoApAlAmgCkCUCaAKQJQJoApAlAmgCkCUCaAKQJQJoApAlAmgCkCUCaAKQJQJoApAlAmgCkCUCaAKQJQJoApAlAmgCkCUCaAKQJQJoApAlAmgCkCUCaAKQJQJoApAlAmgCkCUCaAKQJQJoApAlAmgCkCUCaAKQJQJoApAlAmgCkCUCaAKQJQJoApAlAmgCkCUCaAKQJQJoApAlAmgCkCUCaAKQJQJoApAlAmgCkCUCaAKQJQJoApAlAmgCkCUCaAKQJQJoApAlAmgCkCUCaAKQJQJoApAlAmgCkCUCaAKQJQJoApAlAmgCkCUCaAKQJQJoApAlAmgCkCUCaAKQJQJoApAlAmgCkCUCaAKQJQJoApAlAmgCkCUCaAKQJQJoApAlAmgCkCUCaAKQJQJoApAlAmgCkCUCaAKQJQJoApAlAmgCkCUCaAKQJQJoApAlAmgCkCUCaAKQJQJoApAlAmgCkCUCaAKQJQJoApAlAmgCkCUCaAKQJQJoApAlAmgCkCUCaAKQJQJoApAlAmgCkCUCaAKQJQJoApAlAmgCkCUCaAKQJQJoApAlAmgCkCUCaAKQJQJoApAlAmgCkCUCaAKQJQJoApAlAmgCkCUCaAKQJQJoApAlAmgCkCUCaAKQJQJoApAlAmgCkCUCaAKQJQJoApAlAmgCkCUCaAKQJQJoApAlAmgCkCUCaAKQJQJoApAlAmgCkCUCaAKQJQJoApAlAmgCkCUCaAKQJQJoApAlAmgCkCUCaAKQJQJoApAlAmgCkCUCaAKQJQJoApAlA2gcVQcMQ9LN5NwAAAABJRU5ErkJggg==",
    "tileOutsideProjectExtents": false,
    "Code": 0,
    "Message": "success"
  }
	"""



 
