Feature: AlignmentLinework

Scenario Outline: AlignmentLinework - Bad Request
  Given the service route "/api/v2/linework/alignment" and result repo "AlignmentLineworkResponse.json"
  And with parameter "projectUid" with value "<ProjectUID>"
  And with parameter "alignmentUid" with value "<AlignmentUID>"
  When I send the GET request I expect response code <HttpCode>
  Then the response should contain message "<ErrorMessage>" and code "<ErrorCode>"
  Examples: 
  | ProjectUID                           | AlignmentUID                         | HttpCode | ErrorCode | ErrorMessage                                  |
  |                                      | dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff | 401      | -5        | Missing Project or project does not belong to specified customer or don't have access to the project 00000000-0000-0000-0000-000000000000 |
  | ff91dd40-1569-4765-a2bc-014321f76ace |                                      | 400      | -1        | Unable to access design or alignment file.    |

Scenario Outline: AlignmentLinework - Good Request
  Given the service route "/api/v2/linework/alignment" and result repo "AlignmentLineworkResponse.json"
  And with parameter "projectUid" with value "<ProjectUID>"
  And with parameter "alignmentUid" with value "<AlignmentUID>"
  When I send a GET request with Accept header "application/zip" I expect response code 200
  Then the report result dxf should match the "<ResultName>" from the repository
  Examples: 
  | ProjectUID                           | AlignmentUID                         | HttpCode | ResultName       |
  | ff91dd40-1569-4765-a2bc-014321f76ace | 6ece671b-7959-4a14-86fa-6bfe6ef4dd62 | 200      | Large Sites Road |
