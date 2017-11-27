﻿Feature: CompactionTile
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
And displayMode "<Mode>" and bbox "<BBox>" and width "<Width>" and height "<Height>"
When I request result
#Then the result should match the "<ResultName>" from the repository
Then the result tile should match the "<ResultName>" from the repository
Examples: 
| RequestName       | ProjectUID                           | FilterUID                            | BBox                                                                            | Width | Height | Mode | ResultName        |
| DesignOutside     | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | 1cf81668-1739-42d5-b068-ea025588796a | 36.207022, -115.020854, 36.207563, -115.018414                                  | 256   | 64     | 4    | DesignOutside     |
| DesignIntersects  | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | 81422acc-9b0c-401c-9987-0aedbf153f1d | 36.207400, -115.020000, 36.207430, -115.020030                                  | 256   | 256    | 4    | DesignIntersects  |
| BoundaryFilterCMV | ff91dd40-1569-4765-a2bc-014321f76ace | a37f3008-65e5-44a8-b406-9a078ec62ece | 36.206883952552914, -115.0203323364258, 36.207160975535146, -115.01998901367188 | 256   | 256    | 27   | BoundaryFilterCMV |
# Ignored until #59587 is fixed (Aaron)
#| BoundaryFilterMDP | ff91dd40-1569-4765-a2bc-014321f76ace | 3ef41e3c-d1f5-40cd-b012-99d11ff432ef | 36.207160975535146, -115.01930236816406, 36.20771501855802, -115.01861572265624 | 256   | 256    | 20   | BoundaryFilterMDP |
| BoundaryFilterELV | ff91dd40-1569-4765-a2bc-014321f76ace | 3ef41e3c-d1f5-40cd-b012-99d11ff432ef | 36.207160975535146, -115.01930236816406, 36.20771501855802, -115.01861572265624 | 256   | 256    | 0    | BoundaryFilterELV |
| BoundaryFilterSPD | ff91dd40-1569-4765-a2bc-014321f76ace | 3ef41e3c-d1f5-40cd-b012-99d11ff432ef | 36.207160975535146, -115.01930236816406, 36.20771501855802, -115.01861572265624 | 256   | 256    | 26   | BoundaryFilterSPD |
| BoundaryFilterPCS | ff91dd40-1569-4765-a2bc-014321f76ace | 3ef41e3c-d1f5-40cd-b012-99d11ff432ef | 36.207160975535146, -115.01930236816406, 36.20771501855802, -115.01861572265624 | 256   | 256    | 14   | BoundaryFilterPCS |


Scenario Outline: Compaction Get Tiles for cutfill
Given the Compaction service URI "/api/v2/compaction/productiondatatiles"
And the result file "CompactionGetProductionDataTilesResponse.json"
And projectUid "<ProjectUID>"
And filterUid "<FilterUID>"
And cutfillDesignUid "<cutfillDesignUid>"
And displayMode "<Mode>" and bbox "<BBox>" and width "<Width>" and height "<Height>"
And a summary volume file with volumeCalcType "<VolCalc>" and a topUid "<TopUid>" and a baseUid "<BaseUid>"
When I request result
Then the result should match the "<ResultName>" from the repository
Examples: 
| RequestName       | ProjectUID                           | FilterUID                            | cutfillDesignUid                     | BBox                                                                          | Width | Height | Mode | VolCalc        | TopUid                               | BaseUid                              | ResultName                 |
| FilterAreaMachine | ff91dd40-1569-4765-a2bc-014321f76ace | 9c27697f-ea6d-478a-a168-ed20d6cd9a20 | dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff | 36.20660692859012, -115.0213623046875, 36.20882309283712, -115.01861572265624 | 256   | 256    | 8    |                |                                      |                                      | BoundaryMachineFilterTiles |
| NoDesign          | ff91dd40-1569-4765-a2bc-014321f76ace | 9c27697f-ea6d-478a-a168-ed20d6cd9a20 |                                      | 36.20660692859012, -115.0213623046875, 36.20882309283712, -115.01861572265624 | 256   | 256    | 8    |                |                                      |                                      | NoDesignTiles              |
| SVGroundToGround1 | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | 81422acc-9b0c-401c-9987-0aedbf153f1d |                                      | 36.20700097553514, -115.0199023681640, 36.20741501855802, -115.01881572265624 | 256   | 256    | 8    | GroundToGround | 9c27697f-ea6d-478a-a168-ed20d6cd9a20 | 81422acc-9b0c-401c-9987-0aedbf153f1d | SummaryVolGroundGround1    |
| SVGroundToDesign  | ff91dd40-1569-4765-a2bc-014321f76ace | 9c27697f-ea6d-478a-a168-ed20d6cd9a20 |                                      | 36.20660692859012, -115.0213623046875, 36.20882309283712, -115.01861572265624 | 256   | 256    | 8    | GroundToDesign | dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff | 9c27697f-ea6d-478a-a168-ed20d6cd9a20 | SummaryVolDesignGround     |
| SVDesignToGround  | ff91dd40-1569-4765-a2bc-014321f76ace | 9c27697f-ea6d-478a-a168-ed20d6cd9a20 |                                      | 36.20660692859012, -115.0213623046875, 36.20882309283712, -115.01861572265624 | 256   | 256    | 8    | DesignToGround | 9c27697f-ea6d-478a-a168-ed20d6cd9a20 | dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff | SummaryVolGroundDesign     |
| SVGroundToGround2 | ff91dd40-1569-4765-a2bc-014321f76ace |                                      |                                      | 36.20660692859012, -115.0213623046875, 36.20882309283712, -115.01861572265624 | 256   | 256    | 8    | GroundToGround | 9c27697f-ea6d-478a-a168-ed20d6cd9a20 | 9c27697f-ea6d-478a-a168-ed20d6cd9a20 | SummaryVolGroundGround2    |
