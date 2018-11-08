Feature: GetMachineDesigns
  I should be able to get on-machine designs.

Scenario: GetMachineDesigns - Good Request
  Given only the service route "/api/v1/projects/1001158/machinedesigns"
  When I send the GET request I expect response code 200
  Then the following machine designs should be returned:
  | designId | designName                             |
  | 0        | <No Design>                            |
  | 1        | Pond1_2                                |
  | 2        | Canal_DTM                              |
  | 3        | Canal_Road                             |
  | 4        | Canal_DC 03                            |
  | 5        | Canal_DC 02                            |
  | 6        | Canal_DC                               |
  | 7        | Canal2-DC                              |
  | 8        | MAP 01                                 |
  | 9        | Trimble Dim Rd                         |
  | 10       | Canal Design 2                         |
  | 11       | Canal_DCv2                             |
  | 12       | LEVEL 01                               |
  | 13       | Canal_DC v3                            |
  | 14       | Dimensions-Canal                       |
  | 15       | Dimensions-Canal_20121105_105256       |
  | 16       | Trimble Command Center                 |
  | 17       | Trimble Command Center_20121030_141320 |
  | 18       | OGN                                    |
  | 19       | Ground                                 |
  | 20       | Ground Outside                         |
  | 21       | Ground_sync                            |
  | 22       | Design OGN                             |
  | 23       | Outside Ground                         |
  | 24       | OGN_Ground                             |
  | 25       | OGL                                    |
  | 26       | Trimble Road 29 10 2012                |
  | 27       | Small Site Road 29 10 2012             |
  | 28       | LEVEL 02                               |
  | 29       | LEVEL 03                               |
  | 30       | LEVEL 04                               |
  | 31       | SLOPE 02                               |
  | 32       | LEVEL 05                               |
  | 33       | SLOPE 03                               |
  | 34       | LEVEL 06                               |
  | 35       | LEVEL 07                               |
  | 36       | SLOPE 04                               |
  | 37       | BC12                                   |
  | 38       | Building Pad                           |
  | 39       | Building Pad_20121026_115902           |
  | 40       | SLOPE 01                               |
  | 41       | Road2                                  |
  | 42       | Large Sites Road                       |
  | 43       | Small Sites                            |
  | 44       | OriginalGround                         |
  | 45       | Design1BCD1                            |
  | 46       | Dimensions Canal                       |
  | 47       | Trimble Road with Ref Surfaces v2      |
  | 48       | Design                                 |
  | 49       | we love u juarne                       |

Scenario Outline: GetMachineDesigns For Date Range - Good Request
  Given the service route "/api/v2/projects/<ProjectUID>/<Operation>" and result repo "GetMachineDesignsResponse.json"
  And with parameter "startUTC" with value "<startUTC>"
  And with parameter "endUTC" with value "<endUTC>"
  When I send the GET request I expect response code <HttpCode>
  Then the response should match "<ResultName>" from the repository
  Examples: 
  | RequestName   | ProjectUID                           | Operation            | startUTC             | endUTC               | ResultName    | HttpCode |
  | NoFilter      | ff91dd40-1569-4765-a2bc-014321f76ace | machinedesigns       |                      |                      | NoFilter      | 200      |
  | NoDateRange   | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | machinedesigndetails |                      |                      | NoDateRange   | 200      |
  | WithDateRange | ff91dd40-1569-4765-a2bc-014321f76ace | machinedesigndetails | 2012-11-01T00:00:00Z | 2012-11-02T00:00:00Z | WithDateRange | 200      |
