Feature: CompactionTile
  I should be able to request compaction tiles

###################################################### Production Data Tiles ####################################################
Scenario Outline: Compaction Get Tiles - No Design Filter - Good Request
	Given the Compaction service URI "/api/v2/compaction/productiondatatiles"
  And the result file "CompactionGetProductionDataTilesResponse.json"	
  And projectUid "<ProjectUID>"
	And displayMode "0" and bbox "<BBox>" and width "<Width>" and height "<Height>"
  When I request result
  Then the result should match the "<ResultName>" from the repository
	Examples: 
	| RequestName  | ProjectUID                           | BBox                                                                                        | Width | Height | ResultName  |
	|              | ff91dd40-1569-4765-a2bc-014321f76ace | 36.206964000089840283, -115.0203540002853231, 36.206956000089640213, -115.02034400028509253 | 256   | 256    | GoodRequest |
  | SS Included  | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | 36.207437, -115.019999, 36.207473, -115.019959                                              | 256   | 256    | SSIncluded  |
  | SS Excluded  | 86a42bbf-9d0e-4079-850f-835496d715c5 | 36.207437, -115.019999, 36.207473, -115.019959                                              | 256   | 256    | SSExcluded  |

Scenario Outline: Compaction Get Tiles
  Given the Compaction service URI "/api/v2/compaction/productiondatatiles"
  And the result file "CompactionGetProductionDataTilesResponse.json"
  And projectUid "<ProjectUID>"
	And filterUid "<FilterUID>"
  And displayMode "4" and bbox "<BBox>" and width "<Width>" and height "<Height>"
	When I request result
	Then the result should match the "<ResultName>" from the repository
	Examples: 
  | RequestName      | ProjectUID                           | FilterUID                            | BBox                                           | Width | Height | ResultName       |
  | DesignOutside    | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | 1cf81668-1739-42d5-b068-ea025588796a | 36.207022, -115.020854, 36.207563, -115.018414 | 256   | 64     | DesignOutside    |
#  | DesignIntersects | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | 81422acc-9b0c-401c-9987-0aedbf153f1d | 36.207022, -115.020854, 36.207563, -115.018414 | 256   | 64     | DesignIntersects |
