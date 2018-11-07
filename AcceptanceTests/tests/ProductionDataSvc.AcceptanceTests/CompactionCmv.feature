Feature: CompactionCmv
I should be able to request compaction CMV data

######################################################## CMV Summary ############################################################
Scenario Outline: Compaction Get CMV Summary - No Design Filter
  Given the service route "/api/v2/cmv/summary" and result repo "CompactionGetCMVDataResponse.json"
  And with parameter "projectUid" with value "<ProjectUID>"
  When I send the GET request I expect response code <HttpCode>
  Then the response should match "<ResultName>" from the repository
  Examples: 
  | RequestName     | ProjectUID                           | ResultName                | HttpCode |
  |                 | ff91dd40-1569-4765-a2bc-014321f76ace | NoDesignFilter_Summary    | 200      |
  | ProjectSettings | 3335311a-f0e2-4dbe-8acd-f21135bafee4 | NoDesignFilter_Summary_PS | 200      |

Scenario Outline: Compaction Get CMV Summary
  Given the service route "/api/v2/cmv/summary" and result repo "CompactionGetCMVDataResponse.json"
  And with parameter "projectUid" with value "<ProjectUID>"
  And with parameter "filterUid" with value "<FilterUID>"
  When I send the GET request I expect response code <HttpCode>
  Then the response should match "<ResultName>" from the repository
  Examples: 
  | RequestName       | ProjectUID                           | FilterUID                            | ResultName                | HttpCode |
  | DesignOutside     | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | 1cf81668-1739-42d5-b068-ea025588796a | DesignOutside_Summary     | 200      |
  | DesignIntersects  | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | 3d9086f2-3c04-4d92-9141-5134932b1523 | DesignIntersects_Summary  | 200      |
  | FilterArea        | ff91dd40-1569-4765-a2bc-014321f76ace | a37f3008-65e5-44a8-b406-9a078ec62ece | BoundaryFilter_Summary    | 200      |
  | AsAtToday         | ff91dd40-1569-4765-a2bc-014321f76ace | c638018c-5026-44be-af0b-006ecad65462 | BoundaryFilter_Summary    | 200      |
  | AsAtCustom        | ff91dd40-1569-4765-a2bc-014321f76ace | a8405aca-71f1-463d-8821-c2415d67e78c | AsAtCustom_Summary        | 200      |
  | TemperatureFilter | ff91dd40-1569-4765-a2bc-014321f76ace | 1980fc8b-c892-4f9f-b673-bc09827bf2b5 | TemperatureFilter_Summary | 200      |
  | PassCountFilter   | ff91dd40-1569-4765-a2bc-014321f76ace | c5590172-a1bb-440a-bc7d-6c35ecc75724 | PassCountFilter_Summary   | 200      |
  | AutomaticsFilter  | ff91dd40-1569-4765-a2bc-014321f76ace | 887f90a6-56b9-4266-9d62-ff99e7d346f0 | AutomaticsFilter_Summary  | 200      |

Scenario Outline: Compaction Get CMV Summary - No Data
  Given the service route "/api/v2/cmv/summary" and result repo "CompactionGetCMVDataResponse.json"
  And with parameter "projectUid" with value "<ProjectUID>"
  And with parameter "filterUid" with value "<FilterUID>"
  When I send the GET request I expect response code <HttpCode>
  Then the response should match "<ResultName>" from the repository
  Examples: 
  | RequestName     | ProjectUID                           | FilterUID                            | ResultName             | HttpCode |
  | AlignmentFilter | ff91dd40-1569-4765-a2bc-014321f76ace | 2811c7c3-d270-4d63-97e2-fc3340bf6c7a | AlignmentFilter_NoData | 200      |

######################################################## CMV Details ############################################################
Scenario Outline: Compaction Get CMV Details - No Design Filter
  Given the service route "/api/v2/cmv/details/targets" and result repo "CompactionGetCMVDataResponse.json"
  And with parameter "projectUid" with value "<ProjectUID>"
  When I send the GET request I expect response code <HttpCode>
  Then the response should match "<ResultName>" from the repository
  Examples: 
  | RequestName     | ProjectUID                           | ResultName                | HttpCode |
  |                 | ff91dd40-1569-4765-a2bc-014321f76ace | NoDesignFilter_Details    | 200      |
  | ProjectSettings | 3335311a-f0e2-4dbe-8acd-f21135bafee4 | NoDesignFilter_Details_PS | 200      |

Scenario Outline: Compaction Get CMV Details
  Given the service route "/api/v2/cmv/details/targets" and result repo "CompactionGetCMVDataResponse.json"
  And with parameter "projectUid" with value "<ProjectUID>"
  And with parameter "filterUid" with value "<FilterUID>"
  When I send the GET request I expect response code <HttpCode>
  Then the response should match "<ResultName>" from the repository
  Examples: 
  | RequestName      | ProjectUID                           | FilterUID                            | ResultName               | HttpCode |
  | DesignOutside    | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | 1cf81668-1739-42d5-b068-ea025588796a | DesignOutside_Details    | 200      |
  | DesignIntersects | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | 3d9086f2-3c04-4d92-9141-5134932b1523 | DesignIntersects_Details | 200      |

######################################################## CMV Details Extended ###################################################
Scenario Outline: Compaction Get CMV Details Extended - No Design Filter
  Given the service route "/api/v2/cmv/details" and result repo "CompactionGetCMVDataResponse.json"
  And with parameter "projectUid" with value "<ProjectUID>"
  When I send the GET request I expect response code <HttpCode>
  Then the response should match "<ResultName>" from the repository
  Examples: 
  | RequestName            | ProjectUID                           | ResultName                            | HttpCode |
  |                        | ff91dd40-1569-4765-a2bc-014321f76ace | Ext_NoDesignFilter_Details            | 200      |
  | ProjectSettingsDefault | 3335311a-f0e2-4dbe-8acd-f21135bafee4 | Ext_NoDesignFilter_Details_PS_Default | 200      |
  | ProjectSettings        | 86a42bbf-9d0e-4079-850f-835496d715c5 | Ext_NoDesignFilter_Details_PS         | 200      |

Scenario Outline: Compaction Get CMV Details Extended
  Given the service route "/api/v2/cmv/details" and result repo "CompactionGetCMVDataResponse.json"
  And with parameter "projectUid" with value "<ProjectUID>"
  And with parameter "filterUid" with value "<FilterUID>"
  When I send the GET request I expect response code <HttpCode>
  Then the response should match "<ResultName>" from the repository
  Examples: 
  | RequestName      | ProjectUID                           | FilterUID                            | ResultName                   | HttpCode |
  | DesignOutside    | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | 1cf81668-1739-42d5-b068-ea025588796a | Ext_DesignOutside_Details    | 200      |
  | DesignIntersects | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | 3d9086f2-3c04-4d92-9141-5134932b1523 | Ext_DesignIntersects_Details | 200      |

######################################################## CMV % Change Summary ###################################################
Scenario Outline: Compaction Get CMV % Change Summary - No Design Filter
  Given the service route "/api/v2/cmv/percentchange" and result repo "CompactionGetCMVDataResponse.json"
  And with parameter "projectUid" with value "<ProjectUID>"
  When I send the GET request I expect response code <HttpCode>
  Then the response should match "<ResultName>" from the repository
  Examples: 
  | RequestName     | ProjectUID                           | ResultName                      | HttpCode |
  |                 | ff91dd40-1569-4765-a2bc-014321f76ace | NoDesignFilter_PercentChange    | 200      |
  | ProjectSettings | 3335311a-f0e2-4dbe-8acd-f21135bafee4 | NoDesignFilter_PercentChange_PS | 200      |

Scenario Outline: Compaction Get CMV % Change Summary
  Given the service route "/api/v2/cmv/percentchange" and result repo "CompactionGetCMVDataResponse.json"
  And with parameter "projectUid" with value "<ProjectUID>"
  And with parameter "filterUid" with value "<FilterUID>"
  When I send the GET request I expect response code <HttpCode>
  Then the response should match "<ResultName>" from the repository
  Examples: 
  | RequestName       | ProjectUID                           | FilterUID                            | ResultName                             | HttpCode |
  | DesignOutside     | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | 1cf81668-1739-42d5-b068-ea025588796a | DesignOutside_PercentChangeSummary     | 200      |
  | DesignIntersects  | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | 3d9086f2-3c04-4d92-9141-5134932b1523 | DesignIntersects_PercentChangeSummary  | 200      |
  | FilterArea        | ff91dd40-1569-4765-a2bc-014321f76ace | a37f3008-65e5-44a8-b406-9a078ec62ece | BoundaryFilter_PercentChangeSummary    | 200      |
  | AsAtToday         | ff91dd40-1569-4765-a2bc-014321f76ace | c638018c-5026-44be-af0b-006ecad65462 | BoundaryFilter_PercentChangeSummary    | 200      |
  | AsAtCustom        | ff91dd40-1569-4765-a2bc-014321f76ace | a8405aca-71f1-463d-8821-c2415d67e78c | AsAtCustom_PercentChangeSummary        | 200      |
  | TemperatureFilter | ff91dd40-1569-4765-a2bc-014321f76ace | 1980fc8b-c892-4f9f-b673-bc09827bf2b5 | TemperatureFilter_PercentChangeSummary | 200      |
  | PassCountFilter   | ff91dd40-1569-4765-a2bc-014321f76ace | c5590172-a1bb-440a-bc7d-6c35ecc75724 | PassCountFilter_PercentChangeSummary   | 200      |
