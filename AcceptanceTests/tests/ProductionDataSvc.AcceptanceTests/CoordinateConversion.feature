Feature: CoordinateConversion
  I should be able to convert coordinates from LL to NE or vice versa.

Scenario: CoordinateConversion - LL to NE
  Given the service URI "/api/v1/coordinateconversion"
  And request body property "projectId" with value "1001158"
  And the coordinate conversion type "LatLonToNorthEast"
  And these coordinates
    | x            | y           |
    | -2.007453062 | 0.631935272 |
    | -2.007483867 | 0.631929809 |
  When I POST with no parameters I expect response code 200
  Then the response should be:
  """
  {
    "conversionCoordinates": [
        {
            "x": 2884.6670047633297,
            "y": 1193.9659626211164
        },
        {
            "x": 2725.9309200498797,
            "y": 1159.2245309413186
        }
    ],
    "Code": 0,
    "Message": "success"
  }
  """

Scenario: CoordinateConversion - NE to LL
  Given the service URI "/api/v1/coordinateconversion"
  And request body property "projectId" with value "1001158"
  And the coordinate conversion type "NorthEastToLatLon"
  And these coordinates
    | x        | y        |
    | 2884.667 | 1193.966 |
    | 2725.931 | 1159.225 |
  When I POST with no parameters I expect response code 200
  Then the response should be:
  """
  {
    "conversionCoordinates": [
        {
            "x": -2.0074530620009243,
            "y": 0.63193527200587885
        },
        {
            "x": -2.0074838669844821,
            "y": 0.63192980907377039
        }
    ],
    "Code": 0,
    "Message": "success"
  }
  """

Scenario: CoordinateConversion - Bad Request (Bad LL)
  Given the service URI "/api/v1/coordinateconversion"
  And request body property "projectId" with value "1001158"
  And these coordinates
    | x            | y           |
    | -5.007453062 | 0.631935272 |
    | -2.007483867 | 5.631929809 |
  When I POST with no parameters I expect response code 400
  Then the response should contain code "-1"
