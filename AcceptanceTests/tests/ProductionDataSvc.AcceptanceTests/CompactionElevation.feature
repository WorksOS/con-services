Feature: CompactionElevation
I should be able to request compaction elevation and project statistics

######################################################## Elevation Range ########################################################
Scenario Outline: Compaction Get Elevation Range - No Design Filter
Given the Compaction service URI "/api/v2/elevationrange" for operation "ElevationRange"
And the result file "CompactionGetElevationAndProjectStatisticsDataResponse.json"
And projectUid "<ProjectUID>"
When I request result
Then the result should match the "<ResultName>" from the repository
Examples: 
| RequestName | ProjectUID                           | ResultName        |
|             | ff91dd40-1569-4765-a2bc-014321f76ace | NoDesignFilter_ER |
  
Scenario Outline: Compaction Get Elevation Range - No Data
Given the Compaction service URI "/api/v2/elevationrange" for operation "ElevationRange"
And the result file "CompactionGetElevationAndProjectStatisticsDataResponse.json"
And projectUid "<ProjectUID>"
And filterUid "<FilterUid>"
When I request result
Then the result should match the "<ResultName>" from the repository
Examples: 
| RequestName | ProjectUID                           | FilterUid                            | ResultName |
|             | ff91dd40-1569-4765-a2bc-014321f76ace | 200c7b47-b5e6-48ee-a731-7df6623412da | NoData_ER  |

Scenario Outline: Compaction Get Elevation Range
Given the Compaction service URI "/api/v2/elevationrange" for operation "ElevationRange"
And the result file "CompactionGetElevationAndProjectStatisticsDataResponse.json"
And projectUid "<ProjectUID>"
And filterUid "<FilterUid>"
When I request result
Then the result should match the "<ResultName>" from the repository
Examples: 
| RequestName      | ProjectUID                           | FilterUid                            | ResultName      |
| AlignmentFilter  | ff91dd40-1569-4765-a2bc-014321f76ace | 2811c7c3-d270-4d63-97e2-fc3340bf6c7a | AlignmentFilter |

#Scenario Outline: Compaction Get Speed Summary
#  Given the Compaction service URI "/api/v2/elevationrange" for operation "ElevationRange"
#  And the result file "CompactionGetElevationAndProjectStatisticsDataResponse.json"
#  And projectUid "<ProjectUID>"
#	And filterUid "<FilterUID>"
#	When I request result
#	Then the result should match the "<ResultName>" from the repository
#	Examples: 
#	| RequestName      | ProjectUID                           | FilterUID                            | ResultName               |
#  | DesignOutside    | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | 1cf81668-1739-42d5-b068-ea025588796a | DesignOutside_ER    |
#	| DesignIntersects | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | 3d9086f2-3c04-4d92-9141-5134932b1523 | DesignIntersects_ER |

######################################################## Project Statistics #####################################################
Scenario Outline: Compaction Get Project Statistics - Good Request
Given the Compaction service URI "/api/v2/projectstatistics" for operation "ProjectStatistics"  
And the result file "CompactionGetElevationAndProjectStatisticsDataResponse.json"	
And projectUid "<ProjectUID>"
When I request result
Then the result should match the "<ResultName>" from the repository
Examples: 
| RequestName | ProjectUID                           | ResultName      |
|             | ff91dd40-1569-4765-a2bc-014321f76ace | GoodRequest_PRS |

#######################################################Alignment offset test####################################################
Scenario Outline: Compaction Get Alignment Station Range - Good request
Given the Compaction service URI "/api/v2/alignmentstationrange" for operation "GetAlignmentStationRange"   
And the result file "CompactionGetElevationAndProjectStatisticsDataResponse.json"
And projectUid "<ProjectUID>"
And fileUid "<FileUID>"
When I request result
Then the result should match the "<ResultName>" from the repository
Examples: 
| FileUID                              | ProjectUID                           | ResultName             |
| 6ece671b-7959-4a14-86fa-6bfe6ef4dd62 | ff91dd40-1569-4765-a2bc-014321f76ace | Large_Sites_Align_Good |
| c6662be1-0f94-4897-b9af-28aeeabcd09b | ff91dd40-1569-4765-a2bc-014321f76ace | TopCon_Align_Good      |
| 3ead0c55-1e1f-4d30-aaf8-873526a2ab82 | ff91dd40-1569-4765-a2bc-014321f76ace | Milling_Align_Good     |

Scenario Outline: Compaction Get Alignment Station Range - Bad request
Given the Compaction service URI "/api/v2/alignmentstationrange" for operation "GetAlignmentStationRange"   
And the result file "CompactionGetElevationAndProjectStatisticsDataResponse.json"
And projectUid "<ProjectUID>"
And fileUid "<FileUID>"
When I request a Station Range Expecting BadRequest
Then I should get error code "<ErrorCode>" and message "<Message>"
# First example is a design file
# Second example is a file not belonging to the project
Examples: 
| FileUID                              | ProjectUID                           | ErrorCode | Message                                        |
| dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff | ff91dd40-1569-4765-a2bc-014321f76ace | -4        | Failed to get station range for alignment file |
| dcb41fbd-7d43-4b36-a144-e22bbccc24a8 | ff91dd40-1569-4765-a2bc-014321f76ace | -1        | Unable to access design file.                  |
