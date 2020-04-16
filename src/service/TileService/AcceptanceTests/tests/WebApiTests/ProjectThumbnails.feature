Feature: ProjectThumbnails

#Background: 
#  Given The project thumbnail URI is "/api/v1/projectthumbnail"
#  And the expected response is in the "ProjectThumbnailResponse.json" respository

@ignore
Scenario: WithProductionData
  Given The project thumbnail URI is "/api/v1/projectthumbnail3d/png" for operation "png"
  And the expected response is in the "ProjectThumbnailResponse.json" respository
	When I request a Report Tile for project UID "ff91dd40-1569-4765-a2bc-014321f76ace" 
	Then The resulting thumbnail should match "ProductionData" from the response repository within "3" percent

@ignore
Scenario: WithProductionDataBase64
  Given The project thumbnail URI is "/api/v1/projectthumbnail3d/base64" for operation "base64"
  And the expected response is in the "ProjectThumbnailResponse.json" respository
	When I request a Report Tile for project UID "ff91dd40-1569-4765-a2bc-014321f76ace" 
	Then The resulting thumbnail should match "ProductionData" from the response repository within "3" percent

@ignore
Scenario: WithoutProductionData
  Given The project thumbnail URI is "/api/v1/projectthumbnail3d/png" for operation "png"
  And the expected response is in the "ProjectThumbnailResponse.json" respository
	When I request a Report Tile for project UID "290df997-7331-405f-ac9c-bebd193965e0" 
	Then The resulting thumbnail should match "NoProductionData" from the response repository within "3" percent

@ignore
Scenario: ProjectBoundaryOnly
  Given The project thumbnail URI is "/api/v1/projectthumbnail/png" for operation "png"
  And the expected response is in the "ProjectThumbnailResponse.json" respository
	When I request a Report Tile for project UID "ff91dd40-1569-4765-a2bc-014321f76ace" 
	Then The resulting thumbnail should match "ProjectBoundaryOnly" from the response repository within "3" percent

@ignore
Scenario: ProjectBoundaryOnlyBase64
  Given The project thumbnail URI is "/api/v1/projectthumbnail/base64" for operation "base64"
  And the expected response is in the "ProjectThumbnailResponse.json" respository
	When I request a Report Tile for project UID "ff91dd40-1569-4765-a2bc-014321f76ace" 
	Then The resulting thumbnail should match "ProjectBoundaryOnly" from the response repository within "3" percent

@ignore
Scenario: WithLoadDumpData
  Given The project thumbnail URI is "/api/v1/projectthumbnail2d/png" for operation "png"
  And the expected response is in the "ProjectThumbnailResponse.json" respository
	When I request a Report Tile for project UID "ff91dd40-1569-4765-a2bc-014321f76ace" 
	Then The resulting thumbnail should match "LoadDumpData" from the response repository within "3" percent

@ignore
Scenario: WithLoadDumpDataBase64
  Given The project thumbnail URI is "/api/v1/projectthumbnail2d/base64" for operation "base64"
  And the expected response is in the "ProjectThumbnailResponse.json" respository
	When I request a Report Tile for project UID "ff91dd40-1569-4765-a2bc-014321f76ace" 
	Then The resulting thumbnail should match "LoadDumpData" from the response repository within "3" percent

