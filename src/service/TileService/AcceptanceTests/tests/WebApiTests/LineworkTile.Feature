Feature: LineworkTile
I should be able to request linework tiles

@ignore
Scenario Outline: Linework Tiles 3D
Given the Linework Tile 3D service URI "/api/v1/lineworktiles3d" 
And a projectUid "<ProjectUID>"
And a fileType "<FileType>"
And a zoomLevel "<ZoomLevel>"
And a yTile "<TileY>"
And a xTile "<TileX>"
When I request a Linework Tile and the result file "LineworkTileResponse.json"	
Then the result tile should match the "<ResultName>" from the repository within "<Difference>" percent
Examples: 
| ResultName            | ProjectUID                           | FileType      | ZoomLevel | TileY  | TileX | Difference | 
| Linework              | ff91dd40-1569-4765-a2bc-014321f76ace | Linework      | 15        | 12844  | 5914  | 1          | 
| Alignment             | ff91dd40-1569-4765-a2bc-014321f76ace | Alignment     | 18        | 102753 | 47317 | 1          | 
| GeoTiff               | ff91dd40-1569-4765-a2bc-014321f76ace | GeoTiff       | 16        | 41583  | 64159 | 1          | 

@ignore
Scenario: Linework Tile - Missing File Type 
Given the Linework Tile 3D service URI "/api/v1/lineworktiles3d" 
And a projectUid "ff91dd40-1569-4765-a2bc-014321f76ace"
And a zoomLevel "20"
And a yTile "12345"
And a xTile "54321"
When I request a Linework Tile Expecting BadRequest
Then I should get error code -1 and message "Missing file type"

@ignore
Scenario: Linework Tile - Unsupported File Type 
Given the Linework Tile 3D service URI "/api/v1/lineworktiles3d" 
And a projectUid "ff91dd40-1569-4765-a2bc-014321f76ace"
And a fileType "DesignSurface"
And a zoomLevel "20"
And a yTile "12345"
And a xTile "54321"
When I request a Linework Tile Expecting BadRequest
Then I should get error code -1 and message "Unsupported file type DesignSurface"

@ignore
Scenario: Linework Tile - Invalid File Type 
Given the Linework Tile 3D service URI "/api/v1/lineworktiles3d" 
And a projectUid "ff91dd40-1569-4765-a2bc-014321f76ace"
And a fileType "Dummy"
And a zoomLevel "20"
And a yTile "12345"
And a xTile "54321"
When I request a Linework Tile Expecting BadRequest
Then I should get error code -1 and message "Invalid file type Dummy"