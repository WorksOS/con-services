Feature: Notification
 I should be able to request file notifications

 Scenario: Notification Add File - Good Request 
	Given the Add File Notification service URI "/api/v2/notification/addfile"
	And a projectUid "ff91dd40-1569-4765-a2bc-014321f76ace"
	And a filespaceId "u3bdc38d6-1afe-470e-8c1c-fc241d4c5e01" and a path "/NotificationAcceptanceTest" and a fileName "Topcon Road - DesignMap.dxf"
	And a fileId "1234"
	When I request File Notification
	Then the File Notification result should be 
  """
	{
    "Code": 0,
    "Message": "Add file notification successful"
  }
	"""
 
 Scenario: Notification Delete File - Good Request 
	Given the Delete File Notification service URI "/api/v2/notification/deletefile"
	And a projectUid "ff91dd40-1569-4765-a2bc-014321f76ace"
	And a filespaceId "u3bdc38d6-1afe-470e-8c1c-fc241d4c5e01" and a path "/NotificationAcceptanceTest" and a fileName "Topcon Road - DesignMap.dxf"
	And a fileId "1234"
	When I request File Notification
	Then the File Notification result should be
  """
	{
    "Code": 0,
    "Message": "Delete file notification successful"
  }
	"""
