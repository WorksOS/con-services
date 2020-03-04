Feature: MileageTargetAPIFeature

User Stroy:43963 Implementation of API: Mileage Target API
#---------------------------------------------------------------------------------------------------------------------------

@Automated @Sanity @Positive
@MileageTargetAPIFeature @US43963
Scenario:MileageTargetAPI_HappyPath
Given TargetAPI is ready to verify '<Decription>'
   And I create Asset using create asset request
   And TargetAPI request is setup with default valid values
When I POST valid TargetAPI request
Then estimated mileage value retrived should match with DB
 

#@ignore @Automated @Regression @Positive
#@MileageTargetAPIFeature @US43963
#Scenario Outline: MileageTargetAPI_NumberOfAssetUIDs
#Given TargetAPI is ready to verify '<Description>'
#   And I create Asset'<numberOfAssets>' using create asset request
#When I Set '<AssetUids>' to mileage Request
# And I POST valid TargetAPI request
#Then estimated mileage value retrived should match with DB
#Examples:
#| numberOfAssetUIDs |
#| 1                 |
#| 5                 |
#| 15                |
#| 30                |
#
#@Manual @Regression @Positive
#@MileageTargetAPIFeature @US43963
#Scenario Outline:MileageTargetAPI_StartDateValues
#Given TargetAPI is ready to verify '<Description>'
#   And I create Asset using create asset request
#   And I set startDate to '<startDate>' in the url
#   And TargetAPI request is setup with default valid values
#When I POST valid TargetAPI request
#Then TargetAPI should return estimated mileage value 
#   And estimated mileage value retrived should match with DB
#Examples:
#| Description | startDate |
#| PastDate    |           |
#| CurrentDate |           |
#| FutureDate  |           |
#| WithNoDate  |           |
#
#@Manual @Regression @Positive
#@MileageTargetAPIFeature @US43963
#Scenario:MileageTargetAPI_anotherUserCustomerAsset
#Given TargetAPI is ready to verify '<Description>'
#   And I create Asset using create asset request
#   And TargetAPI request is set up with anotherUserCustomerAsset
#When I POST valid TargetAPI request
#Then TargetAPI should return Error_Message 




 



