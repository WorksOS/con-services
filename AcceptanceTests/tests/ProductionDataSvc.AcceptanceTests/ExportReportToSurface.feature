Feature: ExportReportToSurface
  I should be able to request production data to surface export report.

Background: 
	Given the Export Report To Surface service URI "/api/v2/export/surface" and the result file "ExportReportToSurfaceResponse.json"

Scenario Outline: ExportReportToSurface - Good Request
  And projectUid "<ProjectUID>"
	And fileName is "<FileName>"
	And tolerance "<Tolerance>"
	When I request an Export Report To Surface
	Then the export result should successful
	Examples: 
	| RequestName    | ProjectUID                           | Tolerance | FileName             | ResultName    |
	| With Tolerance | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | 1.50      | SurfaceWithTolerance | WithTolerance |

Scenario Outline: ExportReportToSurface - Good Request - No Tolerance
  And projectUid "<ProjectUID>"
	And fileName is "<FileName>"
	When I request an Export Report To Surface
	Then the export result should successful
	Examples: 
	| RequestName | ProjectUID                           | FileName           | ResultName  |
	|             | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | SurfaceNoTolerance | NoTolerance |

Scenario Outline: ExportReportToSurface - Bad Request - NoProjectUID
	And fileName is "<FileName>"
	And tolerance "<Tolerance>"
	When I request an Export Report To Surface expecting Unauthorized
	Then the report result should contain error code <ErrorCode> and error message "<ErrorMessage>"
	Examples:
	| RequestName | Tolerance | FileName | ErrorCode | ErrorMessage        |
	|             | 0.05      | Test     |  -5       | Missing Project or project does not belong to specified customer or don't have access to the project |

Scenario Outline: ExportReportToSurface - Bad Request - NoFileName
	And projectUid "<ProjectUID>"
	And tolerance "<Tolerance>"
	When I request an Export Report To Surface expecting BadRequest
	Then the report result should contain error code <ErrorCode> and error message "<ErrorMessage>"
	Examples:
	| RequestName | ProjectUID                           | Tolerance | ErrorCode | ErrorMessage                        |
	|             | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | 0.05      | -4        | Failed to get requested export data |