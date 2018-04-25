Feature: Tiles
	I should be able to request tiles.

Background: 
	Given the Tile service URI "/api/v1/tiles", request repo "TileRequest.json" and result repo "TileResponse.json"

Scenario Outline: Tiles - Serialized
	When I request Tiles supplying "<ParameterName>" paramters from the repository
	Then the Tiles response should match "<ResultName>" result from the repository
	Examples: 
	| ParameterName                           | ResultName                              |
	| Height                                  | Height                                  |
	| CCV                                     | CCV                                     |
	| CCVPercent                              | CCVPercent                              |
	| PassCount                               | PassCount                               |
	#| CutFill                                 | CutFill                                 |
	#| CutFillCustomPalettes                   | CutFillCustomPalettes                   |
	| TemperatureSummary                      | TemperatureSummary                      |
	| CCVSummary                              | CCVSummary                              |
	| CCVPercentSummary                       | CCVPercentSummary                       |
	| PassCountSummaryConstTarget             | PassCountSummaryConstTarget             |
	| PassCountSummaryRangeTarget             | PassCountSummaryRangeTarget             |
	| CompactionCoverage                      | CompactionCoverage                      |
	| VolumeCoverage                          | VolumeCoverage                          |
	| MDP                                     | MDP                                     |
	| MDPSummary                              | MDPSummary                              |
	| MDPPercent                              | MDPPercent                              |
	| MDPPercentSummary                       | MDPPercentSummary                       |
	| MachineSpeed                            | MachineSpeed                            |
	| MachineSpeedSummary                     | MachineSpeedSummary                     |
	| CCVPercentChange                        | CCVPercentChange                        |
	| CCVPercentChangeFiltered                | CCVPercentChangeFiltered                |
	| CCVPercentChangeSinglePass              | CCVPercentChangeSinglePass              |
	| CCVPercentChangeOverrideCCV             | CCVPercentChangeOverrideCCV             |
	| ThicknessSummary                        | ThicknessSummary                        |
	| ThicknessSummaryPartialOverlap          | ThicknessSummaryPartialOverlap          |
	| ThicknessSummaryLayerIdFiltered         | ThicknessSummaryLayerIdFiltered         |
	| ThicknessSummaryDesignToFilter          | ThicknessSummaryDesignToFilter          |
	| PassCountOutOfBoundary                  | PassCountOutOfBoundary                  |
	| SpeedSummaryExcludeSuperdedLifts        | SpeedSummaryExcludeSuperdedLifts        |
	| SpeedSummaryIncludeSuperdedLifts        | SpeedSummaryIncludeSuperdedLifts        |
	| RedWhenZoomedOutTooMuch                 | RedWhenZoomedOutTooMuch                 |
	| CCVChangeExcludeSupersededLifts         | CCVChangeExcludeSupersededLifts         |
	| CCVChangeIncludeSupersededLifts         | CCVChangeIncludeSupersededLifts         |
	| CCVChangeTopLayerIncludeSupersededLifts | CCVChangeTopLayerIncludeSupersededLifts |
	| CCVChangeNoneLiftDetection              | CCVChangeNoneLiftDetection              |
	| CCVChangeNoChange                       | CCVChangeNoChange                       |

Scenario Outline: Tiles - PNG
	When I request PNG Tiles supplying "<ParameterName>" paramters from the repository
	Then the PNG Tiles response should match "<ResultName>" result from the repository
	Examples: 
	| ParameterName                           | ResultName                              |
	| Height                                  | Height                                  |
	| CCV                                     | CCV                                     |
	| CCVPercent                              | CCVPercent                              |
	| PassCount                               | PassCount                               |
	#| CutFill                                 | CutFill                                 |
	#| CutFillCustomPalettes                   | CutFillCustomPalettes                   |
	| TemperatureSummary                      | TemperatureSummary                      |
	| CCVSummary                              | CCVSummary                              |
	| CCVPercentSummary                       | CCVPercentSummary                       |
	| PassCountSummaryConstTarget             | PassCountSummaryConstTarget             |
	| PassCountSummaryRangeTarget             | PassCountSummaryRangeTarget             |
	| CompactionCoverage                      | CompactionCoverage                      |
	| VolumeCoverage                          | VolumeCoverage                          |
	| MDP                                     | MDP                                     |
	| MDPSummary                              | MDPSummary                              |
	| MDPPercent                              | MDPPercent                              |
	| MDPPercentSummary                       | MDPPercentSummary                       |
	| MachineSpeed                            | MachineSpeed                            |
	| MachineSpeedSummary                     | MachineSpeedSummary                     |
	| CCVPercentChange                        | CCVPercentChange                        |
	| CCVPercentChangeFiltered                | CCVPercentChangeFiltered                |
	| CCVPercentChangeSinglePass              | CCVPercentChangeSinglePass              |
	| CCVPercentChangeOverrideCCV             | CCVPercentChangeOverrideCCV             |
	| ThicknessSummary                        | ThicknessSummary                        |
	| ThicknessSummaryPartialOverlap          | ThicknessSummaryPartialOverlap          |
	| ThicknessSummaryLayerIdFiltered         | ThicknessSummaryLayerIdFiltered         |
	| ThicknessSummaryDesignToFilter          | ThicknessSummaryDesignToFilter          |
	| PassCountOutOfBoundary                  | PassCountOutOfBoundary                  |
	| SpeedSummaryExcludeSuperdedLifts        | SpeedSummaryExcludeSuperdedLifts        |
	| SpeedSummaryIncludeSuperdedLifts        | SpeedSummaryIncludeSuperdedLifts        |
	| RedWhenZoomedOutTooMuch                 | RedWhenZoomedOutTooMuch                 |
	| CCVChangeExcludeSupersededLifts         | CCVChangeExcludeSupersededLifts         |
	| CCVChangeIncludeSupersededLifts         | CCVChangeIncludeSupersededLifts         |
	| CCVChangeTopLayerIncludeSupersededLifts | CCVChangeTopLayerIncludeSupersededLifts |
	| CCVChangeNoneLiftDetection              | CCVChangeNoneLiftDetection              |
	| CCVChangeNoChange                       | CCVChangeNoChange                       |

Scenario Outline: Tiles - Raw PNG
	Given the PNG Tile service URI "/api/v1/tiles/png"
	When I request PNG Tiles supplying "<ParameterName>" paramters from the repository
	Then the Raw PNG Tiles response should match "<ResultName>" result from the repository
	And the X-Warning in the response header should be "<X-Warning>"
	Examples: 
	| ParameterName                           | ResultName                              | X-Warning |
	| Height                                  | Height                                  | False     |
	| CCV                                     | CCV                                     | False     |
	| CCVPercent                              | CCVPercent                              | False     |
	| PassCount                               | PassCount                               | False     |
	#| CutFill                                 | CutFill                                 | False     |
	#| CutFillCustomPalettes                   | CutFillCustomPalettes                   | False     |
	| TemperatureSummary                      | TemperatureSummary                      | False     |
	| CCVSummary                              | CCVSummary                              | False     |
	| CCVPercentSummary                       | CCVPercentSummary                       | False     |
	| PassCountSummaryConstTarget             | PassCountSummaryConstTarget             | False     |
	| PassCountSummaryRangeTarget             | PassCountSummaryRangeTarget             | False     |
	| CompactionCoverage                      | CompactionCoverage                      | False     |
	| VolumeCoverage                          | VolumeCoverage                          | False     |
	| MDP                                     | MDP                                     | False     |
	| MDPSummary                              | MDPSummary                              | False     |
	| MDPPercent                              | MDPPercent                              | False     |
	| MDPPercentSummary                       | MDPPercentSummary                       | False     |
	| MachineSpeed                            | MachineSpeed                            | False     |
	| MachineSpeedSummary                     | MachineSpeedSummary                     | False     |
	| CCVPercentChange                        | CCVPercentChange                        | False     |
	| CCVPercentChangeFiltered                | CCVPercentChangeFiltered                | False     |
	| CCVPercentChangeSinglePass              | CCVPercentChangeSinglePass              | False     |
	| CCVPercentChangeOverrideCCV             | CCVPercentChangeOverrideCCV             | False     |
	| ThicknessSummary                        | ThicknessSummary                        | False     |
	| ThicknessSummaryPartialOverlap          | ThicknessSummaryPartialOverlap          | False     |
	| ThicknessSummaryLayerIdFiltered         | ThicknessSummaryLayerIdFiltered         | False     |
	| ThicknessSummaryDesignToFilter          | ThicknessSummaryDesignToFilter          | False     |
	| PassCountOutOfBoundary                  | PassCountOutOfBoundary                  | True      |
	| SpeedSummaryExcludeSuperdedLifts        | SpeedSummaryExcludeSuperdedLifts        | False     |
	| SpeedSummaryIncludeSuperdedLifts        | SpeedSummaryIncludeSuperdedLifts        | False     |
	| RedWhenZoomedOutTooMuch                 | RedWhenZoomedOutTooMuch                 | False     |
	| CCVChangeExcludeSupersededLifts         | CCVChangeExcludeSupersededLifts         | False     |
	| CCVChangeIncludeSupersededLifts         | CCVChangeIncludeSupersededLifts         | False     |
	| CCVChangeTopLayerIncludeSupersededLifts | CCVChangeTopLayerIncludeSupersededLifts | False     |
	| CCVChangeNoneLiftDetection              | CCVChangeNoneLiftDetection              | False     |
	| CCVChangeNoChange                       | CCVChangeNoChange                       | False     |

Scenario Outline: Tiles - Bad Request
	When I request Tiles supplying "<ParameterName>" paramters from the repository expecting BadRequest
	Then the response should contain error code <errorCode>
	Examples: 
	| ParameterName                  | errorCode |
	| NullProjectId                  | -1        |
	| CutfillMissingDesign           | -1        |
	| UnsupportedVolumeType          | -1        |
	| SpeedSummaryTooManyColorValues | -1        |