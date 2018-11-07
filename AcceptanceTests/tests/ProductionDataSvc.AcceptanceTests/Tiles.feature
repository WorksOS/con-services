Feature: Tiles
  I should be able to request tiles.

Scenario Outline: Tiles - Serialized
  Given the service route "/api/v1/tiles" request repo "TileRequest.json" and result repo "TileResponse.json"
  When I POST with parameter "<ParameterName>" I expect response code <HttpCode>
  Then the response should match "<ResultName>" from the repository
  Examples: 
  | ParameterName                           | ResultName                              | HttpCode |
  | Height                                  | Height                                  | 200      |
  | CCV                                     | CCV                                     | 200      |
  | CCVPercent                              | CCVPercent                              | 200      |
  | PassCount                               | PassCount                               | 200      |
  | CutFill                                 | CutFill                                 | 200      |
  | CutFillCustomPalettes                   | CutFillCustomPalettes                   | 200      |
  | TemperatureSummary                      | TemperatureSummary                      | 200      |
  | CCVSummary                              | CCVSummary                              | 200      |
  | CCVPercentSummary                       | CCVPercentSummary                       | 200      |
  | PassCountSummaryConstTarget             | PassCountSummaryConstTarget             | 200      |
  | PassCountSummaryRangeTarget             | PassCountSummaryRangeTarget             | 200      |
  | CompactionCoverage                      | CompactionCoverage                      | 200      |
  | MDP                                     | MDP                                     | 200      |
  | MDPSummary                              | MDPSummary                              | 200      |
  | MDPPercent                              | MDPPercent                              | 200      |
  | MDPPercentSummary                       | MDPPercentSummary                       | 200      |
  | MachineSpeed                            | MachineSpeed                            | 200      |
  | MachineSpeedSummary                     | MachineSpeedSummary                     | 200      |
  | CCVPercentChange                        | CCVPercentChange                        | 200      |
  | CCVPercentChangeFiltered                | CCVPercentChangeFiltered                | 200      |
  | CCVPercentChangeSinglePass              | CCVPercentChangeSinglePass              | 200      |
  | CCVPercentChangeOverrideCCV             | CCVPercentChangeOverrideCCV             | 200      |
  | ThicknessSummary                        | ThicknessSummary                        | 200      |
  | ThicknessSummaryPartialOverlap          | ThicknessSummaryPartialOverlap          | 200      |
  | ThicknessSummaryLayerIdFiltered         | ThicknessSummaryLayerIdFiltered         | 200      |
  | ThicknessSummaryDesignToFilter          | ThicknessSummaryDesignToFilter          | 200      |
  | PassCountOutOfBoundary                  | PassCountOutOfBoundary                  | 200      |
  | SpeedSummaryExcludeSuperdedLifts        | SpeedSummaryExcludeSuperdedLifts        | 200      |
  | SpeedSummaryIncludeSuperdedLifts        | SpeedSummaryIncludeSuperdedLifts        | 200      |
  | RedWhenZoomedOutTooMuch                 | RedWhenZoomedOutTooMuch                 | 200      |
  | CCVChangeExcludeSupersededLifts         | CCVChangeExcludeSupersededLifts         | 200      |
  | CCVChangeIncludeSupersededLifts         | CCVChangeIncludeSupersededLifts         | 200      |
  | CCVChangeTopLayerIncludeSupersededLifts | CCVChangeTopLayerIncludeSupersededLifts | 200      |
  | CCVChangeNoneLiftDetection              | CCVChangeNoneLiftDetection              | 200      |
  | CCVChangeNoChange                       | CCVChangeNoChange                       | 200      |

Scenario Outline: Tiles - PNG
  Given the service route "/api/v1/tiles" request repo "TileRequest.json" and result repo "TileResponse.json"
  When I POST with parameter "<ParameterName>" I expect response code <HttpCode>
  Then the response should match "<ResultName>" from the repository
  Examples: 
  | ParameterName                           | ResultName                              | HttpCode |
  | Height                                  | Height                                  | 200      |
  | CCV                                     | CCV                                     | 200      |
  | CCVPercent                              | CCVPercent                              | 200      |
  | PassCount                               | PassCount                               | 200      |
  | CutFill                                 | CutFill                                 | 200      |
  | CutFillCustomPalettes                   | CutFillCustomPalettes                   | 200      |
  | TemperatureSummary                      | TemperatureSummary                      | 200      |
  | CCVSummary                              | CCVSummary                              | 200      |
  | CCVPercentSummary                       | CCVPercentSummary                       | 200      |
  | PassCountSummaryConstTarget             | PassCountSummaryConstTarget             | 200      |
  | PassCountSummaryRangeTarget             | PassCountSummaryRangeTarget             | 200      |
  | CompactionCoverage                      | CompactionCoverage                      | 200      |
  | MDP                                     | MDP                                     | 200      |
  | MDPSummary                              | MDPSummary                              | 200      |
  | MDPPercent                              | MDPPercent                              | 200      |
  | MDPPercentSummary                       | MDPPercentSummary                       | 200      |
  | MachineSpeed                            | MachineSpeed                            | 200      |
  | MachineSpeedSummary                     | MachineSpeedSummary                     | 200      |
  | CCVPercentChange                        | CCVPercentChange                        | 200      |
  | CCVPercentChangeFiltered                | CCVPercentChangeFiltered                | 200      |
  | CCVPercentChangeSinglePass              | CCVPercentChangeSinglePass              | 200      |
  | CCVPercentChangeOverrideCCV             | CCVPercentChangeOverrideCCV             | 200      |
  | ThicknessSummary                        | ThicknessSummary                        | 200      |
  | ThicknessSummaryPartialOverlap          | ThicknessSummaryPartialOverlap          | 200      |
  | ThicknessSummaryLayerIdFiltered         | ThicknessSummaryLayerIdFiltered         | 200      |
  | ThicknessSummaryDesignToFilter          | ThicknessSummaryDesignToFilter          | 200      |
  | PassCountOutOfBoundary                  | PassCountOutOfBoundary                  | 200      |
  | SpeedSummaryExcludeSuperdedLifts        | SpeedSummaryExcludeSuperdedLifts        | 200      |
  | SpeedSummaryIncludeSuperdedLifts        | SpeedSummaryIncludeSuperdedLifts        | 200      |
  | RedWhenZoomedOutTooMuch                 | RedWhenZoomedOutTooMuch                 | 200      |
  | CCVChangeExcludeSupersededLifts         | CCVChangeExcludeSupersededLifts         | 200      |
  | CCVChangeIncludeSupersededLifts         | CCVChangeIncludeSupersededLifts         | 200      |
  | CCVChangeTopLayerIncludeSupersededLifts | CCVChangeTopLayerIncludeSupersededLifts | 200      |
  | CCVChangeNoneLiftDetection              | CCVChangeNoneLiftDetection              | 200      |
  | CCVChangeNoChange                       | CCVChangeNoChange                       | 200      |

Scenario Outline: Tiles - Raw PNG
  Given the service route "/api/v1/tiles/png" request repo "TileRequest.json" and result repo "TileResponse.json"
  #When I request PNG Tiles supplying "<ParameterName>" parameters from the repository
  When I POST with parameter "<ParameterName>" I expect response code 200
  Then the Raw PNG Tiles response should match "<ResultName>" result from the repository
  Examples: 
  | ParameterName                           | ResultName                              | X-Warning |
  | Height                                  | Height                                  | False     |
  | CCV                                     | CCV                                     | False     |
  | CCVPercent                              | CCVPercent                              | False     |
  | PassCount                               | PassCount                               | False     |
  | CutFill                                 | CutFill                                 | False     |
  | CutFillCustomPalettes                   | CutFillCustomPalettes                   | False     |
  | TemperatureSummary                      | TemperatureSummary                      | False     |
  | CCVSummary                              | CCVSummary                              | False     |
  | CCVPercentSummary                       | CCVPercentSummary                       | False     |
  | PassCountSummaryConstTarget             | PassCountSummaryConstTarget             | False     |
  | PassCountSummaryRangeTarget             | PassCountSummaryRangeTarget             | False     |
  | CompactionCoverage                      | CompactionCoverage                      | False     |
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
  Given the service route "/api/v1/tiles" request repo "TileRequest.json" and result repo "TileResponse.json"
  When I POST with parameter "<ParameterName>" I expect response code <HttpCode>
  Then the response should contain code "<errorCode>"
  Examples: 
  | ParameterName                  | errorCode | HttpCode |
  | NullProjectId                  | -1        | 400      |
  | CutfillMissingDesign           | -1        | 400      |
  | UnsupportedVolumeType          | -1        | 400      |
  | SpeedSummaryTooManyColorValues | -1        | 400      |
