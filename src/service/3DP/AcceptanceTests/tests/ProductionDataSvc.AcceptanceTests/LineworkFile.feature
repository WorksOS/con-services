Feature: LineworkFile

@ignore
Scenario Outline: LineworkFile - Bad Request
  Given the service route "/api/v2/linework/boundaries" and result repo "LineworkFileResponse.json"
  And with parameter "projectUid" with value "<ProjectUID>"
  And with parameter "designUid" with value "<DesignUID>"
  When I send the GET request I expect response code <HttpCode>
  Then the response should contain code <ErrorCode>
  Examples: 
  | ProjectUID                           | DesignUID                            | ErrorCode | HttpCode |
  |                                      | dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff | -1        | 400      |
  | ff91dd40-1569-4765-a2bc-014321f76ace |                                      | -1        | 400      |
  | ff91dd40-1569-4765-a2bc-014321f76ace | 54C5FFE4-42B4-4A39-95AB-4E2CC04245E0 | -1        | 400      |
