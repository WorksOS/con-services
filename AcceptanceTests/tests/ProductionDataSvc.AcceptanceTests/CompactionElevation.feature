Feature: CompactionElevation
  I should be able to request compaction elevation and project statistics

######################################################## Elevation Range ########################################################
Scenario Outline: Compaction Get Elevation Range - No Design Filter
	Given the Compaction service URI "/api/v2/compaction/elevationrange" for operation "ElevationRange"
  And the result file "CompactionGetElevationAndProjectStatisticsDataResponse.json"
	And projectUid "<ProjectUID>"
	When I request result
  Then the result should match the "<ResultName>" from the repository
	Examples: 
	| RequestName | ProjectUID                           | ResultName        |
	|             | ff91dd40-1569-4765-a2bc-014321f76ace | NoDesignFilter_ER |
  
Scenario Outline: Compaction Get Elevation Range - No Data
	Given the Compaction service URI "/api/v2/compaction/elevationrange" for operation "ElevationRange"
  And the result file "CompactionGetElevationAndProjectStatisticsDataResponse.json"
	And projectUid "<ProjectUID>"
  And startUtc "<StartUTC>" and endUtc "<EndUTC>"
	When I request result
  Then the result should match the "<ResultName>" from the repository
	Examples: 
	| RequestName | ProjectUID                           | StartUTC   | EndUTC     | ResultName |
	|             | ff91dd40-1569-4765-a2bc-014321f76ace | 2017-01-01 | 2017-01-01 | NoData_ER  |

#Scenario Outline: Compaction Get Speed Summary
#  Given the Compaction service URI "/api/v2/compaction/elevationrange" for operation "ElevationRange"
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
	Given the Compaction service URI "/api/v2/compaction/projectstatistics" for operation "ProjectStatistics"  
  And the result file "CompactionGetElevationAndProjectStatisticsDataResponse.json"	
  And projectUid "<ProjectUID>"
	When I request result
  Then the result should match the "<ResultName>" from the repository
	Examples: 
	| RequestName | ProjectUID                           | ResultName      |
	|             | ff91dd40-1569-4765-a2bc-014321f76ace | GoodRequest_PRS |
