Feature: Compaction
  I should be able to request compaction data


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
    "maxCMVPercent": 120
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
	      95.47749263777871,
        4.5225073622212877,
        0.0,
        0.0,
        0.0
			],
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
	Then the Passcount summary result should be
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

	Scenario: Compaction Get Passcount Details 
		Given the Compaction Passcount Details service URI "/api/v2/compaction/passcounts/details"
		And a projectUid "ff91dd40-1569-4765-a2bc-014321f76ace"
		When I request Passcount details
		Then the Passcount details result should be
	"""
	{
		"passCountDetailsData": {
			"percents": [
				19.632901900694421,
				8.5406274791075756,
				6.160684206522566,
				5.132635651332877,
				4.1589237005401056,
				3.5981699431639118,
				2.8341972853432442,
				2.0126278268618436,
				47.929232006433452
			],
			"totalCoverageArea": 1678.9744000000003,
			"passCountTarget": {
				"minPassCountMachineTarget": 0,
				"maxPassCountMachineTarget": 0,
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
      "percentEqualsTarget": 9.4,
      "percentGreaterThanTarget": 21.5,
      "percentLessThanTarget": 69.1,
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
  And a displayMode "0" and a bbox "36.206964000089840283, -115.0203540002853231, 36.206956000089640213, -115.02034400028509253" and a width "256" and a height "256"
	When I request a Tile
	Then the Tile result should be
  """
  {
    "TileData": "iVBORw0KGgoAAAANSUhEUgAAAQAAAAEACAIAAADTED8xAAAABnRSTlMA/wD/AP83WBt9AAADi0lEQVR42u3TQU0FARQEQdYNmpCD8Y8FLpPNS1cp6Es/n58vJr7fDuAfHgOsGOACA8wY4AIDzBjgAgPMGOACA8wY4AIDzBjgAgPMGOACA8wY4AIDzBjgAgPMGOACA8wY4AIDzBjgAgPMGOACA8wY4AIDzBjgAgPMGOACA8wY4AIDzBjgAgPMGOACA8wY4AIDzBjgAgPMGOACA8wY4AIDzBjgAgPMGOACA8wY4AIDzBjgAgPMGOACA8wY4AIDzBjgAgPMGOACA8wY4AIDzBjgAgPMGOACA8wY4AIDzBjgAgPMGOACA8wY4AIDzBjgAgPMGOACA8wY4AIDzBjgAgPMGOACA8wY4AIDzBjgAgPMGOACA8wY4AIDzBjgAgPMGOACA8wY4AIDzBjgAgPMGOACA8wY4AIDzBjgAgPMGOACA8wY4AIDzBjgAgPMGOACA8wY4AIDzBjgAgPMGOACA8wY4AIDzBjgAgPMGOACA8wY4AIDzBjgAgPMGOACA8wY4AIDzBjgAgPMGOACA8wY4AIDzBjgAgPMGOACA8wY4AIDzBjgAgPMGOACA8wY4AIDzBjgAgPMGOACA8wY4ILn8/t2ArzHAKQZgDQDkGYA0gxAmgFIMwBpBiDNAKQZgDQDkGYA0gxAmgFIMwBpBiDNAKQZgDQDkGYA0gxAmgFIMwBpBiDNAKQZgDQDkGYA0gxAmgFIMwBpBiDNAKQZgDQDkGYA0gxAmgFIMwBpBiDNAKQZgDQDkGYA0gxAmgFIMwBpBiDNAKQZgDQDkGYA0gxAmgFIMwBpBiDNAKQZgDQDkGYA0gxAmgFIMwBpBiDNAKQZgDQDkGYA0gxAmgFIMwBpBiDNAKQZgDQDkGYA0gxAmgFIMwBpBiDNAKQZgDQDkGYA0gxAmgFIMwBpBiDNAKQZgDQDkGYA0gxAmgFIMwBpBiDNAKQZgDQDkGYA0gxAmgFIMwBpBiDNAKQZgDQDkGYA0gxAmgFIMwBpBiDNAKQZgDQDkGYA0gxAmgFIMwBpBiDNAKQZgDQDkGYA0gxAmgFIMwBpBiDNAKQZgDQDkGYA0gxAmgFIMwBpBiDNAKQZgDQDkGYA0gxAmgFIMwBpBiDNAKQZgDQDkGYA0gxAmgFIMwBpBiDNAKQZgDQDkGYA0gxAmgFIMwBpBiDNAKQZgDQDkGYA0gxAmgFIMwBpBiDNAKQZgDQDkGYA0gxAmgFI+wPL4oIQ8iSaXgAAAABJRU5ErkJggg==",
    "TileOutsideProjectExtents": false,
    "Code": 0,
    "Message": "success"
  }
	"""
   
  Scenario: Compaction Get Palettes 
	Given the Compaction Palettes service URI "/api/v2/compaction/colorpalettes"
  And a projectUid "ff91dd40-1569-4765-a2bc-014321f76ace"
	When I request Palettes
	Then the Palettes result should be
  """
  {
    "elevationPalette": {
      "colorValues": [
        {
          "color": 16711680,
          "value": 591.9539794921875
        },
        {
          "color": 14760960,
          "value": 593.062394766972
        },
        {
          "color": 16734720,
          "value": 594.17081004175645
        },
        {
          "color": 16744960,
          "value": 595.279225316541
        },
        {
          "color": 16755200,
          "value": 596.38764059132541
        },
        {
          "color": 16762880,
          "value": 597.49605586610994
        },
        {
          "color": 16768000,
          "value": 598.60447114089436
        },
        {
          "color": 16442880,
          "value": 599.7128864156789
        },
        {
          "color": 14476800,
          "value": 600.82130169046332
        },
        {
          "color": 13821440,
          "value": 601.92971696524785
        },
        {
          "color": 13166080,
          "value": 603.03813224003238
        },
        {
          "color": 11855360,
          "value": 604.1465475148168
        },
        {
          "color": 9889280,
          "value": 605.25496278960134
        },
        {
          "color": 8578560,
          "value": 606.36337806438576
        },
        {
          "color": 6615040,
          "value": 607.47179333917029
        },
        {
          "color": 65280,
          "value": 608.58020861395471
        },
        {
          "color": 61540,
          "value": 609.68862388873924
        },
        {
          "color": 59010,
          "value": 610.79703916352366
        },
        {
          "color": 59030,
          "value": 611.9054544383082
        },
        {
          "color": 59060,
          "value": 613.01386971309262
        },
        {
          "color": 59080,
          "value": 614.12228498787715
        },
        {
          "color": 59090,
          "value": 615.23070026266168
        },
        {
          "color": 56540,
          "value": 616.3391155374461
        },
        {
          "color": 51430,
          "value": 617.44753081223064
        },
        {
          "color": 46320,
          "value": 618.55594608701506
        },
        {
          "color": 38645,
          "value": 619.66436136179959
        },
        {
          "color": 30970,
          "value": 620.772776636584
        },
        {
          "color": 23295,
          "value": 621.88119191136855
        },
        {
          "color": 18175,
          "value": 622.989607186153
        },
        {
          "color": 255,
          "value": 624.0980224609375
        }
      ],
      "aboveLastColor": 8388736,
      "belowFirstColor": 16711935
    },
    "cmvDetailPalette": {
      "colorValues": [
        {
          "color": 8421504,
          "value": 0.0
        },
        {
          "color": 255,
          "value": 20.0
        },
        {
          "color": 65280,
          "value": 70.0
        },
        {
          "color": 65280,
          "value": 71.0
        },
        {
          "color": 16711680,
          "value": 101.0
        }
      ],
      "aboveLastColor": null,
      "belowFirstColor": null
    },
    "passCountDetailPalette": {
      "colorValues": [
        {
          "color": 16763955,
          "value": 1.0
        },
        {
          "color": 16776960,
          "value": 2.0
        },
        {
          "color": 65535,
          "value": 3.0
        },
        {
          "color": 39423,
          "value": 4.0
        },
        {
          "color": 13434624,
          "value": 5.0
        },
        {
          "color": 16711935,
          "value": 6.0
        },
        {
          "color": 6684825,
          "value": 7.0
        },
        {
          "color": 6697728,
          "value": 8.0
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
          "value": 0.0
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
          "value": 5.0
        },
        {
          "color": 65535,
          "value": 20.0
        },
        {
          "color": 16711680,
          "value": 50.0
        }
      ],
      "aboveLastColor": 16776960,
      "belowFirstColor": 33554431
    },
    "speedSummaryPalette": {
      "aboveTargetColor": 8388736,
      "onTargetColor": 65280,
      "belowTargetColor": 65535
    },
    "temperatureDetailPalette": {
      "colorValues": [
        {
          "color": 2971523,
          "value": 70.0
        },
        {
          "color": 4430812,
          "value": 80.0
        },
        {
          "color": 12509169,
          "value": 90.0
        },
        {
          "color": 14479047,
          "value": 100.0
        },
        {
          "color": 10341991,
          "value": 110.0
        },
        {
          "color": 7053374,
          "value": 120.0
        },
        {
          "color": 3828517,
          "value": 130.0
        },
        {
          "color": 16174803,
          "value": 140.0
        },
        {
          "color": 13990524,
          "value": 150.0
        },
        {
          "color": 12660791,
          "value": 160.0
        }
      ],
      "aboveLastColor": null,
      "belowFirstColor": null
    },
    "Code": 0,
    "Message": "success"
  }
	"""



 
