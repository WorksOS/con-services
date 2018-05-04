Feature: ReportTile
I should be able to request report tiles

Scenario Outline: Report Tiles
Given the Report Tile service URI "/api/v2/reporttiles" 
And a projectUid "<ProjectUID>"
And a filterUid "<filterUID>"
And an overlayType "<overlayType>"
And a mapType "<mapType>"
And a mode "<mode>"
And a language "<language>"
When I request a Report Tile and the result file "CompactionReportTileResponse.json"	
Then the result tile should match the "<ResultName>" from the repository within "<Difference>" percent
Examples: 
| ResultName            | ProjectUID                           | filterUID                            | overlayType                            | mapType   | mode | Difference | language |
| DxfLinework           | ff91dd40-1569-4765-a2bc-014321f76ace | 7b2bd262-8355-44ba-938a-d50f9712dafc | DxfLinework                            |           |      | 1          |          |
| Alignments            | ff91dd40-1569-4765-a2bc-014321f76ace |                                      | Alignments                             |           |      | 1          |          |
| ProjectBoundary       | ff91dd40-1569-4765-a2bc-014321f76ace |                                      | ProjectBoundary                        |           |      | 1          |          |
| BaseMap               | ff91dd40-1569-4765-a2bc-014321f76ace |                                      | BaseMap                                | MAP       |      | 1          |          |
| BaseMapZH             | ff91dd40-1569-4765-a2bc-014321f76ace |                                      | BaseMap                                | MAP       |      | 1          | zh-CN    |
| BaseMapEN             | ff91dd40-1569-4765-a2bc-014321f76ace |                                      | BaseMap                                | MAP       |      | 1          | en_US    |
| Elevation             | ff91dd40-1569-4765-a2bc-014321f76ace |                                      | ProductionData                         |           | 0    | 5          |          |
| MDP                   | ff91dd40-1569-4765-a2bc-014321f76ace |                                      | ProductionData                         |           | 20   | 3          |          |
| CMV                   | ff91dd40-1569-4765-a2bc-014321f76ace |                                      | ProductionData                         |           | 1    | 3          |          |
| CMVchange             | ff91dd40-1569-4765-a2bc-014321f76ace |                                      | ProductionData                         |           | 27   | 3          |          |
| CMVsummary            | ff91dd40-1569-4765-a2bc-014321f76ace |                                      | ProductionData                         |           | 13   | 3          |          |
| Speed                 | ff91dd40-1569-4765-a2bc-014321f76ace |                                      | ProductionData                         |           | 26   | 8          |          |
| Temperature           | ff91dd40-1569-4765-a2bc-014321f76ace |                                      | ProductionData                         |           | 10   | 3          |          |
| PassCntDetailOverlay  | ff91dd40-1569-4765-a2bc-014321f76ace |                                      | ProductionData,BaseMap,ProjectBoundary | SATELLITE | 4    | 10         |          |
| PassCntSummaryOverlay | ff91dd40-1569-4765-a2bc-014321f76ace |                                      | ProductionData,BaseMap,ProjectBoundary | SATELLITE | 14   | 10         |          |
| CMVchangeOverlay      | ff91dd40-1569-4765-a2bc-014321f76ace |                                      | ProductionData,BaseMap,ProjectBoundary | SATELLITE | 27   | 10         |          |
| CMVsummaryOverlay     | ff91dd40-1569-4765-a2bc-014321f76ace |                                      | ProductionData,BaseMap,ProjectBoundary | SATELLITE | 13   | 10         |          |
| SpeedOverlay          | ff91dd40-1569-4765-a2bc-014321f76ace |                                      | ProductionData,BaseMap,ProjectBoundary | SATELLITE | 26   | 10         |          |
| TemperatureOverlay    | ff91dd40-1569-4765-a2bc-014321f76ace |                                      | ProductionData,BaseMap,ProjectBoundary | SATELLITE | 10   | 10         |          |
| ElevationOverlay      | ff91dd40-1569-4765-a2bc-014321f76ace |                                      | ProductionData,BaseMap,ProjectBoundary | SATELLITE | 0    | 10         |          |
| MDPOverlay            | ff91dd40-1569-4765-a2bc-014321f76ace |                                      | ProductionData,BaseMap,ProjectBoundary | SATELLITE | 20   | 10         |          |
| CMVOverlay            | ff91dd40-1569-4765-a2bc-014321f76ace |                                      | ProductionData,BaseMap,ProjectBoundary | SATELLITE | 1    | 10         |          |
#| ElevationOverlayAll     | ff91dd40-1569-4765-a2bc-014321f76ace |                                      | AllOverlays                            | HYBRID    | 0    | 3          |
#| CMVchangeOverlayAll     | ff91dd40-1569-4765-a2bc-014321f76ace |                                      | AllOverlays                            | HYBRID    | 27   | 3          |
#| CMVsummaryOverlayAll    | ff91dd40-1569-4765-a2bc-014321f76ace |                                      | AllOverlays                            | HYBRID    | 13   | 3          |
#| SpeedOverlayAll         | ff91dd40-1569-4765-a2bc-014321f76ace |                                      | AllOverlays                            | HYBRID    | 26   | 8          |
#| TemperatureOverlayAll   | ff91dd40-1569-4765-a2bc-014321f76ace |                                      | AllOverlays                            | HYBRID    | 10   | 3          |
#| MDPOverlayAll           | ff91dd40-1569-4765-a2bc-014321f76ace |                                      | AllOverlays                            | HYBRID    | 20   | 3          |
#| CMVOverlayAll           | ff91dd40-1569-4765-a2bc-014321f76ace |                                      | AllOverlays                            | HYBRID    | 1    | 3          |
#| PCWithAlignOverlayAll   | ff91dd40-1569-4765-a2bc-014321f76ace | 2811c7c3-d270-4d63-97e2-fc3340bf6c6b | AllOverlays                            | HYBRID    | 4    | 10         |
#| ElevWithAlignOverlayAll | ff91dd40-1569-4765-a2bc-014321f76ace | 2811c7c3-d270-4d63-97e2-fc3340bf6c6b | AllOverlays                            | HYBRID    | 0    | 3          |
#| TempWithAlignOverlayAll | ff91dd40-1569-4765-a2bc-014321f76ace | 2811c7c3-d270-4d63-97e2-fc3340bf6c6b | AllOverlays                            | HYBRID    | 10   | 3          |
#| Geofences                            | ff91dd40-1569-4765-a2bc-014321f76ace |                                        | Geofences |      |            | 1        |  |


Scenario Outline: Large Report Tiles
Given the Report Tile service URI "/api/v2/reporttiles" 
And a width "1024" and a height "1024"
And a projectUid "<ProjectUID>"
And a filterUid "<filterUID>"
And an overlayType "<overlayType>"
And a mapType "<mapType>"
And a mode "<mode>"
When I request a Report Tile and the result file "CompactionReportTileResponse.json"	
Then the result tile should match the "<ResultName>" from the repository within "<Difference>" percent
Examples: 
| ResultName            | ProjectUID                           | filterUID                            | overlayType                                                             | mapType   | mode | Difference |
| CMVLarge              | ff91dd40-1569-4765-a2bc-014321f76ace |                                      | ProductionData,BaseMap,ProjectBoundary                                  | SATELLITE | 1    | 10         |

Scenario Outline: Report cutfill and volume tiles
Given the Report Tile service URI "/api/v2/reporttiles" 
And a projectUid "<ProjectUID>"
And an overlayType "<overlayType>"
And a mapType "<mapType>"
And a mode "<mode>"
And a cutFillDesignUid "<cutFillDesignUid>"
And a volumeCalcType "<volumeCalcType>" 
And a volumeTopUid "<volumeTopUid>" 
And a volumeBaseUid "<volumeBaseUid>"
And a width "1024" and a height "1024"
And a filterUid "<filterUID>"
When I request a Report Tile and the result file "CompactionReportTileResponse.json"	
Then the result tile should match the "<ResultName>" from the repository within "<Difference>" percent
Examples: 
| ResultName               | ProjectUID                           | cutFillDesignUid                     | volumeCalcType | volumeTopUid                         | volumeBaseUid                        | overlayType                            | mapType | mode | Difference | filterUID                            |
| CutFill                  | ff91dd40-1569-4765-a2bc-014321f76ace | dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff |                |                                      |                                      | ProductionData                         |         | 8    | 5          |                                      |
| CutFillOverlay           | ff91dd40-1569-4765-a2bc-014321f76ace | dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff |                |                                      |                                      | ProductionData,BaseMap,ProjectBoundary | MAP     | 8    | 3         |                                      |
| CutFillTerrain           | ff91dd40-1569-4765-a2bc-014321f76ace | dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff |                |                                      |                                      | ProductionData,BaseMap,ProjectBoundary | TERRAIN | 8    | 3        |                                      |
#| CutFillOverlayAll        | ff91dd40-1569-4765-a2bc-014321f76ace | dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff |                |                                      |                                      | AllOverlays                            | HYBRID  | 8    | 5          |                                      |
| GroundToGround           | ff91dd40-1569-4765-a2bc-014321f76ace |                                      | GroundToGround | A40814AA-9CDB-4981-9A21-96EA30FFECDD | F07ED071-F8A1-42C3-804A-1BDE7A78BE5B | ProductionData                         |         | 8    | 1          |                                      |
#| DesignToGround           | ff91dd40-1569-4765-a2bc-014321f76ace |                                      | DesignToGround | a54e5945-1aaa-4921-9cc1-c9d8c0a343d3 | dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff | ProductionData                         |         | 8    | 5          |                                      |
| GroundToDesign           | ff91dd40-1569-4765-a2bc-014321f76ace |                                      | GroundToDesign | dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff | 9c27697f-ea6d-478a-a168-ed20d6cd9a22 | ProductionData                         |         | 8    | 5          |                                      |
#| D2GOverlayAll            | ff91dd40-1569-4765-a2bc-014321f76ace |                                      | DesignToGround | a54e5945-1aaa-4921-9cc1-c9d8c0a343d3 | dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff | AllOverlays                            | HYBRID  | 8    | 5          |                                      |
| G2DOverlayAll            | ff91dd40-1569-4765-a2bc-014321f76ace |                                      | GroundToDesign | dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff | 9c27697f-ea6d-478a-a168-ed20d6cd9a22 | AllOverlays                            | HYBRID  | 8    | 5          |                                      |
| CFillWithAlignOverlayAll | ff91dd40-1569-4765-a2bc-014321f76ace | dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff |                |                                      |                                      | AllOverlays                            | HYBRID  | 8    | 5          | 2811c7c3-d270-4d63-97e2-fc3340bf6c6b |

Scenario: Report Tile - Missing Mode 
Given the Report Tile service URI "/api/v2/reporttiles" 
And a projectUid "ff91dd40-1569-4765-a2bc-014321f76ace"
And an overlayType "ProductionData"
When I request a Report Tile Expecting BadRequest
Then I should get error code -1 and message "Missing display mode parameter for production data overlay"

Scenario: Report Tile - Missing Map Type 
Given the Report Tile service URI "/api/v2/reporttiles" 
And a projectUid "ff91dd40-1569-4765-a2bc-014321f76ace"
And an overlayType "BaseMap"
When I request a Report Tile Expecting BadRequest
Then I should get error code -1 and message "Missing map type parameter for base map overlay"

Scenario: Report Tile - Missing Overlays 
Given the Report Tile service URI "/api/v2/reporttiles" 
And a projectUid "ff91dd40-1569-4765-a2bc-014321f76ace"
When I request a Report Tile Expecting BadRequest
Then I should get error code -1 and message "At least one type of map tile overlay must be specified"

Scenario: Report Tile - Invalid Size 
Given the Report Tile service URI "/api/v2/reporttiles" 
And a projectUid "ff91dd40-1569-4765-a2bc-014321f76ace"
And an overlayType "BaseMap"
And a width "16" and a height "16"
When I request a Report Tile Expecting BadRequest
Then I should get error code -1 and message "Tile size must be between 64 and 2048 with a base map or 64 and 4096 otherwise"

Scenario: Report Tile - Missing CutFill Design 
Given the Report Tile service URI "/api/v2/reporttiles" 
And a projectUid "ff91dd40-1569-4765-a2bc-014321f76ace"
And an overlayType "ProductionData"
And a mode "8"
When I request a Report Tile Expecting BadRequest
Then I should get error code -1 and message "Missing design for cut-fill production data overlay"

Scenario: Report Tile - Missing Volume Design 
Given the Report Tile service URI "/api/v2/reporttiles" 
And a projectUid "ff91dd40-1569-4765-a2bc-014321f76ace"
And an overlayType "ProductionData"
And a mode "8"
And a volumeCalcType "DesignToGround"
When I request a Report Tile Expecting BadRequest
Then I should get error code -1 and message "Missing design for summary volumes production data overlay"

Scenario: Report Tile - Missing Base Filter 
Given the Report Tile service URI "/api/v2/reporttiles" 
And a projectUid "ff91dd40-1569-4765-a2bc-014321f76ace"
And an overlayType "ProductionData"
And a mode "8"
And a volumeCalcType "GroundToDesign"
And a volumeTopUid "dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff"
When I request a Report Tile Expecting BadRequest
Then I should get error code -1 and message "Missing base filter for summary volumes production data overlay"

Scenario: Report Tile - Missing Top Filter 
Given the Report Tile service URI "/api/v2/reporttiles" 
And a projectUid "ff91dd40-1569-4765-a2bc-014321f76ace"
And an overlayType "ProductionData"
And a mode "8"
And a volumeCalcType "DesignToGround"
And a volumeBaseUid "dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff"
When I request a Report Tile Expecting BadRequest
Then I should get error code -1 and message "Missing top filter for summary volumes production data overlay"









