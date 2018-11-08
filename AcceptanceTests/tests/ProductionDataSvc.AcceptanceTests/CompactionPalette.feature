Feature: CompactionPalette
  I should be able to request compaction palettes

######################################################## Elevation Palette ######################################################
Scenario Outline: Compaction Get Elevation Palette - No Design Filter
  Given the service route "/api/v2/elevationpalette" and result repo "CompactionGetCompactionPalettesResponse.json"
  And with parameter "projectUid" with value "<ProjectUID>"
  When I send the GET request I expect response code <HttpCode>
  Then the response should match "<ResultName>" from the repository
  Examples: 
  | RequestName | ProjectUID                           | ResultName        | HttpCode |
  |             | ff91dd40-1569-4765-a2bc-014321f76ace | NoDesignFilter_EP | 200      |

Scenario Outline: Compaction Get Elevation Palette - No Data
  Given the service route "/api/v2/elevationpalette" and result repo "CompactionGetCompactionPalettesResponse.json"
  And with parameter "projectUid" with value "<ProjectUID>"
  And with parameter "filterUid" with value "<FilterUID>"
  When I send the GET request I expect response code <HttpCode>
  Then the response should match "<ResultName>" from the repository
  Examples: 
  | RequestName | ProjectUID                           | FilterUID                            | ResultName | HttpCode |
  |             | ff91dd40-1569-4765-a2bc-014321f76ace | 200c7b47-b5e6-48ee-a731-7df6623412da | NoData_EP  | 200      |

####################################################### Compaction Palettes #####################################################
Scenario Outline: Compaction Get Palettes
  Given the service route "/api/v2/colorpalettes" and result repo "CompactionGetCompactionPalettesResponse.json"
  And with parameter "projectUid" with value "<ProjectUID>"
  When I send the GET request I expect response code <HttpCode>
  Then the response should match "<ResultName>" from the repository
  Examples: 
  | RequestName | ProjectUID                           | ResultName           | HttpCode |
  |             | ff91dd40-1569-4765-a2bc-014321f76ace | GoodRequest_Palettes | 200      |
