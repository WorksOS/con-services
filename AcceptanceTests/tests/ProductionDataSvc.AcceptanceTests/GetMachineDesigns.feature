Feature: GetMachineDesigns
	I should be able to get on-machine designs.

Background: 
	Given the Machine Design service URI "/api/v1/projects/{0}/machinedesigns"

Scenario: GetMachineDesigns - Good Request
	Given a project Id 1001158
	When I request machine designs
	Then the following machine designs should be returned
	| designId | designName                             |
	| 0        | <No Design>                            |
	| 37       | BC12                                   |
	| 38       | Building Pad                           |
	| 39       | Building Pad_20121026_115902           |
	| 10       | Canal Design 2                         |
	| 6        | Canal_DC                               |
	| 5        | Canal_DC 02                            |
	| 4        | Canal_DC 03                            |
	| 13       | Canal_DC v3                            |
	| 11       | Canal_DCv2                             |
	| 2        | Canal_DTM                              |
	| 3        | Canal_Road                             |
	| 7        | Canal2-DC                              |
	| 48       | Design                                 |
	| 22       | Design OGN                             |
	| 45       | Design1BCD1                            |
	| 46       | Dimensions Canal                       |
	| 14       | Dimensions-Canal                       |
	| 15       | Dimensions-Canal_20121105_105256       |
	| 19       | Ground                                 |
	| 20       | Ground Outside                         |
	| 21       | Ground_sync                            |
	| 42       | Large Sites Road                       |
	| 12       | LEVEL 01                               |
	| 28       | LEVEL 02                               |
	| 29       | LEVEL 03                               |
	| 30       | LEVEL 04                               |
	| 32       | LEVEL 05                               |
	| 34       | LEVEL 06                               |
	| 35       | LEVEL 07                               |
	| 8        | MAP 01                                 |
	| 25       | OGL                                    |
	| 18       | OGN                                    |
	| 24       | OGN_Ground                             |
	| 44       | OriginalGround                         |
	| 23       | Outside Ground                         |
	| 1        | Pond1_2                                |
	| 41       | Road2                                  |
	| 40       | SLOPE 01                               |
	| 31       | SLOPE 02                               |
	| 33       | SLOPE 03                               |
	| 36       | SLOPE 04                               |
	| 27       | Small Site Road 29 10 2012             |
	| 43       | Small Sites                            |
	| 16       | Trimble Command Center                 |
	| 17       | Trimble Command Center_20121030_141320 |
	| 9        | Trimble Dim Rd                         |
	| 26       | Trimble Road 29 10 2012                |
	| 47       | Trimble Road with Ref Surfaces v2      |
	| 49       | we love u juarne                       |

#Scenario: GetMachineDesigns - Bad Request (Invalid Project ID)
#	Given a project Id 0
#	When I request machine designs expecting Bad Request
#	Then the response should contain Code -2 and Message "Invalid project ID: 0"