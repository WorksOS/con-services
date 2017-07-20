Feature: ExportReportToSurface
  I should be able to request production data to surface export report.

Background: 
	Given the Export Report To Surface service URI "/api/v2/export/surface" and the result file "ExportReportToSurfaceResponse.json"

Scenario Outline: ExportReportToSurface - Good Request
  And projectUid "<ProjectUID>"
	And fileName is "<FileName>"
	And tolerance "<Tolerance>"
	When I request an Export Report To Surface
	Then the report result should match the "<ResultName>" from the repository
	Examples: 
	| RequetsName    | ProjectUID                           | Tolerance | FileName             | ResultName    |
	| With Tolerance | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | 1.50      | SurfaceWithTolerance | WithTolerance |

@ignore
Scenario Outline: ExportReportToSurface - Good Request - No Tolerance
  And projectUid "<ProjectUID>"
	And fileName is "<FileName>"
	When I request an Export Report To Surface
	Then the report result should match the "<ResultName>" from the repository
	Examples: 
	| RequetsName | ProjectUID                           | FileName           | ResultName  |
	|             | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | SurfaceNoTolerance | NoTolerance |

Scenario Outline: ExportReportToSurface - Bad Request - NoProjectUID
	And fileName is "<FileName>"
	And tolerance "<Tolerance>"
	When I request an Export Report To Surface expecting BadRequest
	Then the report result should contain error code <ErrorCode> and error message "<ErrorMessage>"
	Examples:
	| RequetsName | Tolerance | FileName | ErrorCode | ErrorMessage        |
	|             | 0.05      | Test     | -2        | Missing project UID |

Scenario Outline: ExportReportToSurface - Bad Request - NoFileName
	And projectUid "<ProjectUID>"
	And tolerance "<Tolerance>"
	When I request an Export Report To Surface expecting BadRequest
	Then the report result should contain error code <ErrorCode> and error message "<ErrorMessage>"
	Examples:
	| RequetsName | ProjectUID                           | Tolerance | ErrorCode | ErrorMessage                        |
	|             | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | 0.05      | -4        | Failed to get requested export data |