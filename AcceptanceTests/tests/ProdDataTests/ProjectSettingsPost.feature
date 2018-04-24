Feature: ProjectSettingsPost
  I should be able to validate project settings but posting these for a validation

Background: 
	Given the Project Settings service URI "/api/v2/validatesettings", request repo "ProjectSettingsValidationRequest.json" and result repo "ProjectSettingsValidationResponse.json"

Scenario Outline: Project Settings POST Validate
	When I Post ProjectSettingsValidation supplying "<ParameterName>" paramters from the repository
	Then the ProjectSettingsValidation response should match "<ResultName>" result from the repository
	Examples: 
	| ParameterName        | ResultName    |
	| TargetsDefault       | TargetsResult |
	| ColorsDefault        | ColorsResult  |
	| TargetsPartialCustom | TargetsResult |
	| ColorsPartialCustom  | ColorsResult  |
	| TargetsFullCustom    | TargetsResult |
	| ColorsFullCustom     | ColorsResult  |
