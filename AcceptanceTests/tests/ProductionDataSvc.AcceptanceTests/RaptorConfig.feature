Feature: RaptorConfig
  I should be able to get configuration for Raptor

Scenario: RaptorConfig - Request Raptor Config
  Given only the service route "/api/v1/configuration"
  When I send the GET request I expect response code 200
  Then the response should contain message "success" and code "0"
  And the config should contain correct tags
