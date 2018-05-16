Feature: SummaryVolumes
	I should be able to request Summary Volumes.

Background: 
	Given the Summary Volumes service URI "/api/v1/volumes/summary", request repo "SummaryVolumeRequest.json" and result repo "SummaryVolumeResponse.json"

@requireSurveyedSurface
Scenario Outline: SummaryVolumes - Good Request
	When I request Summary Volumes supplying "<ParameterName>" paramters from the repository
	Then the response should match "<ResultName>" result from the repository
	Examples: 
	| ParameterName                        | ResultName                           |
	| FilterToFilter                       | FilterToFilter                       |
	| EarliestFilterToDesign               | EarliestFilterToDesign               |
	| LatestFilterToDesign                 | LatestFilterToDesign                 |
	| DesignToEarliestFilter               | DesignToEarliestFilter               |
	| DesignToLatestFilter                 | DesignToLatestFilter                 |
	#| FilterToCompositeWithSurveyedSurface | FilterToCompositeWithSurveyedSurface |
	| FilterToCompositeNoSurveyedSurface   | FilterToCompositeNoSurveyedSurface   |
	#| CompositeToDesignWithSurveyedSurface | CompositeToDesignWithSurveyedSurface |
	| CompositeToDesignNoSurveyedSurface   | CompositeToDesignNoSurveyedSurface   |
	#| DesignToCompositeWithSurveyedSurface | DesignToCompositeWithSurveyedSurface |
	| DesignToCompositeNoSurveyedSurface   | DesignToCompositeNoSurveyedSurface   |
	| SummationTestLotOneOfThree           | SummationTestLotOneOfThree           |
	| SummationTestLotTwoOfThree           | SummationTestLotTwoOfThree           |
	| SummationTestLotThreeOfThree         | SummationTestLotThreeOfThree         |
	| SummationTestTheWholeLot             | SummationTestTheWholeLot             |
	| FilterToDesignWithFillTolerances     | FilterToDesignWithFillTolerances     |
	| FilterToDesignWithCutTolerances      | FilterToDesignWithCutTolerances      |
	| FilterToFilterWithBothTolerances     | FilterToFilterWithBothTolerances     |

#@requireOldSurveyedSurface
#Scenario Outline: SummaryVolumes - Good Request with Old SS
#	When I request Summary Volumes supplying "<ParameterName>" paramters from the repository
#	Then the response should match "<ResultName>" result from the repository
#	Examples: 
#	| ParameterName                        | ResultName                            |
#	| FilterToCompositeWithSurveyedSurface | FilterToCompositeNoSurveyedSurfaceOld |
#	| CompositeToDesignWithSurveyedSurface | CompositeToDesignNoSurveyedSurfaceOld |
#	| DesignToCompositeWithSurveyedSurface | DesignToCompositeNoSurveyedSurfaceOld |

Scenario Outline: SummaryVolumes - Bad Request
	When I request Summary Volumes supplying "<ParameterName>" paramters from the repository expecting error http code <HttpCode>
	Then the response body should contain Error Code <ErrorCode>
	Examples: 
	| ParameterName         | HttpCode | ErrorCode |
	| NullProjectId			| 400      | -1        |
	| InvalidLatLon			| 400      | -1		   |