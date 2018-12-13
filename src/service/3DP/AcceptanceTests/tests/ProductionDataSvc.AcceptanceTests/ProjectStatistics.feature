Feature: ProjectStatistics
  I should be able to request statistics for a project.

Scenario: ProjectStatistics - Excluding Surveyed Surfaces
  Given the service URI "/api/v1/projects/statistics"
  And a Project Statistics project id 1007778
  And I decide to exclude surveyed surfaces "14177,14176,14175,14174,14222"
  When I POST with no parameters I expect response code 200
  Then I should get the following project statistics:
  | startTime               | endTime                 | cellSize | indexOriginOffset | maxX               | maxY    | maxZ             | minX    | minY    | minZ               |
  | 2012-10-30T19:53:39.894 | 2012-11-07T20:39:05.203 | 0.34     | 536870912         | 2886.7700000000004 | 1250.69 | 613.843505859375 | 2316.59 | 1160.93 | 594.96612548828125 |

Scenario: ProjectStatistics - Bad Request (Invalid Project)
  Given the service URI "/api/v1/projects/statistics"
  And a Project Statistics project id 0
  When I POST with no parameters I expect response code 400
  Then the response should contain code "-1"
