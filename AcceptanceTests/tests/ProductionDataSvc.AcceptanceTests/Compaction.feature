Feature: Compaction
  I should be able to request compaction data

######################################################## CMV Summary ############################################################
Scenario Outline: Compaction Get CMV Summary - No Design Filter
	Given the Compaction service URI "/api/v2/compaction/cmv/summary" for operation "CMVSummary"
  And the result file "CompactionGetCMVSummaryResponse.json"
	And projectUid "<ProjectUID>"
	When I request result
  Then the result should match the "<ResultName>" from the repository
	Examples: 
	| RequetsName | ProjectUID                           | ResultName     |
	|             | ff91dd40-1569-4765-a2bc-014321f76ace | NoDesignFilter |

######################################################## MDP Summary ############################################################
Scenario Outline: Compaction Get MDP Summary - No Design Filter
	Given the Compaction service URI "/api/v2/compaction/mdp/summary" for operation "MDPSummary"
  And the result file "CompactionGetMDPSummaryResponse.json"
	And projectUid "<ProjectUID>"
	When I request result
  Then the result should match the "<ResultName>" from the repository
	Examples: 
	| RequetsName | ProjectUID                           | ResultName     |
	|             | ff91dd40-1569-4765-a2bc-014321f76ace | NoDesignFilter |

Scenario Outline: Compaction Get MDP Summary
  Given the Compaction service URI "/api/v2/compaction/mdp/summary" for operation "MDPSummary"
  And the result file "CompactionGetMDPSummaryResponse.json"
  And projectUid "<ProjectUID>"
	And designUid "<DesignUID>"
	When I request result
	Then the result should match the "<ResultName>" from the repository
	Examples: 
	| RequetsName      | ProjectUID                           | DesignUID                            | ResultName       |
	| DesignOutside    | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | 220e12e5-ce92-4645-8f01-1942a2d5a57f | DesignOutside    |
  | DesignIntersepts | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff | DesignIntersepts |

######################################################## Pass Count Summary #####################################################
Scenario Outline: Compaction Get Passcount Summary - No Design Filter
	Given the Compaction service URI "/api/v2/compaction/passcounts/summary" for operation "PassCountSummary"
  And the result file "CompactionGetPassCountSummaryResponse.json"
	And projectUid "<ProjectUID>"
	When I request result
  Then the result should match the "<ResultName>" from the repository
	Examples: 
	| RequetsName | ProjectUID                           | ResultName     |
	|             | ff91dd40-1569-4765-a2bc-014321f76ace | NoDesignFilter |

######################################################## Pass Count Details #####################################################
Scenario Outline: Compaction Get Passcount Details - No Design Filter
	Given the Compaction service URI "/api/v2/compaction/passcounts/details" for operation "PassCountDetails"
  And the result file "CompactionGetPassCountDetailsResponse.json"
	And projectUid "<ProjectUID>"
	When I request result
  Then the result should match the "<ResultName>" from the repository
	Examples: 
	| RequetsName | ProjectUID                           | ResultName     |
	|             | ff91dd40-1569-4765-a2bc-014321f76ace | NoDesignFilter |

######################################################## Temperature Summary ####################################################
Scenario Outline: Compaction Get Temperature Summary - No Design Filter
	Given the Compaction service URI "/api/v2/compaction/temperature/summary" for operation "TemperatureSummary"
  And the result file "CompactionGetTemperatureSummaryResponse.json"
	And projectUid "<ProjectUID>"
	When I request result
  Then the result should match the "<ResultName>" from the repository
	Examples: 
	| RequetsName | ProjectUID                           | ResultName     |
	|             | ff91dd40-1569-4765-a2bc-014321f76ace | NoDesignFilter |

######################################################## Speed Summary ##########################################################
Scenario Outline: Compaction Get Speed Summary - No Design Filter
	Given the Compaction service URI "/api/v2/compaction/speed/summary" for operation "SpeedSummary"
  And the result file "CompactionGetSpeedSummaryResponse.json"
	And projectUid "<ProjectUID>"
	When I request result
  Then the result should match the "<ResultName>" from the repository
	Examples: 
	| RequetsName | ProjectUID                           | ResultName     |
	|             | ff91dd40-1569-4765-a2bc-014321f76ace | NoDesignFilter |

######################################################## CMV % Change Summary ####################################################
Scenario Outline: Compaction Get CMV % Change Summary - No Design Filter
	Given the Compaction service URI "/api/v2/compaction/cmv/percentchange" for operation "CMVPercentChangeSummary"
  And the result file "CompactionGetCMVPercentChangeSummaryResponse.json"
	And projectUid "<ProjectUID>"
	When I request result
  Then the result should match the "<ResultName>" from the repository
	Examples: 
	| RequetsName | ProjectUID                           | ResultName     |
	|             | ff91dd40-1569-4765-a2bc-014321f76ace | NoDesignFilter |

######################################################## Elevation Range ########################################################
Scenario Outline: Compaction Get Elevation Range - No Design Filter
	Given the Compaction service URI "/api/v2/compaction/elevationrange" for operation "ElevationRange"
  And the result file "CompactionGetElevationRangeResponse.json"
	And projectUid "<ProjectUID>"
	When I request result
  Then the result should match the "<ResultName>" from the repository
	Examples: 
	| RequetsName | ProjectUID                           | ResultName     |
	|             | ff91dd40-1569-4765-a2bc-014321f76ace | NoDesignFilter |
  
Scenario Outline: Compaction Get Elevation Range - No Data
	Given the Compaction service URI "/api/v2/compaction/elevationrange" for operation "ElevationRange"
  And the result file "CompactionGetElevationRangeResponse.json"
	And projectUid "<ProjectUID>"
  And startUtc "<StartUTC>" and endUtc "<EndUTC>"
	When I request result
  Then the result should match the "<ResultName>" from the repository
	Examples: 
	| RequetsName | ProjectUID                           | StartUTC   | EndUTC     | ResultName |
	|             | ff91dd40-1569-4765-a2bc-014321f76ace | 2017-01-01 | 2017-01-01 | NoData     |

######################################################## Project Statistics #####################################################
Scenario Outline: Compaction Get Project Statistics - Good Request
	Given the Compaction service URI "/api/v2/compaction/projectstatistics" for operation "ProjectStatistics"  
  And the result file "CompactionGetProjectStatisticsResponse.json"	
  And projectUid "<ProjectUID>"
	When I request result
  Then the result should match the "<ResultName>" from the repository
	Examples: 
	| RequetsName | ProjectUID                           | ResultName  |
	|             | ff91dd40-1569-4765-a2bc-014321f76ace | GoodRequest |

###################################################### Production Data Tiles ####################################################
Scenario Outline: Compaction Get Tiles - No Design Filter - Good Request
	Given the Compaction service URI "/api/v2/compaction/productiondatatiles" for operation "ProductionDataTiles"  
  And the result file "CompactionGetProductionDataTilesResponse.json"	
  And projectUid "<ProjectUID>"
	And displayMode "0" and bbox "<BBox>" and width "<Width>" and height "<Height>"
  When I request result
  Then the result should match the "<ResultName>" from the repository
	Examples: 
	| RequetsName  | ProjectUID                           | BBox                                                                                        | Width | Height | ResultName  |
	|              | ff91dd40-1569-4765-a2bc-014321f76ace | 36.206964000089840283, -115.0203540002853231, 36.206956000089640213, -115.02034400028509253 | 256   | 256    | GoodRequest |
  | SS Included  | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | 36.207437, -115.019999, 36.207473, -115.019959                                              | 256   | 256    | SSIncluded  |
  | SS Excluded  | 86a42bbf-9d0e-4079-850f-835496d715c5 | 36.207437, -115.019999, 36.207473, -115.019959                                              | 256   | 256    | SSExcluded  |
  
######################################################## Elevation Palette ######################################################
Scenario Outline: Compaction Get Elevation Palette - No Design Filter
	Given the Compaction service URI "/api/v2/compaction/elevationpalette" for operation "ElevationPalette"
  And the result file "CompactionGetElevationPaletteResponse.json"
	And projectUid "<ProjectUID>"
	When I request result
  Then the result should match the "<ResultName>" from the repository
	Examples: 
	| RequetsName | ProjectUID                           | ResultName     |
	|             | ff91dd40-1569-4765-a2bc-014321f76ace | NoDesignFilter |

Scenario Outline: Compaction Get Elevation Palette - No Data
	Given the Compaction service URI "/api/v2/compaction/elevationpalette" for operation "ElevationPalette"
  And the result file "CompactionGetElevationPaletteResponse.json"
	And projectUid "<ProjectUID>"
  And startUtc "<StartUTC>" and endUtc "<EndUTC>"
	When I request result
  Then the result should match the "<ResultName>" from the repository
	Examples: 
	| RequetsName | ProjectUID                           | StartUTC   | EndUTC     | ResultName |
	|             | ff91dd40-1569-4765-a2bc-014321f76ace | 2017-01-01 | 2017-01-01 | NoData     |

####################################################### Compaction Palettes #####################################################
Scenario Outline: Compaction Get Palettes
	Given the Compaction service URI "/api/v2/compaction/colorpalettes" for operation "CompactionPalettes"
  And the result file "CompactionGetCompactionPalettesResponse.json"
	And projectUid "<ProjectUID>"
	When I request result
  Then the result should match the "<ResultName>" from the repository
	Examples: 
	| RequetsName | ProjectUID                           | ResultName  |
	|             | ff91dd40-1569-4765-a2bc-014321f76ace | GoodRequest |

