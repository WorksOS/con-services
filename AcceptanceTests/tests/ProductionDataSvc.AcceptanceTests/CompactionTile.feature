﻿Feature: CompactionTile
I should be able to request compaction tiles

###################################################### Production Data Tiles ####################################################
Scenario Outline: Get Tiles No Design Filter
Given the Compaction service URI "/api/v2/productiondatatiles"
And the result file "CompactionGetProductionDataTilesResponse.json"	
And projectUid "<ProjectUID>"
And displayMode "0" and bbox "<BBox>" and width "<Width>" and height "<Height>"
When I request result
Then the result tile should match the "<ResultName>" from the repository within "<Difference>" percent
Examples: 
| RequestName | ProjectUID                           | BBox                                                                                        | Width | Height | ResultName  | Difference |
|             | ff91dd40-1569-4765-a2bc-014321f76ace | 36.206964000089840283, -115.0203540002853231, 36.206956000089640213, -115.02034400028509253 | 256   | 256    | GoodRequest | 1          |
| SS Included | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | 36.207437, -115.019999, 36.207473, -115.019959                                              | 256   | 256    | SSIncluded  | 1          |
| SS Excluded | 86a42bbf-9d0e-4079-850f-835496d715c5 | 36.207437, -115.019999, 36.207473, -115.019959                                              | 256   | 256    | SSExcluded  | 1          |

Scenario Outline: Get Tiles
Given the Compaction service URI "/api/v2/productiondatatiles"
And the result file "CompactionGetProductionDataTilesResponse.json"
And projectUid "<ProjectUID>"
And filterUid "<FilterUID>"
And displayMode "<Mode>" and bbox "<BBox>" and width "<Width>" and height "<Height>"
When I request result
Then the result tile should match the "<ResultName>" from the repository within "<Difference>" percent
Examples: 
| RequestName        | ProjectUID                           | FilterUID                            | BBox                                                                            | Width | Height | Mode | ResultName         | Difference |
| DesignOutside      | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | 1cf81668-1739-42d5-b068-ea025588796a | 36.207022, -115.020854, 36.207563, -115.018414                                  | 256   | 64     | 4    | DesignOutside      | 1          |
| DesignIntersects   | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | 81422acc-9b0c-401c-9987-0aedbf153f1d | 36.207400, -115.020000, 36.207430, -115.020030                                  | 256   | 256    | 4    | DesignIntersects   | 1          |
| BoundaryFilterELV  | ff91dd40-1569-4765-a2bc-014321f76ace | 3ef41e3c-d1f5-40cd-b012-99d11ff432ef | 36.207160975535146, -115.01930236816406, 36.20771501855802, -115.01861572265624 | 256   | 256    | 0    | BoundaryFilterELV  | 3          |
| BoundaryFilterPCS  | ff91dd40-1569-4765-a2bc-014321f76ace | 3ef41e3c-d1f5-40cd-b012-99d11ff432ef | 36.207160975535146, -115.01930236816406, 36.20771501855802, -115.01861572265624 | 256   | 256    | 14   | BoundaryFilterPCS  | 3          |
| BoundaryFilterPCD  | ff91dd40-1569-4765-a2bc-014321f76ace | 3ef41e3c-d1f5-40cd-b012-99d11ff432ef | 36.207160975535146, -115.01930236816406, 36.20771501855802, -115.01861572265624 | 256   | 256    | 4    | BoundaryFilterPCD  | 3          |
| BoundaryFilterCMV  | ff91dd40-1569-4765-a2bc-014321f76ace | a37f3008-65e5-44a8-b406-9a078ec62ece | 36.206883952552914, -115.0203323364258, 36.207160975535146, -115.01998901367188 | 256   | 256    | 27   | BoundaryFilterCMV  | 3          |
| BoundaryFilterCMVD | ff91dd40-1569-4765-a2bc-014321f76ace | a37f3008-65e5-44a8-b406-9a078ec62ece | 36.206883952552914, -115.0203323364258, 36.207160975535146, -115.01998901367188 | 256   | 256    | 1    | BoundaryFilterCMVD | 3          |
| BoundaryFilterCMVS | ff91dd40-1569-4765-a2bc-014321f76ace | a37f3008-65e5-44a8-b406-9a078ec62ece | 36.206883952552914, -115.0203323364258, 36.207160975535146, -115.01998901367188 | 256   | 256    | 13   | BoundaryFilterCMVS | 3          |
| BoundaryFilterMDP  | ff91dd40-1569-4765-a2bc-014321f76ace | 3ef41e3c-d1f5-40cd-b012-99d11ff432ef | 36.207160975535146, -115.01930236816406, 36.20771501855802, -115.01861572265624 | 256   | 256    | 20   | BoundaryFilterMDP1 | 4          |
| BoundaryFilterSPD  | ff91dd40-1569-4765-a2bc-014321f76ace | 3ef41e3c-d1f5-40cd-b012-99d11ff432ef | 36.207160975535146, -115.01930236816406, 36.20771501855802, -115.01861572265624 | 256   | 256    | 26   | BoundaryFilterSPD  | 4          |
| BoundaryFilterTMP  | ff91dd40-1569-4765-a2bc-014321f76ace | a37f3008-65e5-44a8-b406-9a078ec62ece | 36.206883952552914, -115.0203323364258, 36.207160975535146, -115.01998901367188 | 256   | 256    | 10   | BoundaryFilterTMP  | 3          |
| BdryMDPAsAtToday   | ff91dd40-1569-4765-a2bc-014321f76ace | cefd0bda-53e4-45bf-a2b9-ca0cf6f6907a | 36.207160975535146, -115.01930236816406, 36.20771501855802, -115.01861572265624 | 256   | 256    | 20   | BoundaryFilterMDP  | 4          |
| ElevAsAtCustom     | ff91dd40-1569-4765-a2bc-014321f76ace | a8405aca-71f1-463d-8821-c2415d67e78c | 36.207160975535146, -115.01930236816406, 36.20771501855802, -115.01861572265624 | 256   | 256    | 0    | AsAtCustomELV      | 1          |
#| BdryELVWithPCRange | ff91dd40-1569-4765-a2bc-014321f76ace | bc29dd86-015f-4e84-a29f-cbc0a2add277 | 36.207160975535146, -115.01930236816406, 36.20771501855802, -115.01861572265624 | 256   | 256    | 0    | BdryELVWithPCRange | 3          |
#| BdryCMVWithPCRange | ff91dd40-1569-4765-a2bc-014321f76ace | 026cabf4-f1b2-4211-a3df-8a314e365e80 | 36.206883952552914, -115.0203323364258, 36.207160975535146, -115.01998901367188 | 256   | 256    | 27   | BdryCMVWithPCRange | 3          |
#| BdryPCDWithPCRange | ff91dd40-1569-4765-a2bc-014321f76ace | bc29dd86-015f-4e84-a29f-cbc0a2add277 | 36.207160975535146, -115.01930236816406, 36.20771501855802, -115.01861572265624 | 256   | 256    | 4    | BdryPCDWithPCRange | 3          |
#| BdryTMPWithPCRange | ff91dd40-1569-4765-a2bc-014321f76ace | 026cabf4-f1b2-4211-a3df-8a314e365e80 | 36.206883952552914, -115.0203323364258, 36.207160975535146, -115.01998901367188 | 256   | 256    | 10   | BdryTMPWithPCRange | 3          |

Scenario Outline: Get CutFill Tiles
Given the Compaction service URI "/api/v2/productiondatatiles"
And the result file "CompactionGetProductionDataTilesResponse.json"
And projectUid "<ProjectUID>"
And filterUid "<FilterUID>"
And cutfillDesignUid "<cutfillDesignUid>"
And displayMode "<Mode>" and bbox "<BBox>" and width "<Width>" and height "<Height>"
And a summary volume file with volumeCalcType "<VolCalc>" and a topUid "<TopUid>" and a baseUid "<BaseUid>" 
When I request result
Then the result tile should match the "<ResultName>" from the repository within "<Difference>" percent
Examples: 
| RequestName                       | ProjectUID                           | FilterUID                            | cutfillDesignUid                     | BBox                                                                            | Width | Height | Mode | VolCalc        | TopUid                               | BaseUid                              | ResultName                 | Difference |
| FilterAreaMachine                 | ff91dd40-1569-4765-a2bc-014321f76ace | 9c27697f-ea6d-478a-a168-ed20d6cd9a20 | dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff | 36.207160975535146, -115.01930236816406, 36.20771501855802, -115.01861572265624 | 256   | 256    | 8    |                |                                      |                                      | BoundaryMachineFilterTiles | 5          |
| NoDesign                          | ff91dd40-1569-4765-a2bc-014321f76ace | 9c27697f-ea6d-478a-a168-ed20d6cd9a20 |                                      | 36.20660692859012, -115.0213623046875, 36.20882309283712, -115.01861572265624   | 256   | 256    | 8    |                |                                      |                                      | NoDesignTiles              | 5          |
| SVGroundToDesignEarliest          | ff91dd40-1569-4765-a2bc-014321f76ace | 9c27697f-ea6d-478a-a168-ed20d6cd9a20 |                                      | 36.20660692859012, -115.0213623046875, 36.20882309283712, -115.01861572265624   | 256   | 256    | 8    | GroundToDesign | dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff | 9c27697f-ea6d-478a-a168-ed20d6cd9a22 | SummaryVolDesignGround     | 5          |
| SVDesignToGroundLatest            | ff91dd40-1569-4765-a2bc-014321f76ace | 9c27697f-ea6d-478a-a168-ed20d6cd9a20 |                                      | 36.20660692859012, -115.0213623046875, 36.20882309283712, -115.01861572265624   | 256   | 256    | 8    | DesignToGround | 9c27697f-ea6d-478a-a168-ed20d6cd9a20 | dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff | SummaryVolGroundDesign     | 5          |
| SVGroundToGround1                 | ff91dd40-1569-4765-a2bc-014321f76ace | 9c27697f-ea6d-478a-a168-ed20d6cd9a20 |                                      | 36.20700097553514, -115.0199023681640, 36.20741501855802, -115.01881572265624   | 256   | 256    | 8    | GroundToGround | 9c27697f-ea6d-478a-a168-ed20d6cd9a21 | 9c27697f-ea6d-478a-a168-ed20d6cd9a20 | SummaryVolGroundGround1    | 5          |
| SVGroundToGround2                 | ff91dd40-1569-4765-a2bc-014321f76ace | 9c27697f-ea6d-478a-a168-ed20d6cd9a20 |                                      | 36.20700097553514, -115.0199023681640, 36.20741501855802, -115.01881572265624   | 256   | 256    | 8    | GroundToGround | 9c27697f-ea6d-478a-a168-ed20d6cd9a20 | 9c27697f-ea6d-478a-a168-ed20d6cd9a21 | SummaryVolGroundGround2    | 5          |
| SVGroundToGround3AdjustBaseFilter | ff91dd40-1569-4765-a2bc-014321f76ace | 9244d3f1-af2b-41ed-aa16-5a776278b6eb |                                      | 36.20700097553514, -115.0199023681640, 36.20741501855802, -115.01881572265624   | 256   | 256    | 8    | GroundToGround | 279ed62b-06a2-4184-ab14-dd7462dcc8c1 | 9244d3f1-af2b-41ed-aa16-5a776278b6eb | SummaryVolGroundGround3    | 5          |
#| GDEarliestWithPCRange             | ff91dd40-1569-4765-a2bc-014321f76ace | 9c27697f-ea6d-478a-a168-ed20d6cd9a20 |                                      | 36.20660692859012, -115.0213623046875, 36.20882309283712, -115.01861572265624   | 256   | 256    | 8    | GroundToDesign | dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff | 5a130d7c-a79b-433d-a04a-094b07cfc1dd | GDEarliestWithPCRange      | 5          |
#| DGLatestWithPCRange               | ff91dd40-1569-4765-a2bc-014321f76ace | 9c27697f-ea6d-478a-a168-ed20d6cd9a20 |                                      | 36.20660692859012, -115.0213623046875, 36.20882309283712, -115.01861572265624   | 256   | 256    | 8    | DesignToGround | b06996e4-4944-4d84-b2c7-e1808dd7d7d7 | dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff | DGLatestWithPCRange        | 5          |
#| GGroundWithPCRange2               | ff91dd40-1569-4765-a2bc-014321f76ace | 9c27697f-ea6d-478a-a168-ed20d6cd9a20 |                                      | 36.20700097553514, -115.0199023681640, 36.20741501855802, -115.01881572265624   | 256   | 256    | 8    | GroundToGround | 9c27697f-ea6d-478a-a168-ed20d6cd9a20 | b06996e4-4944-4d84-b2c7-e1808dd7d7d7 | GGroundWithPCRange2        | 5          |
#| GGroundWithPCRange1               | ff91dd40-1569-4765-a2bc-014321f76ace | 9c27697f-ea6d-478a-a168-ed20d6cd9a20 |                                      | 36.20700097553514, -115.0199023681640, 36.20741501855802, -115.01881572265624   | 256   | 256    | 8    | GroundToGround | b06996e4-4944-4d84-b2c7-e1808dd7d7d7 | 9c27697f-ea6d-478a-a168-ed20d6cd9a20 | GGroundWithPCRange1        | 5          |
