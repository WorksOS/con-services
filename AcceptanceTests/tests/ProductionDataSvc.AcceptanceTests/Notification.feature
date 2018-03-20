Feature: Notification
 I should be able to request file notifications

  Scenario: Notification Add DXF File - Good Request 
	Given the Add File Notification service URI "/api/v2/notification/addfile"
	And a projectUid "ff91dd40-1569-4765-a2bc-014321f76ace"
	And a filespaceId "u3bdc38d6-1afe-470e-8c1c-fc241d4c5e01" and a path "/NotificationAcceptanceTest" and a fileName "Topcon Road - DesignMap.dxf"
	And a fileId "1234"
  And a fileUid "314cdcdd-1002-4431-a621-f5aa77be6f79"
  And a dxfUnitsType "1"
	When I request Add File Notification
	Then the Add File Notification result should be 
  """
	{
    "MinZoomLevel": 0,
    "MaxZoomLevel": 0,
    "Code": 0,
    "Message": "Add file notification successful"
  }
	"""
  Scenario: Notification Add TTM File - Good Request 
	Given the Add File Notification service URI "/api/v2/notification/addfile"
	And a projectUid "7925f179-013d-4aaf-aff4-7b9833bb06d6"
	And a filespaceId "u3bdc38d6-1afe-470e-8c1c-fc241d4c5e01" and a path "/NotificationAcceptanceTest" and a fileName "Milling - Milling.TTM"
	And a fileId "15175"
  And a fileUid "220e12e5-ce92-4645-8f01-1942a2d5a57f"
  When I request Add File Notification
	Then the Add File Notification result should be 
  """
	{
    "MinZoomLevel": 0,
    "MaxZoomLevel": 0,
    "Code": 0,
    "Message": "Add file notification successful"
  }
	"""

 Scenario: Notification Delete File - Good Request 
	Given the Delete File Notification service URI "/api/v2/notification/deletefile"
	And a projectUid "ff91dd40-1569-4765-a2bc-014321f76ace"
	And a filespaceId "u3bdc38d6-1afe-470e-8c1c-fc241d4c5e01" and a path "/NotificationAcceptanceTest" and a fileName "Topcon Road - DesignMap.dxf"
	And a fileId "1234"
  And a fileUid "314cdcdd-1002-4431-a621-f5aa77be6f79"
	When I request Delete File Notification
	Then the Delete File Notification result should be
  """
	{
    "Code": 0,
    "Message": "Delete file notification successful"
  }
	"""

 Scenario: Notification Delete File - Design in Filter 
	Given the Delete File Notification service URI "/api/v2/notification/deletefile"
	And a projectUid "7925f179-013d-4aaf-aff4-7b9833bb06d6"
	And a filespaceId "u3bdc38d6-1afe-470e-8c1c-fc241d4c5e01" and a path "/NotificationAcceptanceTest" and a fileName "Milling - Milling.TTM"
	And a fileId "15175"
  And a fileUid "220e12e5-ce92-4645-8f01-1942a2d5a57f"
	When I request Delete File Notification Expecting BadRequest
	Then I should get error code -1 and message "Cannot delete a design surface or alignment file used in a filter"

Scenario: Notification Delete File - Alignment in Filter 
	Given the Delete File Notification service URI "/api/v2/notification/deletefile"
	And a projectUid "ff91dd40-1569-4765-a2bc-014321f76ace"
	And a filespaceId "u3bdc38d6-1afe-470e-8c1c-fc241d4c5e01" and a path "/NotificationAcceptanceTest" and a fileName "Large Sites Road.svl"
	And a fileId "112"
  And a fileUid "6ece671b-7959-4a14-86fa-6bfe6ef4dd62"
	When I request Delete File Notification Expecting BadRequest
	Then I should get error code -1 and message "Cannot delete a design surface or alignment file used in a filter"