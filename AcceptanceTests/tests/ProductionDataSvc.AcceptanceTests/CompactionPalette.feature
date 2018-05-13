Feature: CompactionPalette
I should be able to request compaction palettes

######################################################## Elevation Palette ######################################################
Scenario Outline: Compaction Get Elevation Palette - No Design Filter
Given the Compaction service URI "/api/v2/elevationpalette" for operation "ElevationPalette"
And the result file "CompactionGetCompactionPalettesResponse.json"
And projectUid "<ProjectUID>"
When I request result
Then the result should match the "<ResultName>" from the repository
Examples: 
| RequestName | ProjectUID                           | ResultName        |
|             | ff91dd40-1569-4765-a2bc-014321f76ace | NoDesignFilter_EP |

Scenario Outline: Compaction Get Elevation Palette - No Data
Given the Compaction service URI "/api/v2/elevationpalette" for operation "ElevationPalette"
And the result file "CompactionGetCompactionPalettesResponse.json"
And projectUid "<ProjectUID>"
And filterUid "<FilterUid>"
When I request result
Then the result should match the "<ResultName>" from the repository
Examples: 
| RequestName | ProjectUID                           | FilterUid                            | ResultName |
|             | ff91dd40-1569-4765-a2bc-014321f76ace | 200c7b47-b5e6-48ee-a731-7df6623412da | NoData_EP  |

####################################################### Compaction Palettes #####################################################
Scenario Outline: Compaction Get Palettes
Given the Compaction service URI "/api/v2/colorpalettes" for operation "CompactionPalettes"
And the result file "CompactionGetCompactionPalettesResponse.json"
And projectUid "<ProjectUID>"
When I request result
Then the result should match the "<ResultName>" from the repository
Examples: 
| RequestName | ProjectUID                           | ResultName           |
|             | ff91dd40-1569-4765-a2bc-014321f76ace | GoodRequest_Palettes |


 