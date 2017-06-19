Feature: CompactionTagFile
	I should be able to POST tag files for compaction.

Background: 
	Given the Compaction Tag file service URI "/api/v2/tagfiles" and request repo "CompactionTagFileRequest.json"

Scenario: CompactionTagFile - Good Request
	When I POST a compaction tag file with code 100 from the repository
	Then the Tag File Service response should contain Code 0 and Message "success"

