Feature: RetrieveProductivity

User Story: 44878  Implementation - VisionLink Administrator - Productivity Targets

-----------------------------------------------------------------------------------------------

@US44878@Automated@RetrieveProductivity@Positive
Scenario Outline: RetrieveProductivityDetails
Given '<Description>' is ready to verify
And Retrieve Productivity Details is setup with default valid values
And I set startDate as '<StartDate>' and EndDate as '<EndDate>' 
When I retrieve Productivity Details
Then Valid response should be received 

Examples: 
| Description             | StartDate | EndDate  |
| HappyPath               | 1-1-2017  | 5-1-2017 |
| NoAssetDetails          | 2-1-2018  | 2-1-2018 |
| StartDateAndEndDateSame | 2-1-2017  | 2-1-2017 |

@US44878@Automated@RetrieveProductivity@Negative
Scenario Outline: RetrieveProductivityDetails_NoAssetDetails
Given '<Description>' is ready to verify
And Retrieve Productivity Details is setup with default valid values
And I modify  AssetUID as <'assetUID'> 
When I retrieve Productivity Details
Then Valid  Error response should be received 
Examples: 
| Description            | assetUID      |
| assetUID_Null          | NULL_NULL     |
| assetUID_EmptySpace    | EMPTY_EMPTY   |
| assetUID_InvalidString | InvalidString |


@US44878@Automated@RetrieveProductivity@Negative
Scenario Outline:  RetrieveProductivityDetails_InvalidDateRange
Given '<Description>' is ready to verify
And Retrieve Asset Details is setup with default valid values
And I set startDate as '<StartDate>' and EndDate as '<EndDate>' 
When I retrieve Asset Details 
Then Valid Error response should be received 
Examples: 
| Description                 | StartDate | EndDate   |
| EndDateGreaterThanStartDate | 5-1-2017  | 1-1-2017  |


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
When I try to retrieve Productivity details With Start Date as'<RetrieveStartDate>' and RetrieveEndDate as '<RetrieveEndDate>'
Then Updated Productivity details should be shown
Examples: 
| Description                             | AddAssetStartDate | AddAssetEndDate | UpdateAssetStartDate | UpdateAssetEndDate | MultipleOverlapStartDate | MultipleOverlapEndDate | RetrieveStartDate | RetrieveEndDate |
| MultipleOverlap_RetrieveStart           | 2017-1-5          | 2017-1-10       | 2017-1-10            | 2017-1-15          | 2017-1-5                 | 2017-1-15              | 2017-1-1          | 2017-1-5        |
| MultipleOverlap_RetrieveEnd             | 2017-1-5          | 2017-1-10       | 2017-1-10            | 2017-1-15          | 2017-1-5                 | 2017-1-15              | 2017-1-5          | 2017-1-10       |
| MultipleOverlap_RetrieveMiddle          | 2017-1-5          | 2017-1-10       | 2017-1-10            | 2017-1-15          | 2017-1-5                 | 2017-1-15              | 2017-1-11         | 2017-1-15       |
| MultipleOverlap_RetrieveEntireDateRange | 2017-1-5          | 2017-1-10       | 2017-1-10            | 2017-1-15          | 2017-1-5                 | 2017-1-15              | 2017-1-5          | 2017-1-15       |