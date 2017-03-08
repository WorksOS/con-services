Feature: SurveyedSurface
		I should be able to get, put, delete and post surveyd surfaces

Background: 
	Given the Surveyd surface service URI "/Api/v1/projects/{0}/surveyedsurfaces"
		And using repository "SurveyedSurfaceRequest.json"
		And the Surveyd surface service POST URI "/Api/v1/surveyedsurfaces"
				
Scenario: SurveyedSurface - Get Stored Surface
	Given a project Id 1001151
	When I request surveyd SurveyedSurfaces
	Then the following machine designs should be returned
	| SurveyedSurfaceId | filespaceId                           | fileName                                     | AsAtDate                |
	| 3075              | u3bdc38d6-1afe-470e-8c1c-fc241d4c5e01 | 110411 Topo Haul Road_2011-04-10T110000Z.TTM | 2011-04-10T11:00:00     |
	| 1234              | u3bdc38d6-1afe-470e-8c1c-fc241d4c5e01 | ML3 - Marylands Road - Marylands.ttm         | 2014-12-02T02:14:33.115 |

Scenario: SurveyedSurface - Delete Stored Surveyd Surface
	Given a project Id 1001151
	When I delete surveyd SurveyedSurfaces
		And I request surveyd SurveyedSurfaces
	Then the following machine designs should be returned
	|  SurveyedSurfaceId | filespaceId                           | fileName                                     | AsAtDate                |
	| 3075               | u3bdc38d6-1afe-470e-8c1c-fc241d4c5e01 | 110411 Topo Haul Road_2011-04-10T110000Z.TTM | 2011-04-10T11:00:00     |
	When I post surveyd surface
		And I request surveyd SurveyedSurfaces
	Then the following machine designs should be returned
	| SurveyedSurfaceId | filespaceId                           | fileName                                     | AsAtDate                |
	| 3075              | u3bdc38d6-1afe-470e-8c1c-fc241d4c5e01 | 110411 Topo Haul Road_2011-04-10T110000Z.TTM | 2011-04-10T11:00:00     |
	| 1234              | u3bdc38d6-1afe-470e-8c1c-fc241d4c5e01 | ML3 - Marylands Road - Marylands.ttm         | 2014-12-02T02:14:33.115 |

#Scenario: SurveyedSurface - Bad Request (Invalid Project ID)
#	Given a project Id 0
#	When I request Surveyed Surface expecting Bad Request
#	Then the response should contain Code -2 and Message "Invalid project ID: 0"

Scenario: SurveyedSurface - Bad Request (Duplicate Surveyed Surface)
	Given a project Id 1001151
	When I Post Surveyd Surface "PostStandardFile" expecting Bad Request
	Then the Post response should contain Code -3 and Message "Failed to process Surveyed Surface data request."