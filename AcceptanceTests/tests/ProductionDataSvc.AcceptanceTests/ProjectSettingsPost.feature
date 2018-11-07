Feature: ProjectSettingsPost
  I should be able to validate project settings but posting these for a validation

Scenario Outline: Project Settings POST Validate
  Given the service route "/api/v2/validatesettings" request repo "ProjectSettingsValidationRequest.json" and result repo "ProjectSettingsValidationResponse.json"
  When I POST with parameter "<ParameterName>" I expect response code <HttpCode>
  Then the response should match "<ResultName>" from the repository
  Examples: 
  | ParameterName        | ResultName    | HttpCode |
  | TargetsDefault       | TargetsResult | 200      |
  | ColorsDefault        | ColorsResult  | 200      |
  | TargetsPartialCustom | TargetsResult | 200      |
  | ColorsPartialCustom  | ColorsResult  | 200      |
  | TargetsFullCustom    | TargetsResult | 200      |
  | ColorsFullCustom     | ColorsResult  | 200      |
