Feature: StateChangeTracking
	I should be able to track machines.

Scenario: StateChangeTracking - Track a Machine
Given the Tag service URI "/api/v2/tagfiles", Tag request repo file "StateChangeTrackingRequest.json"
And the Machine service URI "/api/v2/projects/ff91dd40-1569-4765-a2bc-014321f76ace/machines/", Machine result repo file "StateChangeTrackingResponse.json"
When I post Tag file "FirstDot" from the Tag request repo
And I get and save the machine detail in one place
Then the first saved machine detail should match "FirstDot" result from the Machine result repo