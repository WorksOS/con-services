Feature: Notification Delete
  I should be able to request file notifications

Scenario: Notification Delete File - Good Request 
  Given only the service route "/api/v2/notification/deletefile"
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
  When I send the GET request I expect response code 200
  Then the response should be:
  """
  {
    "Code": 0,
    "Message": "Delete file notification successful"
  }
  """

Scenario: Notification Delete File - Design in Filter 
  Given only the service route "/api/v2/notification/deletefile"
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
  When I send the GET request I expect response code 400
  Then the response should contain message "Cannot delete a design surface or alignment file used in a filter" and code "-1"

Scenario: Notification Delete File - Alignment in Filter 
  Given only the service route "/api/v2/notification/deletefile"
  And with parameter "projectUid" with value "ff91dd40-1569-4765-a2bc-014321f76ace"
  And with parameter "fileType" with value "3"
  And with parameter "fileDescriptor" and multiline value:
  """
  {
    "filespaceId": "u3bdc38d6-1afe-470e-8c1c-fc241d4c5e01",
    "path": "/NotificationAcceptanceTest",
    "fileName": "Large Sites Road.svl"
  }
  """
  And with parameter "fileId" with value "112"
  And with parameter "fileUid" with value "6ece671b-7959-4a14-86fa-6bfe6ef4dd62"
  When I send the GET request I expect response code 400
  Then the response should contain message "Cannot delete a design surface or alignment file used in a filter" and code "-1"