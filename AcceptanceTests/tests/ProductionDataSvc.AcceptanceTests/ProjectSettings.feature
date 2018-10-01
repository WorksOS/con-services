Feature: ProjectSettings
	I should be able to validate project settings

Scenario Outline: Project Settings Validate Default Settings
	Given the Project Settings Validation service URI "/api/v2/validatesettings" 
	And a projectUid "<ProjectUID>"  
  And a projectSettings ""
  And a settingsType "<ProjectSettingsType>"
	When I request settings validation
  Then the result should contain code <Code> and message "<Message>"
  Examples: 
  | RequestName | ProjectUID                           | ProjectSettingsType | Code | Message                            |
  | Targets     | ff91dd40-1569-4765-a2bc-014321f76ace | 1                   | 0    | Project settings Targets are valid |
  | Colors      | ff91dd40-1569-4765-a2bc-014321f76ace | 3                   | 0    | Project settings Colors are valid  |

Scenario Outline:  Project Settings Validate Partial Custom Settings
	Given the Project Settings Validation service URI "/api/v2/validatesettings" 
	And a projectUid "<ProjectUID>"  
  And a projectSettings "<ProjectSettings>"
  And a settingsType "<ProjectSettingsType>"
	When I request settings validation
  Then the result should contain code <Code> and message "<Message>"
  Examples: 
  | RequestName | ProjectUID                           | ProjectSettings                                                                                                                   | ProjectSettingsType | Code | Message                            |
  | Targets     | ff91dd40-1569-4765-a2bc-014321f76ace | { useMachineTargetPassCount : false, customTargetPassCountMinimum : 5, customTargetPassCountMaximum : 7 }                         | 1                   | 0    | Project settings Targets are valid |
  | Colors      | ff91dd40-1569-4765-a2bc-014321f76ace | { useDefaultMDPSummaryColors : false, mdpOnTargetColor : 0x8BC34A, mdpOverTargetColor : 0xD50000, mdpUnderTargetColor : 0x1579B } | 3                   | 0    | Project settings Colors are valid  |

#Scenario Outline:  Project Settings Validate Full Custom Settings
#  Given the Project Settings Validation service URI "/api/v2/validatesettings" and project settings repo "ProjectSettings.json"
#	And a projectUid "<ProjectUID>"  
#  And a settingsType "<ProjectSettingsType>"  
#  And supplying "<ProjectSettingsName>" paramters from the repository
#  When I request settings validation
#  Then the result should contain code <Code> and message "<Message>"
#  Examples: 
#  | ProjectSettingsName | ProjectUID                           | ProjectSettingsType | Code | Message                            |
#  | Targets             | ff91dd40-1569-4765-a2bc-014321f76ace | 1                   | 0    | Project settings Targets are valid |
#  | Colors              | ff91dd40-1569-4765-a2bc-014321f76ace | 3                   | 0    | Project settings Colors are valid  |


Scenario:  Project Settings Validate Full Custom Settings Targets
	Given the Project Settings Validation service URI "/api/v2/validatesettings" 
	And a projectUid "ff91dd40-1569-4765-a2bc-014321f76ace"
  And a settingsType "1"
  And a projectSettings (multiline)
  """
  {
    useMachineTargetPassCount: false,
    customTargetPassCountMinimum: 5,
    customTargetPassCountMaximum: 7,
    useMachineTargetTemperature: false,
    customTargetTemperatureMinimum: 75,
    customTargetTemperatureMaximum: 150,
    useMachineTargetCmv: false,
    customTargetCmv: 77,
    useMachineTargetMdp: false,
    customTargetMdp: 88,
    useDefaultTargetRangeCmvPercent: false,
    customTargetCmvPercentMinimum: 75,
    customTargetCmvPercentMaximum: 105,
    useDefaultTargetRangeMdpPercent: false,
    customTargetMdpPercentMinimum: 85,
    customTargetMdpPercentMaximum: 115,
    useDefaultTargetRangeSpeed: false,
    customTargetSpeedMinimum: 10,
    customTargetSpeedMaximum: 30,
    useDefaultCutFillTolerances: false,
    customCutFillTolerances: [3,2,1,0,-1,-2,-3],
    useDefaultVolumeShrinkageBulking: false,
    customShrinkagePercent: 5,
    customBulkingPercent: 7.5,
    useDefaultPassCountTargets: false,
    customPassCountTargets: [1,3,5,8,11,16,20,25],
    useDefaultCMVTargets: false, 
    customCMVTargets: [0,20,50,100,130],
    useDefaultTemperatureTargets: false, 
    customTemperatureTargets: [0,75,150,250,375]
  }
  """
	When I request settings validation
	Then the settings validation result should be
  """
  {
    "Code": 0,
    "Message": "Project settings Targets are valid"
  }
  """

Scenario:  Project Settings Validate Full Custom Settings Colors
	Given the Project Settings Validation service URI "/api/v2/validatesettings" 
	And a projectUid "ff91dd40-1569-4765-a2bc-014321f76ace"
  And a settingsType "3"
  And a projectSettings (multiline)
  """
  {
    useDefaultElevationColors: true,
    elevationColors: [0xC80000, 0xFF0000, 0xFF3C00, 0xFF5A00, 0xFF8200, 0xFFAA00, 0xFFC800, 
                        0xFFDC00, 0xFAE600, 0xDCE600, 0xD2E600, 0xC8E600, 0xB4E600, 0x96E600, 
                        0x82E600, 0x64F000, 0x00FF00, 0x00F064, 0x00E682, 0x00E696, 0x00E6B4,
                        0x00E6C8, 0x00E6D2, 0x00DCDC, 0x00E6E6, 0x00C8E6, 0x00B4F0, 0x0096F5,
                        0x0078FA, 0x005AFF, 0x0046FF, 0x0000FF],
    useDefaultCMVDetailsColors: true,
    cmvDetailsColors: [0x01579B, 0x6BACD5, 0x99CB65, 0xF6A3A8, 0xD50000],
    useDefaultCMVSummaryColors: true,
    cmvOnTargetColor: 0x8BC34A,
    cmvOverTargetColor: 0xD50000,
    cmvUnderTargetColor: 0x1579B,
    useDefaultCMVPercentColors: true,
    cmvPercentColors: [0xD50000, 0xE57373, 0xFFCDD2, 0x8BC34A, 0xB3E5FC, 0x005AFF, 0x039BE5, 0x01579B],
    useDefaultPassCountDetailsColors: true,
    passCountDetailsColors: [0x2D5783, 0x439BDC, 0xBEDFF1, 0x9DCE67, 0x6BA03E, 0x3A6B25, 0xF6CED3, 0xD57A7C, 0xC13037],
    useDefaultPassCountSummaryColors: true,
    passCountOnTargetColor: 0x8BC34A,
    passCountOverTargetColor: 0xD50000,
    passCountUnderTargetColor: 0x1579B,
    useDefaultCutFillColors: true,
    cutFillColors: [0xD50000, 0xE57373, 0xFFCDD2, 0x8BC34A, 0xB3E5FC, 0x039BE5, 0x01579B],
    useDefaultTemperatureSummaryColors: true,
    temperatureOnTargetColor: 0x8BC34A,
    temperatureOverTargetColor: 0xD50000,
    temperatureUnderTargetColor: 0x1579B,
    useDefaultTemperatureDetailsColors: true,
    temperatureDetailsColors: [0x01579B, 0x6BACD5, 0x99CB65, 0xF6A3A8, 0xD50000],
    useDefaultSpeedSummaryColors: true,
    speedOnTargetColor: 0x8BC34A,
    speedOverTargetColor: 0xD50000,
    speedUnderTargetColor: 0x1579B,
    useDefaultMDPSummaryColors: true,
    mdpOnTargetColor: 0x8BC34A,
    mdpOverTargetColor: 0xD50000,
    mdpUnderTargetColor: 0x1579B
  }
  """
	When I request settings validation
	Then the settings validation result should be
  """
  {
    "Code": 0,
    "Message": "Project settings Colors are valid"
  }
  """


Scenario Outline:  Project Settings Validate Invalid Settings Missing Values
	Given the Project Settings Validation service URI "/api/v2/validatesettings" 
	And a projectUid "<ProjectUID>"
  And a projectSettings "<ProjectSettings>"
  And a settingsType "<ProjectSettingsType>"
	When I request settings validation expecting bad request
	#Then I should get error code -1 and message "Both minimum and maximum target pass count must be specified"
  Then the result should contain code <Code> and message "<Message>"
  Examples: 
  | RequestName | ProjectUID                           | ProjectSettings                                                                                    | ProjectSettingsType | Code | Message                                                      |
  | Targets     | ff91dd40-1569-4765-a2bc-014321f76ace | { useMachineTargetPassCount : false, customTargetPassCountMinimum : 5 }                            | 1                   | -1   | Both minimum and maximum target pass count must be specified |
  | Colors      | ff91dd40-1569-4765-a2bc-014321f76ace | { useDefaultMDPSummaryColors : false, mdpOnTargetColor : 0x8BC34A, mdpOverTargetColor : 0xD50000 } | 3                   | -1   | mdpUnderTargetColor colour values must be specified          |


 Scenario:  Project Settings Validate Invalid Settings Out Of Range Values
	Given the Project Settings Validation service URI "/api/v2/validatesettings" 
	And a projectUid "ff91dd40-1569-4765-a2bc-014321f76ace"
  And a projectSettings "{ useMachineTargetPassCount : false, customTargetPassCountMinimum : 0, customTargetPassCountMaximum : 7 }"
  And a settingsType "1"
	When I request settings validation expecting bad request
	Then I should get error code -1 and message "The field customTargetPassCountMinimum must be between 1 and 80."

Scenario:  Project Settings Validate Invalid Settings Out Of Order Values
	Given the Project Settings Validation service URI "/api/v2/validatesettings" 
	And a projectUid "ff91dd40-1569-4765-a2bc-014321f76ace"
  And a projectSettings "{ useDefaultCutFillTolerances : false, customCutFillTolerances : [3,2,1,0,-1,-3,-2] }"
  And a settingsType "1"
	When I request settings validation expecting bad request
	Then I should get error code -1 and message "Cut-fill tolerances must be in order of highest cut to lowest fill"

