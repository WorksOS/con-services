Feature: AddProductivity

User Story: 44878  Implementation - VisionLink Administrator - Productivity Targets

-----------------------------------------------------------------------------------------------

@US44878@Automated@AddProductivity@Positive@DeleteExistingRecords
Scenario:AddProductivity_HappyPath
Given '<Description>' is ready to verify
And AddProductivity is setup with default valid values
When I Put Valid Productivity details for asset
 And When I try to retrieve Productivity details
Then Valid details should be displayed

@US44878@Automated@AddProductivity@Positive
Scenario Outline:AddProductivity_ValidValuesForAssetTargets
Given '<Description>' is ready to verify
And AddProductivity is setup with default valid values
And I modify  in '<AssetTargetName>' value to be  '<Value>'
When I Put Valid Productivity details for asset
 And When I try to retrieve Productivity details
Then Valid details should be displayed
Examples: 
| Description                | AssetTargetName | Value |
| TargetCycles_MinValue      | targetcycles    | 0     |
| TargetCycles_DecimalValue  | targetcycles    | 1   |
| TargetCycles_MaxValue      | targetcycles    | 5432  |

| TargetVolumes_MinValue     | targetvolumes   | 0     |
| TargetVolumes_DecimalValue | targetvolumes   | 1.2   |
| TargetVolumes_MaxValue     | targetvolumes   | 5432  |

| TargetPayload_MinValue     | TargetPayload   | 0     |
| TargetPayload_DecimalValue | TargetPayload   | 1.2   |
| TargetPayload_MaxValue     | TargetPayload   | 5432  |

 @US44878@Automated@AddProductivity@Negative
Scenario Outline:AddProductivity_InvalidValuesForAssetTargets
Given '<Description>' is ready to verify
And AddProductivity is setup with default valid values
And I modify  in '<AssetTargetName>' value to be  '<Value>'
When I Put Invalid Productivity details for asset
Then Valid Error Code <ErrorCode> should be shown
Examples: 
| Description                          | AssetTargetName | Value  | ErrorCode |
| TargetCycles_NegativeValue           | targetcycles    | -4     | 400108    |
| TargetCycles_DecimalValueMoreThanMax | targetcycles    | 1.2987 | 400108    |
| TargetCycles_MaxValue                | targetcycles    | 54321  | 400108    |

| TargetVolumes_MinValue     | targetvolumes | -100    | 400108 |
| TargetVolumes_DecimalValue | targetvolumes | 45.2345 | 400108 |
| TargetVolumes_MaxValue     | targetvolumes | 54324   | 400108 |

| TargetPayload_MinValue     | TargetPayload | -0.12 | 400108 |
| TargetPayload_DecimalValue | TargetPayload | 1.298 | 400108 |
| TargetPayload_MaxValue     | TargetPayload | 54322 | 400108 |

@US44878@Automated@AddProductivity@Negative
Scenario Outline:AddProductivity_InvalidAssetUID
Given '<Description>' is ready to verify
And AddProductivity is setup with default valid values
And I modify  AssetUID as <'assetUID'> 
When I Put Valid Productivity details for asset
 And When I try to retrieve Productivity details
Then Valid Error Code should be shown
Examples: 
| Description            | assetUID      |
| assetUID_Null          | NULL_NULL     |
| assetUID_EmptySpace    | EMPTY_EMPTY   |
| assetUID_InvalidString | InvalidString |

@US44878@Automated@AddProductivity@Negative
Scenario Outline:AddProductivity_InvalidStartDateAndEndDate
Given '<Description>' is ready to verify
And AddProductivity is setup with default valid values
When I modify  startdate as '<AddAssetStartDate>' and EndDate as '<AddAssetEndDate>'
And  I try to retrieve Productivity details
Then Valid ErrorResponse Should be shown

Examples: 
| Description                 | AddAssetStartDate | AddAssetEndDate |
| InvalidStartAndEndDate      | Today             | Tomorrow        |
| StartDateGreaterThanEndDate | 20-1-2017         | 1-1-2017        |

@US44878@Automated@AddProductivity@Positive
Scenario Outline:AddProductivity_OverLappingScenarios
Given '<Description>' is ready to verify
And AddProductivity is setup with default valid values
And I modify  startdate as '<AddAssetStartDate>' and EndDate as '<AddAssetEndDate>'
And I Put Valid Productivity details for asset
And I modify  startdate as '<UpdateAssetStartDate>' and EndDate as '<UpdateAssetEndDate>'
And I Put Valid Productivity details for asset
When I try to retrieve Productivity details
Then Updated Productivity details should be shown
Examples: 
| Description           | AddAssetStartDate | AddAssetEndDate | UpdateAssetStartDate | UpdateAssetEndDate |
| StartDateOverlap      | 2017-1-20         | 2017-1-26       | 2017-1-18            | 2017-1-22          |
| EndDateOverlap        | 2017-1-20         | 2017-1-26       | 2017-1-24            | 2017-1-30          |
| FullOverlap           | 2017-1-20         | 2017-1-26       | 2017-1-20            | 2017-1-26          |
| SmallOverlapInBetween | 2017-1-20         | 2017-1-26       | 2017-1-22            | 2017-1-25          |


@US44878@Automated@AddProductivity@Positive
Scenario Outline:AddProductivity_MultipleOverlap
Given '<Description>' is ready to verify
And AddProductivity is setup with default valid values
And I modify  startdate as '<AddAssetStartDate>' and EndDate as '<AddAssetEndDate>'
And I Put Valid Productivity details for asset
And I modify  startdate as '<UpdateAssetStartDate>' and EndDate as '<UpdateAssetEndDate>'
And I Put Valid Productivity details for asset
And I modify  startdate as '<MultipleOverlapStartDate>' and EndDate as '<MultipleOverlapEndDate>'
And I Put Valid Productivity details for asset
When I try to retrieve Productivity details
Then Updated Productivity details should be shown
Examples: 
| Description     | AddAssetStartDate | AddAssetEndDate | UpdateAssetStartDate | UpdateAssetEndDate | MultipleOverlapStartDate | MultipleOverlapEndDate |
| MultipleOverlap | 2017-1-5          | 2017-1-10       | 2017-1-10            | 2017-1-15          | 2017-1-5                 | 2017-1-15              |

@US44878@Automated@AddProductivity@Positive
Scenario:AddProductivity_NoUserCustomerRelation
Given '<Description>' is ready to verify
And AddProductivity is setup with default valid values
And I map an user to an different customer which is not mapped
When I try to add asset Details
Then Valid Asset Details response should be returned
And Valid Error Response should be thrown
