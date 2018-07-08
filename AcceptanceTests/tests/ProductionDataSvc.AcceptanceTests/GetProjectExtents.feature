Feature: GetProjectExtents
	I should be able to get project extents.

Background: 
	Given the Project Extent service URI "/api/v1/projectextents"

@requireSurveyedSurfaceLargerThanProductionData
Scenario: GetProjectExtents - Excluding Surveyed Surfaces
	Given a GetProjectExtents project id 1001158
		And I decide to exclude any surveyed surface
	When I try to get the extents
	Then the following Bounding Box ThreeD Grid values should be returned
		| maxX               | maxY    | maxZ              | minX    | minY               | minZ             |
		| 2913.2900000000004 | 1250.69 | 624.1365966796875 | 2306.05 | 1125.2300000000002 | 591.953857421875 |


@requireSurveyedSurfaceLargerThanProductionData @ignore
Scenario: GetProjectExtents - Including Surveyed Surfaces
	Given a GetProjectExtents project id 1001158
	When I try to get the extents
	Then the following Bounding Box ThreeD Grid values should be returned
		| maxX               | maxY               | maxZ              | minX              | minY               | minZ             |
		| 2989.1663015263803 | 1325.9072715855209 | 631.9852294921875 |2184.3218844638031 | 1088.3172778947385 | 591.953857421875 |

Scenario: GetProjectExtents - Bad Request (Null Project ID)
	Given a GetProjectExtents null project id
	When I try to get the extents expecting badrequest
	Then I should get error code -1

Scenario: GetProjectExtents - Bad Request (Invalid Project ID)
	Given a GetProjectExtents project id 0
	When I try to get the extents expecting badrequest
	Then I should get error code -1

Scenario: GetProjectExtents - Bad Request (Deleted Project)
	Given a GetProjectExtents project id 1000992
	When I try to get the extents expecting badrequest
	Then I should get error code -4
