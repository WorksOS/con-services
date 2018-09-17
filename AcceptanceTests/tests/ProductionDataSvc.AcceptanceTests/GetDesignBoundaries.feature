Feature: GetDesignBoundaries
  I should be able to get boundaries of design surfaces imported into a project.

Background: 
	Given the Get Machine Boundaries service URI "/api/v2/designs/boundaries" and the result file "GetDesignBoundariesResponse.json"

Scenario Outline: GetDesignBoundaries - Good Request - No Designs
  And projectUid "<ProjectUID>"
	And tolerance "<Tolerance>"
	When I request design boundaries
	Then the result should match the "<ResultName>" from the repository
	Examples: 
	| RequestName | ProjectUID                           | Tolerance | ResultName |
	|             | 86a42bbf-9d0e-4079-850f-835496d715c5 | 1.00      | NoDesigns  |

Scenario Outline: GetDesignBoundaries - Good Request
  And projectUid "<ProjectUID>"
	And tolerance "<Tolerance>"
	When I request design boundaries
	Then the result should match the "<ResultName>" from the repository
	Examples: 
	| RequestName    | ProjectUID                           | Tolerance | ResultName    |
	| With Tolerance | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | 1.00      | WithTolerance |

Scenario Outline: GetDesignBoundaries - Good Request - No Tolerance
  And projectUid "<ProjectUID>"
	When I request design boundaries
	Then the result should match the "<ResultName>" from the repository
	Examples: 
	| RequestName | ProjectUID                           | ResultName    |
	|             | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | WithTolerance |

Scenario Outline: GetDesignBoundaries - Bad Request - NoProjectUID
	And tolerance "<Tolerance>"
	When I request design boundaries expecting BadRequest BadRequest
	Then the GetDesignBoundaries result should contain error code <ErrorCode> and error message "<ErrorMessage>"
	Examples: 
	| RequestName | Tolerance | ErrorCode | ErrorMessage                                  |
	|             | 1.00      | -1        | ProjectId and ProjectUID cannot both be null. |
