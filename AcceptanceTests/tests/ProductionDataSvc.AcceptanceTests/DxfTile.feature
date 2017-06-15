Feature: DxfTile
	I should be able to request DXF tiles

Scenario: Dxf Tile - Good Request 
	Given the Dxf Tile service URI "/api/v2/compaction/lineworktiles" 
	And a projectUid "ff91dd40-1569-4765-a2bc-014321f76ace"
  And a bbox "-43.5445665843636, 172.578735351563, -43.5405847948288, 172.584228515625" and a width "256" and a height "256"
  And a fileUid "cfcd4c01-6fc8-45d5-872f-513a0f619f03"
	When I request a Dxf Tile
	Then the Dxf Tile result should be
  """
  {
    "tileData": "iVBORw0KGgoAAAANSUhEUgAAAQAAAAEACAYAAABccqhmAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAFySURBVHhe7dRBCgQhDARAv+v/H7CjMkIIwsAeYxWI2nrtNAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAIC/9d5/c71X4Ba7+IYAXMoQgMvkou/yGwBQWCz46WwIQFGx5CsYTue5xxwoIpY87lvM8xtQQC53Lvq+539AAbHgKxhy0U9/gCK+hkB8jzlQRCz5CobTee4xv0drD/arc8l8x/8cAAAAAElFTkSuQmCC",
    "tileOutsideProjectExtents": false,
    "Code": 0,
    "Message": "success"
  }
	"""

Scenario: Dxf Tile - No Imported Files 
	Given the Dxf Tile service URI "/api/v2/compaction/lineworktiles"
	And a projectUid "0fa94210-0d7a-4015-9eee-4d9956f4b250"
  And a bbox "-43.5445665843636, 172.578735351563, -43.5405847948288, 172.584228515625" and a width "256" and a height "256"
  And a fileUid "cfcd4c01-6fc8-45d5-872f-513a0f619f03"
	When I request a Dxf Tile Expecting NoContent
	Then I should get no response body

Scenario: Dxf Tile - No FileUids 
	Given the Dxf Tile service URI "/api/v2/compaction/lineworktiles"
	And a projectUid "ff91dd40-1569-4765-a2bc-014321f76ace"
  And a bbox "-43.5445665843636, 172.578735351563, -43.5405847948288, 172.584228515625" and a width "256" and a height "256"
  And a fileUid ""
	When I request a Dxf Tile Expecting NoContent
	Then I should get no response body

 