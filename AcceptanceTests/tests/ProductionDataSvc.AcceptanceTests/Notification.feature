Feature: Notification
 I should be able to request file notifications

 Scenario: Notification Add File - Good Request 
	Given the Add File Notification service URI "/api/v2/notification/addfile"
	And a projectUid "ff91dd40-1569-4765-a2bc-014321f76ace"
	And a fileDescriptor "{filespaceId:u3bdc38d6-1afe-470e-8c1c-fc241d4c5e01,path:/NotificationAcceptanceTest,fileName:CERA.bg.dxf}"
	And a fileId "1234"
	When I request Add File Notification
	Then the Add File Notification result should be
  """
	{
    "Code": 0,
    "Message": "success"
  }
	"""

 Scenario: Notification Add File - Bad Request 
	Given the Add File Notification service URI "/api/v2/notification/addfile"
	And a projectUid "ff91dd40-1569-4765-a2bc-014321f76ace"
	And a fileDescriptor ""
	And a fileId ""
	When I request Add File Notification
	Then the Add File Notification result should be
  """
	{
    "Code": 0,
    "Message": "success"
  }
	"""

 Scenario: Notification Delete File - Good Request 
	Given the Add File Notification service URI "/api/v2/notification/deletefile"
	And a projectUid "ff91dd40-1569-4765-a2bc-014321f76ace"
	And a fileDescriptor ""
	And a fileId ""
	When I request Delete File Notification
	Then the Delete File Notification result should be
  """
	{
    "Code": 0,
    "Message": "success"
  }
	"""

 Scenario: Notification Delete File - Bad Request 
	Given the Add File Notification service URI "/api/v2/notification/deletefile"
	And a projectUid "ff91dd40-1569-4765-a2bc-014321f76ace"
	And a fileDescriptor ""
	And a fileId ""
	When I request Delete File Notification
	Then the Delete File Notification result should be
  """
	{
    "Code": 0,
    "Message": "success"
  }
	"""