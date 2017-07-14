Feature: ProjectSettings
	I should be able to validate project settings

Scenario: Project Settings Validate Default Settings
	Given the Project Settings Validation service URI "/api/v2/compaction/validatesettings" 
	And a projectUid "ff91dd40-1569-4765-a2bc-014321f76ace"
  And a projectSettings ""
	When I request settings validation
	Then the settings validation result should be
  """
  {
    "Code": 0,
    "Message": "Project settings are valid"
  }
  """

Scenario:  Project Settings Validate Partial Custom Settings
	Given the Project Settings Validation service URI "/api/v2/compaction/validatesettings" 
	And a projectUid "ff91dd40-1569-4765-a2bc-014321f76ace"
  And a projectSettings "{ useMachineTargetPassCount : false, customTargetPassCountMinimum : 5, customTargetPassCountMaximum : 7 }"
	When I request settings validation
	Then the settings validation result should be
  """
  {
    "Code": 0,
    "Message": "Project settings are valid"
  }
  """

Scenario:  Project Settings Validate Full Custom Settings
	Given the Project Settings Validation service URI "/api/v2/compaction/validatesettings" 
	And a projectUid "ff91dd40-1569-4765-a2bc-014321f76ace"
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
    customBulkingPercent: 7.5
  }
  """
	When I request settings validation
	Then the settings validation result should be
  """
  {
    "Code": 0,
    "Message": "Project settings are valid"
  }
  """

Scenario:  Project Settings Validate Invalid Settings Missing Values
	Given the Project Settings Validation service URI "/api/v2/compaction/validatesettings" 
	And a projectUid "ff91dd40-1569-4765-a2bc-014321f76ace"
  And a projectSettings "{ useMachineTargetPassCount : false, customTargetPassCountMinimum : 5 }"
	When I request settings validation expecting bad request
	Then I should get error code -2 and message "Both minimum and maximum target pass count must be specified"

 Scenario:  Project Settings Validate Invalid Settings Out Of Range Values
	Given the Project Settings Validation service URI "/api/v2/compaction/validatesettings" 
	And a projectUid "ff91dd40-1569-4765-a2bc-014321f76ace"
  And a projectSettings "{ useMachineTargetPassCount : false, customTargetPassCountMinimum : 0, customTargetPassCountMaximum : 7 }"
	When I request settings validation expecting bad request
	Then I should get error code -2 and message "The field customTargetPassCountMinimum must be between 1 and 80."

Scenario:  Project Settings Validate Invalid Settings Out Of Order Values
	Given the Project Settings Validation service URI "/api/v2/compaction/validatesettings" 
	And a projectUid "ff91dd40-1569-4765-a2bc-014321f76ace"
  And a projectSettings "{ useDefaultCutFillTolerances : false, customCutFillTolerances : [3,2,1,0,-1,-3,-2] }"
	When I request settings validation expecting bad request
	Then I should get error code -2 and message "Cut-fill tolerances must be in order of highest cut to lowest fill"

