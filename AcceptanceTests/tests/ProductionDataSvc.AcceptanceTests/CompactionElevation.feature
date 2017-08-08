Feature: CompactionElevation
  I should be able to request compaction elevation


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

  Scenario: Compaction Get Elevation Range With No Data 
	Given the Compaction Elevation Range service URI "/api/v2/compaction/elevationrange"
	And a projectUid "ff91dd40-1569-4765-a2bc-014321f76ace"
	And a startUtc "2017-01-01" and an EndUtc "2017-01-01"
	When I request Elevation Range
	Then the Elevation Range result should be
  """
  {
    "boundingExtents": null,
    "minElevation": 0,
    "maxElevation": 0,
    "totalCoverageArea": 0,
    "Code": -4,
    "Message": "No elevation range"
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