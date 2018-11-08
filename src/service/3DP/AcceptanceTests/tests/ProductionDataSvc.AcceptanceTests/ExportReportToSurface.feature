Feature: ExportReportToSurface
  I should be able to request production data to surface export report.

Scenario Outline: ExportReportToSurface - Good Request - With Tolerance
  Given the service route "/api/v2/export/surface" and result repo "ExportReportToSurfaceResponse.json"
  And with parameter "projectUid" with value "<ProjectUID>"
  And with parameter "fileName" with value "<FileName>"
  And with parameter "tolerance" with value "<Tolerance>"
  When I send a GET request with Accept header "application/zip" I expect response code 200
  Then the export result should be of a minimum length
  Examples: 
  | RequestName      | ProjectUID                           | Tolerance | FileName             | ResultName                  |
  | No Excluded SS   | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | 1.50      | SurfaceWithTolerance | WithToleranceNoExcludedSS   |
  | With Excluded SS | 86a42bbf-9d0e-4079-850f-835496d715c5 | 1.50      | SurfaceWithTolerance | WithToleranceWithExcludedSS |

Scenario Outline: ExportReportToSurface - Good Request - No Tolerance
  Given the service route "/api/v2/export/surface" and result repo "ExportReportToSurfaceResponse.json"
  And with parameter "projectUid" with value "<ProjectUID>"
  And with parameter "fileName" with value "<FileName>"
  When I send a GET request with Accept header "application/zip" I expect response code 200
  Then the export result should be of a minimum length
  Examples: 
  | RequestName | ProjectUID                           | FileName           | ResultName  |
  |             | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | SurfaceNoTolerance | NoTolerance |

Scenario Outline: ExportReportToSurface - Bad Request - NoProjectUID
  Given the service route "/api/v2/export/surface" and result repo "ExportReportToSurfaceResponse.json"
  And with parameter "fileName" with value "<FileName>"
  And with parameter "tolerance" with value "<Tolerance>"
  When I send the GET request I expect response code 400
  Then the response should contain message "<ErrorMessage>" and code "<ErrorCode>"
  Examples:
  | RequestName | Tolerance | FileName | ErrorCode | ErrorMessage                                                                                                                              |
  |             | 0.05      | Test     | -1        | ProjectId and ProjectUID cannot both be null. |

Scenario Outline: ExportReportToSurface - Bad Request - NoFileName
  Given the service route "/api/v2/export/surface" and result repo "ExportReportToSurfaceResponse.json"
  And with parameter "projectUid" with value "<ProjectUID>"
  And with parameter "tolerance" with value "<Tolerance>"
  When I send the GET request I expect response code 400
  Then the response should contain message "<ErrorMessage>" and code "<ErrorCode>"
  Examples:
  | RequestName | ProjectUID                           | Tolerance | ErrorCode | ErrorMessage             |
  |             | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | 0.05      | -1        | Missing export file name |

Scenario Outline: ExportReportToSurface - Good Request with Filter
  Given the service route "/api/v2/export/surface" and result repo "ExportReportToSurfaceResponse.json"
  And with parameter "projectUid" with value "<ProjectUID>"
  And with parameter "filterUid" with value "<FilterUID>"
  And with parameter "fileName" with value "<FileName>"
  And with parameter "tolerance" with value "<Tolerance>"
  When I send a GET request with Accept header "application/zip" I expect response code 200
  Then the export result should be of a minimum length
  Examples:
| RequestName | ProjectUID                           | FilterUID                             | Tolerance | FileName              |
|             | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | d7cb424d-b012-4618-b3bc-e526ca84bbd6  | 0.05      | SurfanceWithTolerance |
