Feature: ProjectThumbnails

Background: 
Given The project thumbnail URI is "/api/v2/projectthumbnail"
And the expected response is in the "ProjectThumbnailResponse.json" respository

@Ignore
Scenario: WithProductionData
	When I request a Report Tile for project UID "ff91dd40-1569-4765-a2bc-014321f76ace" 
	Then The resulting thumbnail should match "ProductionData" from the response repository within "3" percent

@Ignore
Scenario: WithoutProductionData
	When I request a Report Tile for project UID "290df997-7331-405f-ac9c-bebd193965e0" 
	Then The resulting thumbnail should match "NoProductionData" from the response repository within "3" percent

