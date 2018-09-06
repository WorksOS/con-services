Feature: StateChangeTracking
	I should be able to track machines.

Scenario: StateChangeTracking - Track a Machine
Given the Tag service URI "/api/v2/tagfiles", Tag request repo file "StateChangeTrackingRequest.json"
And the Machine service URI "/api/v2/projects/d0a0410e-9fcc-44b1-bf1a-378c891d2ddb/machines/", Machine result repo file "StateChangeTrackingResponse.json"
When I post Tag file "FirstDot" from the Tag request repo
And I get and save the machine detail in one place
And I post Tag file "SecondDot" from the Tag request repo
And I get and save the machine detail in another place
Then the first saved machine detail should match "FirstDot" result from the Machine result repo
And the second saved machine detail should match "SecondDot" result from the Machine result repo
