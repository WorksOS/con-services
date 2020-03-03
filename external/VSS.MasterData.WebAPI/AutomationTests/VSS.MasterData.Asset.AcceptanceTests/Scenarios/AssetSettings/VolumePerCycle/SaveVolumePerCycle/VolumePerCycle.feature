Feature: VolumePerCycle

User Story 45631 : Implementation of estimated volume per cycle	
---------------------------------------------------------------------------------------------------------------------------------------
@Automated @Sanity @Positive
@VolumePerCycleAPIFeature @US45631
Scenario:VolumePerCycleSave_HappyPath
Given VolumePerCycle is ready to verify '<Description>'
   And I create Assets using create asset request
   And VolumePerCycle request is setup with default valid values
When I POST valid VolumePerCycle request
Then Posted volume value retrived should match with DB