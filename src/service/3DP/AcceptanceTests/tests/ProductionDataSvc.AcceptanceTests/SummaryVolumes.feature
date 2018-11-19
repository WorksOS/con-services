Feature: SummaryVolumes
  I should be able to request Summary Volumes.

Scenario: SummaryVolumes - Good Request
  Given the service route "/api/v1/volumes/summary" request repo "SummaryVolumeRequest.json" and result repo "SummaryVolumeResponse.json"
  And I require surveyed surface
  When I POST with parameter "<ParameterName>" I expect response code <HttpCode>
  Then the response should match "<ResultName>" from the repository
  Examples: 
  | ParameterName                        | ResultName                           | HttpCode |
  | FilterToFilter                       | FilterToFilter                       | 200      |
  | EarliestFilterToDesign               | EarliestFilterToDesign               | 200      |
  | LatestFilterToDesign                 | LatestFilterToDesign                 | 200      |
  | DesignToEarliestFilter               | DesignToEarliestFilter               | 200      |
  | DesignToLatestFilter                 | DesignToLatestFilter                 | 200      |
  | FilterToCompositeWithSurveyedSurface | FilterToCompositeWithSurveyedSurface | 200      |
  | FilterToCompositeNoSurveyedSurface   | FilterToCompositeNoSurveyedSurface   | 200      |
  | CompositeToDesignWithSurveyedSurface | CompositeToDesignWithSurveyedSurface | 200      |
  | CompositeToDesignNoSurveyedSurface   | CompositeToDesignNoSurveyedSurface   | 200      |
  | DesignToCompositeWithSurveyedSurface | DesignToCompositeWithSurveyedSurface | 200      |
  | DesignToCompositeNoSurveyedSurface   | DesignToCompositeNoSurveyedSurface   | 200      |
  | FilterToDesignWithFillTolerances     | FilterToDesignWithFillTolerances     | 200      |
  | FilterToDesignWithCutTolerances      | FilterToDesignWithCutTolerances      | 200      |
  | FilterToFilterWithBothTolerances     | FilterToFilterWithBothTolerances     | 200      |
  Then Delete surveyed surface file 111

Scenario: SummaryVolumes - Good Request with Old SS
  Given the service route "/api/v1/volumes/summary" request repo "SummaryVolumeRequest.json" and result repo "SummaryVolumeResponse.json"
  And I require surveyed surface
  When I POST with parameter "<ParameterName>" I expect response code <HttpCode>
  Then the response should match "<ResultName>" from the repository
  Examples: 
  | ParameterName                        | ResultName                            | HttpCode |
  | FilterToCompositeWithSurveyedSurface | FilterToCompositeNoSurveyedSurfaceOld | 200      |
  | CompositeToDesignWithSurveyedSurface | CompositeToDesignNoSurveyedSurfaceOld | 200      |
  | DesignToCompositeWithSurveyedSurface | DesignToCompositeNoSurveyedSurfaceOld | 200      |
  Then Delete surveyed surface file 111

Scenario: SummaryVolumes - Bad Request
  Given the service route "/api/v1/volumes/summary" request repo "SummaryVolumeRequest.json" and result repo "SummaryVolumeResponse.json"
  When I POST with parameter "<ParameterName>" I expect response code <HttpCode>
  Then the response should contain code "<ErrorCode>"
  Examples: 
  | ParameterName | HttpCode | ErrorCode |
  | NullProjectId | 400      | -1        |
  | InvalidLatLon | 400      | -1        |
