Feature: CompactionElevation
I should be able to request compaction elevation and project statistics

######################################################## Elevation Range ########################################################
Scenario Outline: Compaction Get Elevation Range
Given the Compaction service URI "/api/v2/elevationrange" for operation "ElevationRange"
And the result file "CompactionGetElevationAndProjectStatisticsDataResponse.json"
And projectUid "<ProjectUID>"
And filterUid "<FilterUid>"
When I request result
Then the result should match the "<ResultName>" from the repository
Examples: 
| RequestName      | ProjectUID                           | FilterUid                            | ResultName          |
| NoDesignFilter   | ff91dd40-1569-4765-a2bc-014321f76ace |                                      | NoDesignFilter_ER   |
| NoData           | ff91dd40-1569-4765-a2bc-014321f76ace | 200c7b47-b5e6-48ee-a731-7df6623412da | NoData_ER           |
| AutomaticsFilter | ff91dd40-1569-4765-a2bc-014321f76ace | 887f90a6-56b9-4266-9d62-ff99e7d346f0 | AutomaticsFilter_ER |
| AlignmentFilter  | ff91dd40-1569-4765-a2bc-014321f76ace | 2811c7c3-d270-4d63-97e2-fc3340bf6c7a | AlignmentFilter     |

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
| ResultName             | FileUID                              | ProjectUID                           |
| Large_Sites_Align_Good | 6ece671b-7959-4a14-86fa-6bfe6ef4dd62 | ff91dd40-1569-4765-a2bc-014321f76ace |
| TopCon_Align_Good      | c6662be1-0f94-4897-b9af-28aeeabcd09b | ff91dd40-1569-4765-a2bc-014321f76ace |
| Milling_Align_Good     | 3ead0c55-1e1f-4d30-aaf8-873526a2ab82 | ff91dd40-1569-4765-a2bc-014321f76ace |

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
| Message                                        | FileUID                              | ProjectUID                           | ErrorCode |
| Failed to get station range for alignment file | dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff | ff91dd40-1569-4765-a2bc-014321f76ace | -4        |
| Unable to access design file.                  | dcb41fbd-7d43-4b36-a144-e22bbccc24a8 | ff91dd40-1569-4765-a2bc-014321f76ace | -1        |
