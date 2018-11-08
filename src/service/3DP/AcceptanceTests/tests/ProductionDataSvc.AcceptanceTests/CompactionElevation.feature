Feature: CompactionElevation
  I should be able to request compaction elevation and project statistics

######################################################## Elevation Range ########################################################
Scenario Outline: Compaction Get Elevation Range
  Given the service route "/api/v2/elevationrange" and result repo "CompactionGetElevationAndProjectStatisticsDataResponse.json"
  And with parameter "projectUid" with value "<ProjectUID>"
  And with parameter "filterUid" with value "<FilterUID>"
  When I send the GET request I expect response code <HttpCode>
  Then the response should match "<ResultName>" from the repository
  Examples: 
  | RequestName      | ProjectUID                           | FilterUID                            | ResultName          | HttpCode |
  | NoDesignFilter   | ff91dd40-1569-4765-a2bc-014321f76ace |                                      | NoDesignFilter_ER   | 200      |
  | NoData           | ff91dd40-1569-4765-a2bc-014321f76ace | 200c7b47-b5e6-48ee-a731-7df6623412da | NoData_ER           | 200      |
  | AutomaticsFilter | ff91dd40-1569-4765-a2bc-014321f76ace | 887f90a6-56b9-4266-9d62-ff99e7d346f0 | AutomaticsFilter_ER | 200      |
  | AlignmentFilter  | ff91dd40-1569-4765-a2bc-014321f76ace | 2811c7c3-d270-4d63-97e2-fc3340bf6c7a | AlignmentFilter     | 200      |

######################################################## Project Statistics #####################################################
Scenario Outline: Compaction Get Project Statistics - Good Request
  Given the service route "/api/v2/projectstatistics" and result repo "CompactionGetElevationAndProjectStatisticsDataResponse.json"
  And with parameter "projectUid" with value "<ProjectUID>"
  When I send the GET request I expect response code <HttpCode>
  Then the response should exactly match "<ResultName>" from the repository
  Examples: 
  | RequestName | ProjectUID                           | ResultName      | HttpCode |
  |             | ff91dd40-1569-4765-a2bc-014321f76ace | GoodRequest_PRS | 200      |

######################################################## Project Extents #####################################################
Scenario Outline: Compaction Get Project Extents - Good Request
  Given the service route "/api/v2/productiondataextents" and result repo "CompactionGetElevationAndProjectStatisticsDataResponse.json"
  And with parameter "projectUid" with value "<ProjectUID>"
  When I send the GET request I expect response code <HttpCode>
  Then the response should match "<ResultName>" from the repository
  Examples: 
  | RequestName | ProjectUID                           | ResultName      | HttpCode |
  |             | ff91dd40-1569-4765-a2bc-014321f76ace | GoodReq_Extents | 200      |

#######################################################Alignment offset test####################################################
Scenario Outline: Compaction Get Alignment Station Range - Good request
  Given the service route "/api/v2/alignmentstationrange" and result repo "CompactionGetElevationAndProjectStatisticsDataResponse.json"
  And with parameter "projectUid" with value "<ProjectUID>"
  And with parameter "alignmentFileUid" with value "<AlignmentFileUid>"
  When I send the GET request I expect response code <HttpCode>
  Then the response should match "<ResultName>" from the repository
  Examples: 
  | ResultName             | AlignmentFileUid                     | ProjectUID                           | HttpCode |
  | Large_Sites_Align_Good | 6ece671b-7959-4a14-86fa-6bfe6ef4dd62 | ff91dd40-1569-4765-a2bc-014321f76ace | 200      |
  | TopCon_Align_Good      | c6662be1-0f94-4897-b9af-28aeeabcd09b | ff91dd40-1569-4765-a2bc-014321f76ace | 200      |
  | Milling_Align_Good     | 3ead0c55-1e1f-4d30-aaf8-873526a2ab82 | ff91dd40-1569-4765-a2bc-014321f76ace | 200      |

Scenario Outline: Compaction Get Alignment Station Range - Bad request
  Given the service route "/api/v2/alignmentstationrange" and result repo "CompactionGetElevationAndProjectStatisticsDataResponse.json"
  And with parameter "projectUid" with value "<ProjectUID>"
  And with parameter "alignmentFileUid" with value "<AlignmentFileUid>"
  When I send the GET request I expect response code <HttpCode>
  Then the response should contain message "<Message>" and code "<ErrorCode>"
  # First example is a design file
  # Second example is a file not belonging to the project
  Examples: 
  | Message                                        | AlignmentFileUid                     | ProjectUID                           | ErrorCode | HttpCode |
  | Failed to get station range for alignment file | dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff | ff91dd40-1569-4765-a2bc-014321f76ace | -4        | 400      |
  | Unable to access design file.                  | dcb41fbd-7d43-4b36-a144-e22bbccc24a8 | ff91dd40-1569-4765-a2bc-014321f76ace | -1        | 400      |
