Feature: PassCountDetail
  I should be able to request Pass Count Detail.

Scenario Outline: PassCountDetail - Good Request
  Given the service route "/api/v1/compaction/passcounts/detailed" request repo "PassCountDetailRequest.json" and result repo "PassCountDetailResponse.json"
  When I POST with parameter "<ParameterName>" I expect response code <HttpCode>
  Then the response should match "<ResultName>" from the repository
  Examples: 
  | ParameterName              | ResultName                 | HttpCode |
  | CommonSetting              | CommonSetting              | 200      |
  | NonContiguousSetting       | NonContiguousSetting       | 200      |
  | SingleSetting              | SingleSetting              | 200      |
  | FilterByFineGPS            | FilterByFineGPS            | 200      |
  | FilterByFineGPSInc         | FilterByFineGPSInc         | 200      |
  | FilterByMediumGPS          | FilterByMediumGPS          | 200      |
  | FilterByMediumGPSInc       | FilterByMediumGPSInc       | 200      |
  | FilterByCoarseGPS          | FilterByCoarseGPS          | 200      |
  | FilterByCoarseGPSInc       | FilterByCoarseGPSInc       | 200      |
  | FilterByAllGPS             | FilterByAllGPS             | 200      |
  | FilterByOnMachineDesignId  | FilterByOnMachineDesignId  | 200      |
  | FilterByVibrationOn        | FilterByVibrationOn        | 200      |
  | FilterByForward            | FilterByForward            | 200      |
  | FilterByReverse            | FilterByReverse            | 200      |
  | FilterByOffsetFromDesign   | FilterByOffsetFromDesign   | 200      |
  | FilterByOffsetFromBench    | FilterByOffsetFromBench    | 200      |
  | FilterByMachines           | FilterByMachines           | 200      |
  | LiftDetectTypeAutoMapReset | LiftDetectTypeAutoMapReset | 200      |
  | LiftDetectTypeNone         | LiftDetectTypeNone         | 200      |
  | MachineHalfPassTwoDrum     | MachineHalfPassTwoDrum     | 200      |
  | MachineHalfPassFourWheel   | MachineHalfPassFourWheel   | 200      |

Scenario Outline: PassCountDetail - Bad Request
  Given the service route "/api/v1/compaction/passcounts/detailed" request repo "PassCountDetailRequest.json" and result repo "PassCountDetailResponse.json"
  When I POST with parameter "<ParameterName>" I expect response code <HttpCode>
  Then the response should contain code "<ErrorCode>"
  Examples: 
  | ParameterName    | HttpCode | ErrorCode |
  | NullProjectId    | 400      | -1        |
  | EmptySetting     | 400      | -1        |
  | DecendingSetting | 400      | -1        |