Feature: Notification Add
  I should be able to request file notifications

Scenario: Notification Add DXF File - Good Request 
  Given only the service route "/api/v2/notification/addfile"
  And with parameter "projectUid" with value "ff91dd40-1569-4765-a2bc-014321f76ace"
  And with parameter "fileType" with value "0"
  And with parameter "fileDescriptor" and multiline value:
  """
  {
    "filespaceId": "u3bdc38d6-1afe-470e-8c1c-fc241d4c5e01",
    "path": "/NotificationAcceptanceTest",
    "fileName": "Topcon Road - DesignMap.dxf"
  }
  """
  And with parameter "fileId" with value "1234"
  And with parameter "fileUid" with value "314cdcdd-1002-4431-a621-f5aa77be6f79"
  And with parameter "dxfUnitsType" with value "1"
  When I send the GET request I expect response code 200
  Then the response should be:
  """
  {
    "minZoomLevel": 0,
    "maxZoomLevel": 0,
    "fileUid": "00000000-0000-0000-0000-000000000000",
    "fileDescriptor": null,
    "userEmailAddress": null,
    "Code": 0,
    "Message": "Add file notification successful"
  }
  """
Scenario: Notification Add TTM File - Good Request 
  Given only the service route "/api/v2/notification/addfile"
  And with parameter "projectUid" with value "7925f179-013d-4aaf-aff4-7b9833bb06d6"
  And with parameter "fileType" with value "1"
  And with parameter "fileDescriptor" and multiline value:
  """
  {
    "filespaceId": "u3bdc38d6-1afe-470e-8c1c-fc241d4c5e01",
    "path": "/NotificationAcceptanceTest",
    "fileName": "Milling - Milling.TTM"
  }
  """
  And with parameter "fileId" with value "15175"
  And with parameter "fileUid" with value "220e12e5-ce92-4645-8f01-1942a2d5a57f"
  And with parameter "dxfUnitsType" with value "-1"
  When I send the GET request I expect response code 200
  Then the response should be:
  """
  {
    "minZoomLevel": 0,
    "maxZoomLevel": 0,
    "fileUid": "00000000-0000-0000-0000-000000000000",
    "fileDescriptor": null,
    "userEmailAddress": null,
    "Code": 0,
    "Message": "Add file notification successful"
  }
  """
