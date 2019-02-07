Feature: CcaColorPalette
  I should be able to request CcaColorPalette

Scenario: Get CCA color palette
  Given only the service route "/api/v1/ccacolors?projectId=1999999&assetId=1"
  When I send the GET request I expect response code 200
  Then the response should be:
  """
  {
    "palettes": [
    {
      "color": 12632064,
      "value": 1.0
    },
    {
      "color": 16711680,
      "value": 2.0
    },
    {
      "color": 65535,
      "value": 3.0
    },
    {
      "color": 32768,
      "value": 4.0
    }
    ],
    "Code": 0,
    "Message": "success"
  }
  """
