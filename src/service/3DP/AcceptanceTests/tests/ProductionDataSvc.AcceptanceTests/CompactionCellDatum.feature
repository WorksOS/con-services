Feature: CompactionCellDatum
  I should be able to request compaction Cell Datum.

Scenario Outline: CompactionCellDatum - Good Request 
  Given the service route "/api/v2/productiondata/cells/datum" and result repo "CompactionCellDatumResponse.json"
  And with parameter "projectUid" with value "<ProjectUID>"
  And with parameter "filterUid" with value "<FilterUID>"
  And with parameter "cutfillDesignUid" with value "<CutFillDesignUID>"
  And with parameter "displayMode" with value "<DisplayMode>"
  And with parameter "lat" with value "<Latitude>"
  And with parameter "lon" with value "<Longitude>"
  When I send the GET request I expect response code <HttpCode>
  Then the response should match "<ResultName>" from the repository
  Examples: 
  | RequestName         | ProjectUID                           | FilterUID                            | CutFillDesignUID                     | DisplayMode | Latitude    | Longitude     | ResultName          | HttpCode |
  | Height              | ff91dd40-1569-4765-a2bc-014321f76ace |                                      |                                      | 0           | 36.20696541 | -115.02021047 | Height              | 200      |
  | CMV                 | ff91dd40-1569-4765-a2bc-014321f76ace |                                      |                                      | 1           | 36.20696541 | -115.02021047 | CMV                 | 200      |
  | CMVPercent          | ff91dd40-1569-4765-a2bc-014321f76ace |                                      |                                      | 2           | 36.20696541 | -115.02021047 | CMVPercent          | 200      |
  | PassCount           | ff91dd40-1569-4765-a2bc-014321f76ace |                                      |                                      | 4           | 36.20696541 | -115.02021047 | PassCount           | 200      |
  | PassCount2          | ff91dd40-1569-4765-a2bc-014321f76ace |                                      |                                      | 4           | 36.207105   | -115.018953   | PassCount2          | 200      |
  | CutFill             | ff91dd40-1569-4765-a2bc-014321f76ace |                                      |                                      | 8           | 36.20735707 | -115.01959313 | CutFill             | 200      |
  | Temperature         | ff91dd40-1569-4765-a2bc-014321f76ace |                                      |                                      | 10          | 36.20696541 | -115.02021047 | Temperature         | 200      |
  | MDP                 | ff91dd40-1569-4765-a2bc-014321f76ace |                                      |                                      | 17          | 36.207499   | -115.018843   | MDP                 | 200      |
  | MDPBoundary         | ff91dd40-1569-4765-a2bc-014321f76ace | 3ef41e3c-d1f5-40cd-b012-99d11ff432ef |                                      | 17          | 36.207499   | -115.018843   | MDP2                | 200      |
  | MachineSpeed        | ff91dd40-1569-4765-a2bc-014321f76ace |                                      |                                      | 23          | 36.20696541 | -115.02021047 | MachineSpeed        | 200      |
  | CMVPercentChange    | ff91dd40-1569-4765-a2bc-014321f76ace |                                      |                                      | 27          | 36.206958   | -115.020144   | CMVPercentChange    | 200      |
  | HeightOutsideFilter | ff91dd40-1569-4765-a2bc-014321f76ace | 154470b6-15ae-4cca-b281-eae8ac1efa6c |                                      | 0           | 36.20696541 | -115.02021047 | HeightOutsideFilter | 200      |
  | CutFillWithDesign   | ff91dd40-1569-4765-a2bc-014321f76ace |                                      | dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff | 8           | 36.20735707 | -115.01959313 | CutFillWithDesign   | 200      |
  | PassCountFilter     | ff91dd40-1569-4765-a2bc-014321f76ace | c5590172-a1bb-440a-bc7d-6c35ecc75724 |                                      | 4           | 36.20696541 | -115.02021047 | PassCountFilter     | 200      |
  | MachineSpeedFilter  | ff91dd40-1569-4765-a2bc-014321f76ace | c5590172-a1bb-440a-bc7d-6c35ecc75724 |                                      | 23          | 36.20696541 | -115.02021047 | MachineSpeedFilter  | 200      |
  | HeightFilter        | ff91dd40-1569-4765-a2bc-014321f76ace | c5590172-a1bb-440a-bc7d-6c35ecc75724 |                                      | 0           | 36.20696541 | -115.02021047 | HeightFilter        | 200      |

Scenario Outline: CompactionCellDatum - Bad Request 
  Given the service route "/api/v2/productiondata/cells/datum" and result repo "CompactionCellDatumResponse.json"
  And with parameter "projectUid" with value "<ProjectUID>"
  And with parameter "filterUid" with value "<FilterUID>"
  And with parameter "cutfillDesignUid" with value "<CutFillDesignUID>"
  And with parameter "displayMode" with value "<DisplayMode>"
  And with parameter "lat" with value "<Latitude>"
  And with parameter "lon" with value "<Longitude>"
  When I send the GET request I expect response code <HttpCode>
  Then the response should contain code <errorCode>
  Examples: 
  | RequestName   | ProjectUID                           | FilterUID                            | CutFillDesignUID                     | DisplayMode | Latitude    | Longitude     | HttpCode | errorCode |
  | NoProjectUID  |                                      |                                      |                                      | 0           | 36.20696541 | -115.02021047 | 400      | -1        |  
  | NoFoundFilter | ff91dd40-1569-4765-a2bc-014321f76ace | 7f2fb9ec-2384-420e-b2e3-72b9cea939a3 |                                      | 0           | 36.20696541 | -115.02021047 | 400      | -1        |
  | NoFoundDesign | ff91dd40-1569-4765-a2bc-014321f76ace |                                      | 3d255208-8aa2-4172-9046-f97a36eff896 | 8           | 36.20696541 | -115.02021047 | 400      | -1        |
    