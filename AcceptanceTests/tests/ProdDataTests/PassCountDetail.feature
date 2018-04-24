Feature: PassCountDetail
	I should be able to request Pass Count Detail.

Background: 
	Given the Pass Count Detail service URI "/api/v1/compaction/passcounts/detailed", request repo "PassCountDetailRequest.json" and result repo "PassCountDetailResponse.json"

Scenario Outline: PassCountDetail - Good Request
	When I request Pass Count Detail supplying "<RequestName>" paramters from the repository
	Then the Pass Count Detail response should match "<ResultName>" result from the repository
	Examples: 
	| RequestName                | ResultName                 |
	| CommonSetting              | CommonSetting              |
	| NonContiguousSetting       | NonContiguousSetting       |
	| SingleSetting              | SingleSetting              |
	| FilterByFineGPS            | FilterByFineGPS            |
	| FilterByFineGPSInc         | FilterByFineGPSInc         |
	| FilterByMediumGPS          | FilterByMediumGPS          |
	| FilterByMediumGPSInc       | FilterByMediumGPSInc       |
	| FilterByCoarseGPS          | FilterByCoarseGPS          |
	| FilterByCoarseGPSInc       | FilterByCoarseGPSInc       |
	| FilterByAllGPS             | FilterByAllGPS             |
	| FilterByOnMachineDesignId  | FilterByOnMachineDesignId  |
	| FilterByVibrationOn        | FilterByVibrationOn        |
	#| FilterByVibrationOff       | FilterByVibrationOff       |
	| FilterByForward            | FilterByForward            |
	| FilterByReverse            | FilterByReverse            |
	| FilterByOffsetFromDesign   | FilterByOffsetFromDesign   |
	| FilterByOffsetFromBench    | FilterByOffsetFromBench    |
	| FilterByMachines           | FilterByMachines           |
	| LiftDetectTypeAutoMapReset | LiftDetectTypeAutoMapReset |
	| LiftDetectTypeNone         | LiftDetectTypeNone         |
	| MachineHalfPassTwoDrum     | MachineHalfPassTwoDrum     |
	| MachineHalfPassFourWheel   | MachineHalfPassFourWheel   |

Scenario Outline: PassCountDetail - Bad Request
	When I request Pass Count Detail supplying "<RequestName>" paramters from the repository expecting http error code <httpCode>
	Then the response should contain error code <errorCode>
	Examples: 
	| RequestName      | httpCode | errorCode |
	| NullProjectId    | 400      | -1        |
	| EmptySetting     | 400      | -1        |
	| DecendingSetting | 400      | -1        |