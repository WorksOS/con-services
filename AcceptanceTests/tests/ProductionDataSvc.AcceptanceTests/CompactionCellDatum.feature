Feature: CompactionCellDatum
I should be able to request compaction Cell Datum.

Background: 
	Given the CompactionCellDatum service URI "/api/v2/productiondata/cells/datum" and result repo "CompactionCellDatumResponse.json"

Scenario Outline: CompactionCellDatum - Good Request 
And projectUid "<ProjectUID>"
And filterUid "<FilterUID>"
And cutfillDesignUid "<CutFillDesignUID>"
And displayMode "<DisplayMode>"
And lat "<Latitude>"
And lon "<Longitude>"
When I request Compaction Cell Datum
Then the Compaction Cell Datum response should match "<ResultName>" result from the repository
Examples: 
| RequestName         | ProjectUID                           | FilterUID                            | CutFillDesignUID                     | DisplayMode | Latitude    | Longitude     | ResultName          |
| Height              | ff91dd40-1569-4765-a2bc-014321f76ace |                                      |                                      | 0           | 36.20696541 | -115.02021047 | Height              |
| CMV                 | ff91dd40-1569-4765-a2bc-014321f76ace |                                      |                                      | 1           | 36.20696541 | -115.02021047 | CMV                 |
| CMVPercent          | ff91dd40-1569-4765-a2bc-014321f76ace |                                      |                                      | 2           | 36.20696541 | -115.02021047 | CMVPercent          |
| PassCount           | ff91dd40-1569-4765-a2bc-014321f76ace |                                      |                                      | 4           | 36.20696541 | -115.02021047 | PassCount           |
| PassCount2          | ff91dd40-1569-4765-a2bc-014321f76ace |                                      |                                      | 4           | 36.207105   | -115.018953   | PassCount2          |
| CutFill             | ff91dd40-1569-4765-a2bc-014321f76ace |                                      |                                      | 8           | 36.20735707 | -115.01959313 | CutFill             |
| Temperature         | ff91dd40-1569-4765-a2bc-014321f76ace |                                      |                                      | 10          | 36.20696541 | -115.02021047 | Temperature         |
| MDP                 | ff91dd40-1569-4765-a2bc-014321f76ace |                                      |                                      | 17          | 36.207499   | -115.018843   | MDP                 |
| MDPBoundary         | ff91dd40-1569-4765-a2bc-014321f76ace | 3ef41e3c-d1f5-40cd-b012-99d11ff432ef |                                      | 17          | 36.207499   | -115.018843   | MDP2                |
| MachineSpeed        | ff91dd40-1569-4765-a2bc-014321f76ace |                                      |                                      | 23          | 36.20696541 | -115.02021047 | MachineSpeed        |
| CMVPercentChange    | ff91dd40-1569-4765-a2bc-014321f76ace |                                      |                                      | 24          | 36.20696541 | -115.02021047 | CMVPercentChange    |
| HeightOutsideFilter | ff91dd40-1569-4765-a2bc-014321f76ace | 154470b6-15ae-4cca-b281-eae8ac1efa6c |                                      | 0           | 36.20696541 | -115.02021047 | HeightOutsideFilter |
| CutFillWithDesign   | ff91dd40-1569-4765-a2bc-014321f76ace |                                      | dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff | 8           | 36.20735707 | -115.01959313 | CutFillWithDesign   |

Scenario Outline: CompactionCellDatum - Bad Request 
And projectUid "<ProjectUID>"
And filterUid "<FilterUID>"
And cutfillDesignUid "<CutFillDesignUID>"
And displayMode "<DisplayMode>"
And lat "<Latitude>"
And lon "<Longitude>"
When I request Compaction Cell Datum I expect http error code <httpCode>
Then the response should contain error code <errorCode>
Examples: 
| RequestName   | ProjectUID                           | FilterUID                            | CutFillDesignUID                     | DisplayMode | Latitude    | Longitude     | httpCode | errorCode |
| NoProjectUID  |                                      |                                      |                                      | 0           | 36.20696541 | -115.02021047 | 400      | -1        |  
#| NoFoundFilter | ff91dd40-1569-4765-a2bc-014321f76ace | 7f2fb9ec-2384-420e-b2e3-72b9cea939a3 |                                      | 0           | 36.20696541 | -115.02021047 | 400      | -1        |
| NoFoundDesign | ff91dd40-1569-4765-a2bc-014321f76ace |                                      | 3d255208-8aa2-4172-9046-f97a36eff896 | 8           | 36.20696541 | -115.02021047 | 400      | -1        |
    