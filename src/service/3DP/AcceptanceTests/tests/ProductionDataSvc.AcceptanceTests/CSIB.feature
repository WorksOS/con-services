Feature: CSIB
  I should be able to request the CSIB for a project.

Scenario Outline: Get CSIB for project
  Given the service route "/api/v1/csib" and result repo "CSIBDataResponse.json"
  And with parameter "projectUid" with value "<ProjectUID>"
  When I send the GET request I expect response code <HttpCode>
  Then the response should match "<ResultName>" from the repository
  Examples: 
  | ProjectUID                           | ResultName        | HttpCode |
  |                                      | NoProjectUid      | 401      |
  | 441b2036-990c-4166-a6b3-3f9fda0cc786 | InvalidProjectUid | 401      |
  | b14bb927-3c10-47b2-b958-4ce7aabbc594 | ValidCSIB         | 200      |
