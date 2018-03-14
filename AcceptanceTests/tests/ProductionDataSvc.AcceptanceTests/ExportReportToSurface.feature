Feature: ExportReportToSurface
  I should be able to request production data to surface export report.

Background: 
	Given the Export Report To Surface service URI "/api/v2/export/surface" and the result file "ExportReportToSurfaceResponse.json"

Scenario Outline: ExportReportToSurface - Good Request - With Tolerance
  And projectUid "<ProjectUID>"
	And fileName is "<FileName>"
	And tolerance "<Tolerance>"
	When I request an Export Report To Surface
	Then the export result should successful
	Examples: 
	| RequestName      | ProjectUID                           | Tolerance | FileName             | ResultName                  |
	| No Excluded SS   | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | 1.50      | SurfaceWithTolerance | WithToleranceNoExcludedSS   |
  | With Excluded SS | 86a42bbf-9d0e-4079-850f-835496d715c5 | 1.50      | SurfaceWithTolerance | WithToleranceWithExcludedSS |

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

Scenario Outline: ExportReportToSurface - No Content - NoFileName
	And projectUid "<ProjectUID>"
	And tolerance "<Tolerance>"
	When I request an Export Report To Surface expecting NoContent
	Examples:
	| RequestName | ProjectUID                           | Tolerance |
	|             | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | 0.05      |

Scenario Outline: ExportReportToSurface - Good Request with Filter
  And projectUid "<ProjectUID>"
  And tolerance "<Tolerance>"
	And filterUid "<FilterUID>"
  And fileName is "<FileName>"
	When I request an Export Report To Surface
	Then the export result should successful
	Examples:
| RequestName | ProjectUID                           | FilterUID                             | Tolerance | FileName              |
|             | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | d7cb424d-b012-4618-b3bc-e526ca84bbd6  | 0.05      | SurfanceWithTolerance |