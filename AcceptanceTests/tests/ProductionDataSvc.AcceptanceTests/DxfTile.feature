Feature: DxfTile
  I should be able to request DXF tiles

@ignore
Scenario: Dxf Tile - Good Request 
  Given the service route "/api/v2/lineworktiles" and result repo "DXFTilesResponse.json"
  And with parameter "projectUid" with value "ff91dd40-1569-4765-a2bc-014321f76ace"
  And with parameter "bbox" with value "-43.5445665843636, 172.578735351563, -43.5405847948288, 172.584228515625"
  And with parameter "width" with value "256"
  And with parameter "height" with value "256"
  And with parameter "fileType" with value "linework"
  When I send the GET request I expect response code 200
  Then the resulting image should match "GoodRequest" from the response repository within 1 percent

@ignore
Scenario: Dxf Tile - Scaled Tile 
  Given the service route "/api/v2/lineworktiles" and result repo "DXFTilesResponse.json"
  And with parameter "projectUid" with value "ff91dd40-1569-4765-a2bc-014321f76ace"
  And with parameter "bbox" with value "-43.545561990655841, 172.58285522460938, -43.545064289564273, 172.58354187011719"
  And with parameter "width" with value "256"
  And with parameter "height" with value "256"
  And with parameter "fileType" with value "linework"
  When I send the GET request I expect response code 200
  Then the resulting image should match "ScaledTile" from the response repository within 1 percent

Scenario: Dxf Tile - No Imported Files 
  Given the service route "/api/v2/lineworktiles" and result repo "DXFTilesResponse.json"
  And with parameter "projectUid" with value "0fa94210-0d7a-4015-9eee-4d9956f4b250"
  And with parameter "bbox" with value "-43.5445665843636, 172.578735351563, -43.5405847948288, 172.584228515625"
  And with parameter "width" with value "256"
  And with parameter "height" with value "256"
  And with parameter "fileType" with value "linework"
  When I send the GET request I expect response code 200
  Then the resulting image should match "NoImportedFiles" from the response repository within 1 percent

Scenario: Dxf Tile - No FileType 
  Given the service route "/api/v2/lineworktiles" and result repo "DXFTilesResponse.json"
  And with parameter "projectUid" with value "ff91dd40-1569-4765-a2bc-014321f76ace"
  And with parameter "bbox" with value "-43.5445665843636, 172.578735351563, -43.5405847948288, 172.584228515625"
  And with parameter "width" with value "256"
  And with parameter "height" with value "256"
  And with parameter "fileType" with value ""
  When I send the GET request I expect response code 400
  Then the response should contain message "Missing file type" and code "-1"

Scenario: Dxf Tile - Bad FileType 
  Given the service route "/api/v2/lineworktiles" and result repo "DXFTilesResponse.json"
  And with parameter "projectUid" with value "ff91dd40-1569-4765-a2bc-014321f76ace"
  And with parameter "bbox" with value "-43.5445665843636, 172.578735351563, -43.5405847948288, 172.584228515625"
  And with parameter "width" with value "256"
  And with parameter "height" with value "256"
  And with parameter "fileType" with value "SurveyedSurface"
  When I send the GET request I expect response code 400
  Then the response should contain message "Unsupported file type SurveyedSurface" and code "-1"
