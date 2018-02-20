Feature: CompactionTagFile
	I should be able to POST tag files for compaction.

Background: 
	Give the Compaction Tag file service URI "/api/v2/tagfiles" and request repo "CompactionTagFileRequest.json"

#This test is no longer valid after raymonds change 15/2/2018
@ignore 
Scenario: CompactionTagFile - Bad Request - Returns Failed to process tagfile
	When I POST a compaction tag file with code 100 from the repository
	Then the Tag File Service response should contain Code 2008 and Message "Failed to process tagfile with error: OnChooseMachine. Machine Subscriptions Invalid."

