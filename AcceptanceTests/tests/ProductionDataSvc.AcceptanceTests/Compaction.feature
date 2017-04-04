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
      "passCountTarget": {
        "minPassCountMachineTarget": 5,
        "maxPassCountMachineTarget": 5,
        "targetVaries": true
      }
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
      "minTarget": 12.0,
      "maxTarget": 15.0
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

  Scenario: Compaction Get Palettes 
	Given the Compaction Palettes service URI "/api/v2/compaction/colorpalettes"
	When I request Palettes
	Then the Palettes result should be
  """
  {
    "elevationPalette": {
      "colorValues": [
        {
          "color": 16711680,
          "value": 0
        },
        {
          "color": 14760960,
          "value": 68.965517241379317
        },
        {
          "color": 16734720,
          "value": 137.93103448275863
        },
        {
          "color": 16744960,
          "value": 206.89655172413796
        },
        {
          "color": 16755200,
          "value": 275.86206896551727
        },
        {
          "color": 16762880,
          "value": 344.82758620689657
        },
        {
          "color": 16768000,
          "value": 413.79310344827593
        },
        {
          "color": 16442880,
          "value": 482.75862068965523
        },
        {
          "color": 14476800,
          "value": 551.72413793103453
        },
        {
          "color": 13821440,
          "value": 620.68965517241384
        },
        {
          "color": 13166080,
          "value": 689.65517241379314
        },
        {
          "color": 11855360,
          "value": 758.62068965517244
        },
        {
          "color": 9889280,
          "value": 827.58620689655186
        },
        {
          "color": 8578560,
          "value": 896.55172413793116
        },
        {
          "color": 6615040,
          "value": 965.51724137931046
        },
        {
          "color": 65280,
          "value": 1034.4827586206898
        },
        {
          "color": 61540,
          "value": 1103.4482758620691
        },
        {
          "color": 59010,
          "value": 1172.4137931034484
        },
        {
          "color": 59030,
          "value": 1241.3793103448277
        },
        {
          "color": 59060,
          "value": 1310.344827586207
        },
        {
          "color": 59080,
          "value": 1379.3103448275863
        },
        {
          "color": 59090,
          "value": 1448.2758620689656
        },
        {
          "color": 56540,
          "value": 1517.2413793103449
        },
        {
          "color": 51430,
          "value": 1586.2068965517242
        },
        {
          "color": 46320,
          "value": 1655.1724137931037
        },
        {
          "color": 38645,
          "value": 1724.137931034483
        },
        {
          "color": 30970,
          "value": 1793.1034482758623
        },
        {
          "color": 23295,
          "value": 1862.0689655172416
        },
        {
          "color": 18175,
          "value": 1931.0344827586209
        },
        {
          "color": 255,
          "value": 2000.0000000000002
        }
      ],
      "aboveLastColor": 8388736,
      "belowFirstColor": 16711935
    },
    "cmvDetailPalette": {
      "colorValues": [
        {
          "color": 16711680,
          "value": 0
        },
        {
          "color": 65280,
          "value": 76.5
        },
        {
          "color": 65280,
          "value": 93.500000000000014
        }
      ],
      "aboveLastColor": 255,
      "belowFirstColor": null
    },
    "passCountDetailPalette": {
      "colorValues": [
        {
          "color": 16763955,
          "value": 1
        },
        {
          "color": 16776960,
          "value": 2
        },
        {
          "color": 65535,
          "value": 3
        },
        {
          "color": 39423,
          "value": 4
        },
        {
          "color": 13434624,
          "value": 5
        },
        {
          "color": 16711935,
          "value": 6
        },
        {
          "color": 6684825,
          "value": 7
        },
        {
          "color": 6697728,
          "value": 8
        }
      ],
      "aboveLastColor": 3355392,
      "belowFirstColor": null
    },
    "passCountSummaryPalette": {
      "aboveTargetColor": 16711680,
      "onTargetColor": 65280,
      "belowTargetColor": 255
    },
    "cutFillPalette": {
      "colorValues": [
        {
          "color": 128,
          "value": -0.2
        },
        {
          "color": 255,
          "value": -0.1
        },
        {
          "color": 8421631,
          "value": -0.05
        },
        {
          "color": 65280,
          "value": 0
        },
        {
          "color": 16744576,
          "value": 0.05
        },
        {
          "color": 16711680,
          "value": 0.1
        },
        {
          "color": 8388608,
          "value": 0.2
        }
      ],
      "aboveLastColor": null,
      "belowFirstColor": null
    },
    "temperatureSummaryPalette": {
      "aboveTargetColor": 16711680,
      "onTargetColor": 65280,
      "belowTargetColor": 255
    },
    "cmvSummaryPalette": {
      "aboveTargetColor": 16711680,
      "onTargetColor": 65280,
      "belowTargetColor": 255
    },
    "mdpSummaryPalette": {
      "aboveTargetColor": 16711680,
      "onTargetColor": 65280,
      "belowTargetColor": 255
    },
    "cmvPercentChangePalette": {
      "colorValues": [
        {
          "color": 65280,
          "value": 10
        },
        {
          "color": 16776960,
          "value": 20
        },
        {
          "color": 16744192,
          "value": 40
        },
        {
          "color": 16711935,
          "value": 80
        }
      ],
      "aboveLastColor": 16711680,
      "belowFirstColor": 0
    },
    "speedSummaryPalette": {
      "aboveTargetColor": 8388736,
      "onTargetColor": 65280,
      "belowTargetColor": 65535
    },
    "temperatureDetailPalette": {
      "colorValues": [
        {
          "color": 255,
          "value": 20
        },
        {
          "color": 65535,
          "value": 55
        },
        {
          "color": 32768,
          "value": 75
        }
      ],
      "aboveLastColor": 8388608,
      "belowFirstColor": 16711935
    },
    "Code": 0,
    "Message": "success"
  }
	"""



 
