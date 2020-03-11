Feature: AddAssetSettings

	UserStory:43961 Implementation: Asset Run-time Hours and Idle Hours API Endpoint
-----------------------------------------------------------------------------------------------

@US43961@Automated@AddAssetSettings@Positive
Scenario: AddAssetSettings_HappyPath
Given '<Description>' is ready to verify
And AddAssetSettings is setup with default valid values
When I Put Valid asset settings
 And When I try to retrieve asset settings
Then Same details should be displayed

#@US43961@Automated@AddAssetSettings@Positive
#Scenario Outline: AddAssetSettings
#Given '<Description>' is ready to verify
#And AddAssetSettings is setup with default valid values
#And I Put Valid asset settings with startdate as '<AddAssetStartDate>' and EndDate as '<AddAssetEndDate>'
#When I update asset settings with default valid values
#And I put Valid asset settings with startdate as'<UpdateAssetStartDate>' and EndDate as '<UpdateAssetEndDate>'
#Then Valid Response should be given 
#And Asset Details should match in DB.
#And Add and update asset events should be published in kafka topic
#Examples: 
#| Description                                            | AddAssetStartDate | AddAssetEndDate | UpdateAssetStartDate | UpdateAssetEndDate |
#| AddAssetFollowedByUpdateWithOverlapping                | 1-1-2017          | 25-1-2017       | 23-1-2017            | 2-2-2017           |
#| AddAssetFollowedByUpdate_OneSettingsRangeWithinAnother | 1-1-2017          | 30-1-2017       | 5-1-2017             | 6-1-2017           |

@US43961@Automated@AddAssetSettings@Negative
Scenario Outline: AddAssetSettings_Invalid_Asset
Given '<Description>' is ready to verify
And AddAssetSettings is setup with default valid values
And I set AssetUID as '<assetUID>'
When I Put Valid asset settings
Then Valid ErrorResponse Should be shown
Examples: 
| Description            | assetUID      |
| assetUID_Null          | NULL_NULL     |
| assetUID_EmptySpace    | EMPTY_EMPTY   |
| assetUID_InvalidString | InvalidString |

@US43961@Automated@AddAssetSettings@Negative
Scenario Outline: AddAssetSettings_Invalid_StartDateAndEndDate
Given '<Description>' is ready to verify
And AddAssetSettings is setup with default valid values
When I Put Valid asset settings with startdate as '<AddAssetStartDate>' and EndDate as '<AddAssetEndDate>'
Then Valid ErrorResponse Should be shown

Examples: 
| Description                 | AddAssetStartDate | AddAssetEndDate |
| StartDate_Null              | NULL_NULL         | 4-1-2017        |
| InvalidStartAndEndDate      | Today             | Tomorrow        |
| StartDateGreaterThanEndDate | 20-1-2017         | 1-1-2017        |

@US43961@Automated@AddAssetSettings@Negative
Scenario:AddAssetSettings_NoUserCustomerRelation
Given '<Description>' is ready to verify
And GetAssetSettings is setup with default valid values
And I map an user to an different customer which is not mapped
When I try to add asset Details
Then Valid Asset Details response should be returned
And Valid Error Response should be thrown