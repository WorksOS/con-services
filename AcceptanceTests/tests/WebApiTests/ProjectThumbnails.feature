Feature: ProjectThumbnails

#Background: 
#  Given The project thumbnail URI is "/api/v1/projectthumbnail"
#  And the expected response is in the "ProjectThumbnailResponse.json" respository

Scenario: WithProductionData
  Given The project thumbnail URI is "/api/v1/projectthumbnail3d/png"
  And the expected response is in the "ProjectThumbnailResponse.json" respository
	When I request a Report Tile for project UID "ff91dd40-1569-4765-a2bc-014321f76ace" 
	Then The resulting thumbnail should match "ProductionData" from the response repository within "3" percent


Scenario: WithoutProductionData
  Given The project thumbnail URI is "/api/v1/projectthumbnail3d/png"
  And the expected response is in the "ProjectThumbnailResponse.json" respository
	When I request a Report Tile for project UID "290df997-7331-405f-ac9c-bebd193965e0" 
	Then The resulting thumbnail should match "NoProductionData" from the response repository within "3" percent

Scenario: ProjectBoundaryOnly
  Given The project thumbnail URI is "/api/v1/projectthumbnail/png"
  And the expected response is in the "ProjectThumbnailResponse.json" respository
	When I request a Report Tile for project UID "ff91dd40-1569-4765-a2bc-014321f76ace" 
	Then The resulting thumbnail should match "ProjectBoundaryOnly" from the response repository within "3" percent

