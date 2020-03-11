Feature: GetDeviceType
		
		UserStory:48359 Implementation: VL Admin - Device Type Filter API
-----------------------------------------------------------------------------------------------

@48359@Automated@GetDeviceType@Positive
Scenario: GetDeviceDetails_HappyPath
Given ' GetDeviceDetails_HappyPath' is ready to verify
And I add a device to a customer 
When I try to get Device details
Then Valid Device Details response should be returned

@48359@Automated@GetDeviceType@Negative
Scenario: GetDeviceDetails_UniqueDevice
Given '<Description>' is ready to verify
And I add same devices to a customer 
When I try to get Device details
Then Valid Device Details response should be returned

@48359@Automated@GetDeviceType@Negative
Scenario: GetDeviceDetails_DifferentCustomerAccess
Given '<Description>' is ready to verify
And I add a device to a customer
And I change Customer details
When I try to get Device details
Then No Details will be displayed

@48359@Automated@GetDeviceType@Negative
Scenario: GetDeviceDetails_customerUIDNull
Given '<Description>' is ready to verify
When I try to get Device details with customerUID null
Then Valid Error Response should be thrown






