Feature: GetAssetSettings
	
	UserStory:43962 Implementation: Asset Run-time Hours and Idle Hours API Endpoint
-----------------------------------------------------------------------------------------------
@43962@Automated@GetAssetSettings@Positive
Scenario: GetAssetSettings_HappyPath
Given '<Description>' is ready to verify
And GetAssetSettings is setup with default valid values
And I add Asset 
When I try to get asset Details
Then Valid Asset Details response should be returned
And I delete Asset


@43962@Automated@GetAssetSettings@Positive
Scenario Outline: GetAssetSettings_ValidFilters
Given '<Description>' is ready to verify
And GetAssetSettings is setup with default valid values
And I set FilterName as '<FilterName>' and  FilterValue as '<FilterValue>'
And I add Asset 
When I try to get asset Details
Then Valid Asset Details response should be returned
And I delete Asset

Examples: 
| Description              | FilterName        | FilterValue |
| FilterNameAndValue_Valid | assetSerialNumber | Fuel        |


@43962@Automated@GetAssetSettings@Negative
Scenario Outline: GetAssetSettings_InValidFilters
Given '<Description>' is ready to verify
And GetAssetSettings is setup with default valid values
And I set FilterName as '<FilterName>' and  FilterValue as '<FilterValue>'
And I add Asset 
When I try to get asset Details
Then No Asset Details response should be returned
And I delete Asset
Examples: 
| Description        | FilterName        | FilterValue |
| FilterName_Invalid | assetSerialNumber | fuel        |

@43962@Automated@GetAssetSettings@Positive
Scenario Outline:GetAssetSettings_sorting
Given '<Description>' is ready to verify
And GetAssetSettings is setup with default valid values
And I set sortcolumn to '<SortColumn>' and SortingType as '<SortingType>'
And I add Asset 
When I try to get asset Details
Then Valid Asset Details response should be returned based on sortcolumn '<SortColumn>'
And I delete Asset

Examples: 
| Description           | SortColumn        | SortingType |
| SortColumn_AssetID    | AssetID           | ascending   |
| SortColumn_Asset S/N  | assetSerialNumber | descending  |
| SortColumn_Make/Model | Make/Model        | ascending   |
| SortColumn_DeviceID   | DeviceID          | descending  |
| SortColumn_Targets    | Targets           | ascending   |

@43962@Automated@GetAssetSettings@Positive
Scenario Outline:GetAssetSettings_PageNumberAndSize
Given '<Description>' is ready to verify
And GetAssetSettings is setup with default valid values
And I set PageNumber as '<PageNumber>' And PageSize as '<PageSize>'
When I try to get asset Details
Then Valid Asset Details response should be returned

Examples: 
| Description     | PageNumber | PageSize |
| ValidPageNumber | 2          | 20       |


@43962@Automated@GetAssetSettings@Negative
Scenario Outline:GetAssetSettings_PageNumberAndSizeInvalid
Given '<Description>' is ready to verify
And GetAssetSettings is setup with default valid values
And I set PageNumber as '<PageNumber>' And PageSize as '<PageSize>'
When I try to get asset Details
Then Valid Asset Details response should be returned

Examples: 
| Description     | PageNumber | PageSize |
| ValidPageNumber | 20         | 20       |

@43962@Automated@GetAssetSettings@Negative
Scenario:GetAssetSettings_NoUserCustomerRelation
Given '<Description>' is ready to verify
And GetAssetSettings is setup with default valid values
And I map an user to an different customer which is not mapped
When I try to get asset Details
Then Valid Asset Details response should be returned



@48359@Automated@Positive@GetAssetSettings
Scenario: GetAssetSettings_DeviceType_Success
Given '<Description>' is ready to verify
And AddAssetSettings is setup with default valid values
And  I Put Valid asset settings
And I set DeviceType as '<DeviceType>'
 When I try to retrieve asset settings
Then Valid Detials Should be displayed


@48359@Automated@Negative@GetAssetSettings
Scenario:GetAssetSettings_InvalidDeviceType 
Given '<Description>' is ready to verify
And AddAssetSettings is setup with default valid values
And  I Put Valid asset settings
And I set DeviceType with Invalid Value '<DeviceType>'
 When I try to retrieve asset settings
Then Valid Error Response should be thrown