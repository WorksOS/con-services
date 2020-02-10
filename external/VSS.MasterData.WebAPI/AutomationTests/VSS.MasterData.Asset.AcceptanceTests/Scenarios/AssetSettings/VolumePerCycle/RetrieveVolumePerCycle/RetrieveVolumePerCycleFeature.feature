Feature: SaveVolumePerCycle

User Story 45631 : Implementation of estimated volume per cycle	
---------------------------------------------------------------------------------------------------------------------------------------

@Automated @Sanity @Positive
@VolumePerCycleAPIFeature @US45631
Scenario: VolumePerCycle_HappyPath
	Given SaveVolumePerCycle is ready to verify '<Description>'
   And I create Assets using create asset  request
   And SaveVolumePerCycle request is setup with default valid values
When I PUT valid VolumePerCycle request
Then saved volume value retrived should match with DB `

@Automated @Regression @Positive
@MileageTargetAPIFeature @US43963
Scenario Outline: MileageTargetAPI_NumberOfAssetUIDs
Given SaveVolumePerCycle is ready to verify '<Description>'
   And I create Assets'<numberOfAssets>' using create asset request
When I Set '<AssetUids>' to volumepercycle Request
 And I POST valid VolumePerCycle request
Then saved volume value retrived should match with DB `
Examples:
| numberOfAssetUIDs |
| 1                 |
| 5                 |
| 15                |
| 30                |