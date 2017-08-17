Feature: CompactionPalette
  I should be able to request compaction palettes

######################################################## Elevation Palette ######################################################
Scenario Outline: Compaction Get Elevation Palette - No Design Filter
	Given the Compaction service URI "/api/v2/compaction/elevationpalette" for operation "ElevationPalette"
  And the result file "CompactionGetCompactionPalettesResponse.json"
	And projectUid "<ProjectUID>"
	When I request result
  Then the result should match the "<ResultName>" from the repository
	Examples: 
	| RequetsName | ProjectUID                           | ResultName        |
	|             | ff91dd40-1569-4765-a2bc-014321f76ace | NoDesignFilter_EP |

Scenario Outline: Compaction Get Elevation Palette - No Data
	Given the Compaction service URI "/api/v2/compaction/elevationpalette" for operation "ElevationPalette"
  And the result file "CompactionGetCompactionPalettesResponse.json"
	And projectUid "<ProjectUID>"
  And startUtc "<StartUTC>" and endUtc "<EndUTC>"
	When I request result
  Then the result should match the "<ResultName>" from the repository
	Examples: 
	| RequetsName | ProjectUID                           | StartUTC   | EndUTC     | ResultName |
	|             | ff91dd40-1569-4765-a2bc-014321f76ace | 2017-01-01 | 2017-01-01 | NoData_EP  |

####################################################### Compaction Palettes #####################################################
Scenario Outline: Compaction Get Palettes
	Given the Compaction service URI "/api/v2/compaction/colorpalettes" for operation "CompactionPalettes"
  And the result file "CompactionGetCompactionPalettesResponse.json"
	And projectUid "<ProjectUID>"
	When I request result
  Then the result should match the "<ResultName>" from the repository
	Examples: 
	| RequetsName | ProjectUID                           | ResultName           |
	|             | ff91dd40-1569-4765-a2bc-014321f76ace | GoodRequest_Palettes |


 