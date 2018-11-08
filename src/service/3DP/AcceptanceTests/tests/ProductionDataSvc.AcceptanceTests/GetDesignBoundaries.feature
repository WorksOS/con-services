Feature: GetDesignBoundaries
  I should be able to get boundaries of design surfaces imported into a project.

Scenario Outline: GetDesignBoundaries - Good Request - No Designs
  Given the service route "/api/v2/designs/boundaries" and result repo "GetDesignBoundariesResponse.json"
  And with parameter "projectUid" with value "<ProjectUID>"
  And with parameter "tolerance" with value "<Tolerance>"
  When I send the GET request I expect response code <HttpCode>
  Then the response should match "<ResultName>" from the repository
  Examples: 
  | RequestName | ProjectUID                           | Tolerance | ResultName | HttpCode |
  |             | 86a42bbf-9d0e-4079-850f-835496d715c5 | 1.00      | NoDesigns  | 200      |

Scenario Outline: GetDesignBoundaries - Good Request
  Given the service route "/api/v2/designs/boundaries" and result repo "GetDesignBoundariesResponse.json"
  And with parameter "projectUid" with value "<ProjectUID>"
  And with parameter "tolerance" with value "<Tolerance>"
  When I send the GET request I expect response code <HttpCode>
  Then the response should match "<ResultName>" from the repository
  Examples: 
  | RequestName    | ProjectUID                           | Tolerance | ResultName    | HttpCode |
  | With Tolerance | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | 1.00      | WithTolerance | 200      |

Scenario Outline: GetDesignBoundaries - Good Request - No Tolerance
  Given the service route "/api/v2/designs/boundaries" and result repo "GetDesignBoundariesResponse.json"
  And with parameter "projectUid" with value "<ProjectUID>"
  When I send the GET request I expect response code <HttpCode>
  Then the response should match "<ResultName>" from the repository
  Examples: 
  | RequestName | ProjectUID                           | ResultName    | HttpCode |
  |             | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | WithTolerance | 200      |

Scenario Outline: GetDesignBoundaries - Bad Request - NoProjectUID
  Given the service route "/api/v2/designs/boundaries" and result repo "GetDesignBoundariesResponse.json"
  And with parameter "tolerance" with value "<Tolerance>"
  When I send the GET request I expect response code <HttpCode>
  Then the response should contain message "<ErrorMessage>" and code "<ErrorCode>"
  Examples: 
  | RequestName | Tolerance | ErrorCode | ErrorMessage                                  | HttpCode |
  |             | 1.00      | -1        | ProjectId and ProjectUID cannot both be null. | 400      |
