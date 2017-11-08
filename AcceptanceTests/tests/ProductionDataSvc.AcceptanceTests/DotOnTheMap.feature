Feature: DotOnTheMap
	I should be able to track machines.
#@ignore
Scenario: DotOnTheMap - Track a Machine
Given the Tag service URI "/api/v1/tagfiles", Tag request repo file "DotOnTheMapRequest.json"
And the Machine service URI "/api/v1/projects/1001210/machines/", Machine result repo file "DotOnTheMapResponse.json"
When I post Tag file "FirstDot" from the Tag request repo
And I get and save the machine detail in one place
And I post Tag file "SecondDot" from the Tag request repo
And I get and save the machine detail in another place
Then the first saved machine detail should match "FirstDot" result from the Machine result repo
And the second saved machine detail should match "SecondDot" result from the Machine result repo
