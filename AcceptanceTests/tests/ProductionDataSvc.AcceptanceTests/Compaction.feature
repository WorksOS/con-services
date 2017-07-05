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
	Given the Compaction Tiles service URI "/api/v2/compaction/productiondatatiles"
	And a projectUid "ff91dd40-1569-4765-a2bc-014321f76ace" 
  And a displayMode "0" and a bbox "36.206964000089840283, -115.0203540002853231, 36.206956000089640213, -115.02034400028509253" and a width "256" and a height "256"
	When I request a Tile
	Then the Tile result should be
  """
  {
    "TileData": "iVBORw0KGgoAAAANSUhEUgAAAQAAAAEACAIAAADTED8xAAAABnRSTlMA/wD/AP83WBt9AAADjklEQVR42u3TQW1DARTEwH4iQVIkZV8kDYVeVtGTZxD44uf3+4uF1+vTBfzDY4ARA5xggBUDnGCAFQOcYIAVA5xggBUDnGCAFQOcYIAVA5xggBUDnGCAFQOcYIAVA5xggBUDnGCAFQOcYIAVA5xggBUDnGCAFQOcYIAVA5xggBUDnGCAFQOcYIAVA5xggBUDnGCAFQOcYIAVA5xggBUDnGCAFQOcYIAVA5xggBUDnGCAFQOcYIAVA5xggBUDnGCAFQOcYIAVA5xggBUDnGCAFQOcYIAVA5xggBUDnGCAFQOcYIAVA5xggBUDnGCAFQOcYIAVA5xggBUDnGCAFQOcYIAVA5xggBUDnGCAFQOcYIAVA5xggBUDnGCAFQOcYIAVA5xggBUDnGCAFQOcYIAVA5xggBUDnGCAFQOcYIAVA5xggBUDnGCAFQOcYIAVA5xggBUDnGCAFQOcYIAVA5xggBUDnGCAFQOcYIAVA5xggBUDnGCAFQOcYIAVA5xggBUDnGCAFQOcYIAVA5xggBUDnGCAFQOcYIAVA5xggBUDnGCAFQOcYIAVA5xggBUDnGCAFQOcYIAVA5xggBUDnPD8/Xw6AT7HAKQZgDQDkGYA0gxAmgFIMwBpBiDNAKQZgDQDkGYA0gxAmgFIMwBpBiDNAKQZgDQDkGYA0gxAmgFIMwBpBiDNAKQZgDQDkGYA0gxAmgFIMwBpBiDNAKQZgDQDkGYA0gxAmgFIMwBpBiDNAKQZgDQDkGYA0gxAmgFIMwBpBiDNAKQZgDQDkGYA0gxAmgFIMwBpBiDNAKQZgDQDkGYA0gxAmgFIMwBpBiDNAKQZgDQDkGYA0gxAmgFIMwBpBiDNAKQZgDQDkGYA0gxAmgFIMwBpBiDNAKQZgDQDkGYA0gxAmgFIMwBpBiDNAKQZgDQDkGYA0gxAmgFIMwBpBiDNAKQZgDQDkGYA0gxAmgFIMwBpBiDNAKQZgDQDkGYA0gxAmgFIMwBpBiDNAKQZgDQDkGYA0gxAmgFIMwBpBiDNAKQZgDQDkGYA0gxAmgFIMwBpBiDNAKQZgDQDkGYA0gxAmgFIMwBpBiDNAKQZgDQDkGYA0gxAmgFIMwBpBiDNAKQZgDQDkGYA0gxAmgFIMwBpBiDNAKQZgDQDkGYA0gxAmgFIMwBpBiDNAKQZgDQDkGYA0gxAmgFIMwBpBiDNAKQZgDQDkGYA0gxAmgFIewP3SloQh/j9MQAAAABJRU5ErkJggg==",
    "TileOutsideProjectExtents": false,
    "Code": 0,
    "Message": "success"
  }
	"""
  
  Scenario: Compaction Get Compaction Coverage Tiles With Surveyed Surfaces Included
  Given the Compaction Tiles service URI "/api/v2/compaction/productiondatatiles"
  And a projectUid "7925f179-013d-4aaf-aff4-7b9833bb06d6"
  And a displayMode "0" and a bbox "36.207437, -115.019999, 36.207473, -115.019959" and a width "256" and a height "256"
  When I request a Tile
  Then the Tile result should be
  """
  {
    "tileData": "iVBORw0KGgoAAAANSUhEUgAAAQAAAAEACAIAAADTED8xAAAABnRSTlMA/wD/AP83WBt9AAACl0lEQVR42u3TAQEAAAiAoPo/uoYIH9gb6FoBKBOANAFIE4A0AUgTgDQBSBOANAFIE4A0AUgTgDQBSBOANAFIE4A0AUgTgDQBSBOANAFIE4A0AUgTgDQBSBOANAFIE4A0AUgTgDQBSBOANAFIE4A0AUgTgDQBSBOANAFIE4A0AUgTgDQBSBOANAFIE4A0AUgTgDQBSBOANAFIE4A0AUgTgDQBSBOANAFIE4A0AUgTgDQBSBOANAFIE4A0AUgTgDQBSBOANAFIE4A0AUgTgDQBSBOANAFIE4A0AUgTgDQBSBOANAFIE4A0AUgTgDQBSBOANAFIE4A0AUgTgDQBSBOANAFIE4A0AUgTgDQBSBOANAFIE4A0AUgTgDQBSBOANAFIE4A0AUgTgDQBSBOANAFIE4A0AUgTgDQBSBOANAFIE4A0AUgTgDQBSBOANAFIE4A0AUgTgDQBSBOANAFIE4A0AUgTgDQBSBOANAFIE4A0AUgTgDQBSBOANAFIE4A0AUgTgDQBSBOANAFIE4A0AUgTgDQBSBOANAFIE4A0AUgTgDQBSBOANAFIE4A0AUgTgDQBSBOANAFIE4A0AUgTgDQBSBOANAFIE4A0AUgTgDQBSBOANAFIE4A0AUgTgDQBSBOANAFIE4A0AUgTgDQBSBOANAFIE4A0AUgTgDQBSBOANAFIE4A0AUgTgDQBSBOANAFIE4A0AUgTgDQBSBOANAFIE4A0AUgTgDQBSBOANAFIE4A0AUgTgDQBSBOANAFIE4A0AUgTgDQBSBOANAFIE4A0AUgTgDQBSBOANAFIE4A0AUgTgDQBSBOANAFIE4A0AUgTgDQBSBOANAFIE4A0AUgTgDQBSBOANAFIE4A0AUgTgDQBSBOANAFIeyF3ABDkaLbCAAAAAElFTkSuQmCC",
    "tileOutsideProjectExtents": false,
    "Code": 0,
    "Message": "success"
  }  
  """

  Scenario: Compaction Get Compaction Coverage Tiles With Surveyed Surfaces Excluded
  Given the Compaction Tiles service URI "/api/v2/compaction/productiondatatiles"
  And a projectUid "86a42bbf-9d0e-4079-850f-835496d715c5"
  And a displayMode "0" and a bbox "36.207437, -115.019999, 36.207473, -115.019959" and a width "256" and a height "256"
  When I request a Tile
  Then the Tile result should be
  """
  {
    "tileData": "iVBORw0KGgoAAAANSUhEUgAAAQAAAAEACAIAAADTED8xAAAABnRSTlMA/wD/AP83WBt9AAACzElEQVR42u3UQRGAQBADQc4ISnCPEpQcInhQW9OtIJ/J2nsfULUEQJkASBMAaQIgTQCkCYA0AZAmANIEQJoASBMAaQIgTQCkCYA0AZAmANIEQJoASBMAaQIgTQCkCYA0AZAmANIEQJoASBMAaQIgTQCkCYA0AZAmANIEQJoASBMAaQIgTQCkCYA0AZAmANIEQJoASBMAaQIgTQCkCYA0AZAmANIEQJoASBMAaQIgTQCkCYA0AZAmANIEQJoASBMAaQIgTQCkCYA0AZAmANIEQJoASBMAaQIgTQCkCYA0AZAmANIEQJoASBMAaQIgTQCkCYA0AZAmANIEQJoASBMAaQIgTQCkCYA0AZAmANIEQJoASBMAaQIgTQCkCYA0AZAmANIEQJoASBMAaQIgTQCkCYA0AZAmANIEQJoASBMAaQIgTQCkCYA0AZAmANIEQJoASBMAaQIgTQCkCYA0AZAmANIEQJoASBMAaQIgTQCkCYA0AZC2nuvvCUx23rMPVAB8IgDSBECaAEgTAGkCIE0ApAmANAGQJgDSBECaAEgTAGkCIE0ApAmANAGQJgDSBECaAEgTAGkCgMEEQJoASBMAaQIgTQCkCYA0AZAmANIEQJoASBMAaQIgTQCkCYA0AZAmANIEQJoASBMAaQIgTQCkCYA0AZAmANIEQJoASBMAaQIgTQCkCYA0AZAmANIEQJoASBMAaQIgTQCkCYA0AZAmANIEQJoASBMAaQIgTQCkCYA0AZAmANIEQJoASBMAaQIgTQCkCYA0AZAmANIEQJoASBMAaQIgTQCkCYA0AZAmANIEQJoASBMAaQIgTQCkCYA0AZAmANIEQJoASBMAaQIgTQCkCYA0AZAmANIEQJoASBMAaQIgTQCkCYA0AZAmANIEQJoASBMAaQIgTQCkCYA0AZAmANIEQJoASBMAaQIgTQCkCYA0AZAmANIEQNoLEuM3XwkCBu0AAAAASUVORK5CYII=",
    "tileOutsideProjectExtents": false,
    "Code": 0,
    "Message": "success"
  }  
  """

  Scenario: Compaction Get Elevation Palette 
	Given the Compaction Elevation Palette service URI "/api/v2/compaction/elevationpalette"
  And a projectUid "ff91dd40-1569-4765-a2bc-014321f76ace"
	When I request Elevation Palette
	Then the Elevation Palette result should be
  """
  {
    "palette": {
      "colorValues": [
        {
          "color": 13107200,
          "value": 591.9539794921875
        },
        {
          "color": 16711680,
          "value": 592.99088410408262
        },
        {
          "color": 14760960,
          "value": 594.02778871597786
        },
        {
          "color": 16734720,
          "value": 595.064693327873
        },
        {
          "color": 16744960,
          "value": 596.10159793976811
        },
        {
          "color": 16755200,
          "value": 597.13850255166335
        },
        {
          "color": 16762880,
          "value": 598.17540716355848
        },
        {
          "color": 16768000,
          "value": 599.2123117754536
        },
        {
          "color": 16442880,
          "value": 600.24921638734884
        },
        {
          "color": 14476800,
          "value": 601.286120999244
        },
        {
          "color": 13821440,
          "value": 602.32302561113909
        },
        {
          "color": 13166080,
          "value": 603.35993022303433
        },
        {
          "color": 11855360,
          "value": 604.39683483492945
        },
        {
          "color": 9889280,
          "value": 605.43373944682457
        },
        {
          "color": 8578560,
          "value": 606.47064405871981
        },
        {
          "color": 6615040,
          "value": 607.50754867061494
        },
        {
          "color": 65280,
          "value": 608.54445328251006
        },
        {
          "color": 61540,
          "value": 609.58135789440519
        },
        {
          "color": 59010,
          "value": 610.61826250630043
        },
        {
          "color": 59030,
          "value": 611.65516711819555
        },
        {
          "color": 59060,
          "value": 612.69207173009067
        },
        {
          "color": 59080,
          "value": 613.72897634198591
        },
        {
          "color": 59090,
          "value": 614.765880953881
        },
        {
          "color": 56540,
          "value": 615.80278556577616
        },
        {
          "color": 51430,
          "value": 616.8396901776714
        },
        {
          "color": 46320,
          "value": 617.87659478956652
        },
        {
          "color": 38645,
          "value": 618.91349940146165
        },
        {
          "color": 30970,
          "value": 619.95040401335689
        },
        {
          "color": 23295,
          "value": 620.987308625252
        },
        {
          "color": 18175,
          "value": 622.02421323714714
        },
        {
          "color": 255,
          "value": 623.06111784904238
        },
        {
          "color": 200,
          "value": 624.0980224609375
        }
      ],
      "aboveLastColor": 8388736,
      "belowFirstColor": 16711935
    },
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
          "color": 13107200,
          "value": 591.9539794921875
        },
        {
          "color": 16711680,
          "value": 592.99088410408262
        },
        {
          "color": 14760960,
          "value": 594.02778871597786
        },
        {
          "color": 16734720,
          "value": 595.064693327873
        },
        {
          "color": 16744960,
          "value": 596.10159793976811
        },
        {
          "color": 16755200,
          "value": 597.13850255166335
        },
        {
          "color": 16762880,
          "value": 598.17540716355848
        },
        {
          "color": 16768000,
          "value": 599.2123117754536
        },
        {
          "color": 16442880,
          "value": 600.24921638734884
        },
        {
          "color": 14476800,
          "value": 601.286120999244
        },
        {
          "color": 13821440,
          "value": 602.32302561113909
        },
        {
          "color": 13166080,
          "value": 603.35993022303433
        },
        {
          "color": 11855360,
          "value": 604.39683483492945
        },
        {
          "color": 9889280,
          "value": 605.43373944682457
        },
        {
          "color": 8578560,
          "value": 606.47064405871981
        },
        {
          "color": 6615040,
          "value": 607.50754867061494
        },
        {
          "color": 65280,
          "value": 608.54445328251006
        },
        {
          "color": 61540,
          "value": 609.58135789440519
        },
        {
          "color": 59010,
          "value": 610.61826250630043
        },
        {
          "color": 59030,
          "value": 611.65516711819555
        },
        {
          "color": 59060,
          "value": 612.69207173009067
        },
        {
          "color": 59080,
          "value": 613.72897634198591
        },
        {
          "color": 59090,
          "value": 614.765880953881
        },
        {
          "color": 56540,
          "value": 615.80278556577616
        },
        {
          "color": 51430,
          "value": 616.8396901776714
        },
        {
          "color": 46320,
          "value": 617.87659478956652
        },
        {
          "color": 38645,
          "value": 618.91349940146165
        },
        {
          "color": 30970,
          "value": 619.95040401335689
        },
        {
          "color": 23295,
          "value": 620.987308625252
        },
        {
          "color": 18175,
          "value": 622.02421323714714
        },
        {
          "color": 255,
          "value": 623.06111784904238
        },
        {
          "color": 200,
          "value": 624.0980224609375
        }
      ],
      "aboveLastColor": 8388736,
      "belowFirstColor": 16711935
    },
    "cmvDetailPalette": {
      "colorValues": [
        {
        "color": 2971523,
        "value": 0.0
				},
				{
					"color": 4430812,
					"value": 100.0
				},
				{
					"color": 12509169,
					"value": 200.0
				},
				{
					"color": 10341991,
					"value": 300.0
				},
				{
					"color": 7053374,
					"value": 400.0
				},
				{
					"color": 3828517,
					"value": 500.0
				},
				{
					"color": 16174803,
					"value": 600.0
				},
				{
					"color": 13990524,
					"value": 700.0
				},
				{
					"color": 12660791,
					"value": 800.0
				},
				{
					"color": 15105570,
					"value": 900.0
				},
				{
					"color": 14785888,
					"value": 1000.0
				},
				{
					"color": 15190446,
					"value": 1100.0
				},
				{
					"color": 5182823,
					"value": 1200.0
				},
				{
					"color": 9259433,
					"value": 1300.0
				},
				{
					"color": 13740258,
					"value": 1400.0
				},
				{
					"color": 1971179,
					"value": 1500.0
				}
      ],
      "aboveLastColor": null,
      "belowFirstColor": null
    },
    "passCountDetailPalette": {
      "colorValues": [
        {
          "color": 2971523,
          "value": 1
        },
        {
          "color": 4430812,
          "value": 2
        },
        {
          "color": 12509169,
          "value": 3
        },
        {
          "color": 10341991,
          "value": 4
        },
        {
          "color": 7053374,
          "value": 5
        },
        {
          "color": 3828517,
          "value": 6
        },
        {
          "color": 16174803,
          "value": 7
        },
        {
          "color": 13990524,
          "value": 8
        }
      ],
      "aboveLastColor": 12660791,
      "belowFirstColor": null
    },
    "passCountSummaryPalette": {
      "aboveTargetColor": 13959168,
      "onTargetColor": 9159498,
      "belowTargetColor": 87963
    },
    "cutFillPalette": {
      "colorValues": [
        {
          "color": 11789820,
          "value": -0.2
        },
        {
          "color": 236517,
          "value": -0.1
        },
        {
          "color": 87963,
          "value": -0.05
        },
        {
          "color": 9159498,
          "value": 0
        },
        {
          "color": 16764370,
          "value": 0.05
        },
        {
          "color": 15037299,
          "value": 0.1
        },
        {
          "color": 13959168,
          "value": 0.2
        }
      ],
      "aboveLastColor": null,
      "belowFirstColor": null
    },
    "temperatureSummaryPalette": {
      "aboveTargetColor": 13959168,
      "onTargetColor": 9159498,
      "belowTargetColor": 87963
    },
    "cmvSummaryPalette": {
      "aboveTargetColor": 13959168,
      "onTargetColor": 9159498,
      "belowTargetColor": 87963
    },
    "mdpSummaryPalette": {
      "aboveTargetColor": 13959168,
      "onTargetColor": 9159498,
      "belowTargetColor": 87963
    },
    "cmvPercentChangePalette": {
      "colorValues": [
        {
          "color": 9159498,
          "value": 5
        },
        {
          "color": 16764370,
          "value": 20
        },
        {
          "color": 15037299,
          "value": 50
        }
      ],
      "aboveLastColor": 13959168,
      "belowFirstColor": 33554431
    },
    "speedSummaryPalette": {
      "aboveTargetColor": 13959168,
      "onTargetColor": 9159498,
      "belowTargetColor": 87963
    },
    "temperatureDetailPalette": {
      "colorValues": [
        {
          "color": 2971523,
          "value": 70
        },
        {
          "color": 4430812,
          "value": 80
        },
        {
          "color": 12509169,
          "value": 90
        },
        {
          "color": 14479047,
          "value": 100
        },
        {
          "color": 10341991,
          "value": 110
        },
        {
          "color": 7053374,
          "value": 120
        },
        {
          "color": 3828517,
          "value": 130
        },
        {
          "color": 16174803,
          "value": 140
        },
        {
          "color": 13990524,
          "value": 150
        },
        {
          "color": 12660791,
          "value": 160
        }
      ],
      "aboveLastColor": null,
      "belowFirstColor": null
    },
    "Code": 0,
    "Message": "success"
  }
	"""



 
