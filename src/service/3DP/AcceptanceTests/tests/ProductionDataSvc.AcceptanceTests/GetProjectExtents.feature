Feature: GetProjectExtents
  I should be able to get project extents.

Scenario: GetProjectExtents - Excluding Surveyed Surfaces
  Given the service URI "/api/v1/projectextents"
  And require surveyed surface larger than production data
  And request body property "projectId" with value "1001158"
  And I decide to exclude any surveyed surface
  When I POST with no parameters I expect response code 200
  Then the following objects should be returned:
    | maxX               | maxY    | maxZ              | minX    | minY               | minZ             |
    | 2913.2900000000004 | 1250.69 | 624.1365966796875 | 2306.05 | 1125.2300000000002 | 591.953857421875 |
  Then Delete surveyed surface file 111

@ignore
Scenario: GetProjectExtents - Including Surveyed Surfaces
  Given the service URI "/api/v1/projectextents"
  And require surveyed surface larger than production data
  And request body property "projectId" with value "1001158"
  When I POST with no parameters I expect response code 200
  Then the following objects should be returned:
    | maxX               | maxY               | maxZ              | minX              | minY               | minZ             |
    | 2989.1663015263803 | 1325.9072715855209 | 631.9852294921875 |2184.3218844638031 | 1088.3172778947385 | 591.953857421875 |
  Then Delete surveyed surface file 111

Scenario: GetProjectExtents - Bad Request (Null Project ID)
  Given the service URI "/api/v1/projectextents"
  When I POST with no parameters I expect response code 400
  Then the response should contain code "-1"

Scenario: GetProjectExtents - Bad Request (Invalid Project ID)
  Given the service URI "/api/v1/projectextents"
  And request body property "projectId" with value "0"
  When I POST with no parameters I expect response code 400
  Then the response should contain code "-1"

Scenario: GetProjectExtents - Bad Request (Deleted Project)
  Given the service URI "/api/v1/projectextents"
  And request body property "projectId" with value "1000992"
  When I POST with no parameters I expect response code 400
  Then the response should contain code "-4"
