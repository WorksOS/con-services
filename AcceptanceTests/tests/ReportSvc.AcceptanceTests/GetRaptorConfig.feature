Feature: RaptorConfig
	I should be able to get configuration for Raptor

Background: 
	Given the Raptor Config service URI "/api/v1/configuration"

Scenario: RaptorConfig - Request Raptor Config
	When I try to get config for Raptor
	Then the response should contain code 0 and message "success"
#	And the config should contain correct tags
