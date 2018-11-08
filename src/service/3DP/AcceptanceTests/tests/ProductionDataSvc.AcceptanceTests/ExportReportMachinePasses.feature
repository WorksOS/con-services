Feature: ExportReportMachinePasses
  I should be able to request production data to machine passes export report.

Scenario Outline: ExportReportMachinePasses - Good Request
  Given the service route "/api/v2/export/machinepasses" and result repo "ExportReportMachinePassesResponse.json"
  And with parameter "projectUid" with value "<ProjectUID>"
  And with parameter "filterUid" with value "<FilterUID>"
  And with parameter "coordType" with value "<CoordType>"
  And with parameter "outputType" with value "<OutputType>"
  And with parameter "restrictOutput" with value "<RestrictOutput>"
  And with parameter "rawDataOutput" with value "<RawDataOutput>"
  And with parameter "fileName" with value "<FileName>"
  When I send a GET request with Accept header "application/zip" I expect response code 200
  Then the report result csv should match the "<ResultName>" from the repository
  Examples: 
  | RequestName                   | ProjectUID                           | FilterUID                            | CoordType | OutputType | RestrictOutput | RawDataOutput | FileName | ResultName                    |
  | NELastPassUnrestrictedNotRaw  | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | d15e65e0-3cb1-476f-8fc6-08507a14a269 | 0         | 0          | false          | false         | Test     | NELastPassUnrestrictedNotRaw  |
  | LLLastPassUnrestrictedNotRaw  | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | d15e65e0-3cb1-476f-8fc6-08507a14a269 | 1         | 0          | false          | false         | Test     | LLLastPassUnrestrictedNotRaw  |
  | NEAllPassesUnrestrictedNotRaw | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | d15e65e0-3cb1-476f-8fc6-08507a14a269 | 0         | 1          | false          | false         | Test     | NEAllPassesUnrestrictedNotRaw |
  | LLAllPassesUnrestrictedNotRaw | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | d15e65e0-3cb1-476f-8fc6-08507a14a269 | 1         | 1          | false          | false         | Test     | LLAllPassesUnrestrictedNotRaw |
  | NELastPassRestrictedNotRaw    | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | d15e65e0-3cb1-476f-8fc6-08507a14a269 | 0         | 0          | true           | false         | Test     | NELastPassRestrictedNotRaw    |
  | NELastPassUnrestrictedRaw     | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | d15e65e0-3cb1-476f-8fc6-08507a14a269 | 0         | 0          | false          | true          | Test     | NELastPassUnrestrictedRaw     |
  | NELastPassRestrictedRaw       | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | d15e65e0-3cb1-476f-8fc6-08507a14a269 | 0         | 0          | true           | true          | Test     | NELastPassRestrictedRaw       |

Scenario Outline: ExportReportMachinePasses - Bad Request - NoProjectUID
  Given the service route "/api/v2/export/machinepasses" and result repo "ExportReportMachinePassesResponse.json"
  And with parameter "filterUid" with value "<FilterUID>"
  And with parameter "coordType" with value "<CoordType>"
  And with parameter "outputType" with value "<OutputType>"
  And with parameter "restrictOutput" with value "<RestrictOutput>"
  And with parameter "rawDataOutput" with value "<RawDataOutput>"
  And with parameter "fileName" with value "<FileName>"
  When I send the GET request I expect response code 400
  Then the response should contain message "<ErrorMessage>" and code "<ErrorCode>"
  Examples:
  | RequestName | FilterUID                            | CoordType | OutputType | RestrictOutput | RawDataOutput | FileName | ErrorCode | ErrorMessage                                  |
  |             | d15e65e0-3cb1-476f-8fc6-08507a14a269 | 0         | 0          | false          | false         | Test     | -1        | ProjectId and ProjectUID cannot both be null. |

Scenario Outline: ExportReportMachinePasses - Good Request - NoDateRange
  Given the service route "/api/v2/export/machinepasses" and result repo "ExportReportMachinePassesResponse.json"
  And with parameter "projectUid" with value "<ProjectUID>"
  And with parameter "coordType" with value "<CoordType>"
  And with parameter "outputType" with value "<OutputType>"
  And with parameter "restrictOutput" with value "<RestrictOutput>"
  And with parameter "rawDataOutput" with value "<RawDataOutput>"
  And with parameter "fileName" with value "<FileName>"
  When I send a GET request with Accept header "application/zip" I expect response code 200
  Then the report result csv should match the "<ResultName>" from the repository
  Examples:
  | RequestName | ProjectUID                           | CoordType | OutputType | RestrictOutput | RawDataOutput | FileName | ResultName  |
  |             | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | 0         | 0          | false          | false         | Test     | NoDateRange |

Scenario Outline: ExportReportMachinePasses - BadRequest with Filter
  Given the service route "/api/v2/export/machinepasses" and result repo "ExportReportMachinePassesResponse.json"
  And with parameter "projectUid" with value "<ProjectUID>"
  And with parameter "filterUid" with value "<FilterUID>"
  And with parameter "coordType" with value "<CoordType>"
  And with parameter "outputType" with value "<OutputType>"
  And with parameter "restrictOutput" with value "<RestrictOutput>"
  And with parameter "rawDataOutput" with value "<RawDataOutput>"
  And with parameter "fileName" with value "<FileName>"
  When I send the GET request I expect response code 400
  Then the response should contain message "<ErrorMessage>" and code "<ErrorCode>"
  Examples:
  | RequestName | ProjectUID                           | FilterUID                            | CoordType | OutputType | RestrictOutput | RawDataOutput | FileName | ErrorCode | ErrorMessage                                                                                   |
  |             | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | 1cf81668-1739-42d5-b068-ea025588796a | 1         | 1          | false          | false         | Test     | -4        | Failed to get/update data requested by CompactionExportExecutor with error: No data for export |

Scenario Outline: ExportReportMachinePasses - Good Request with Filter
  Given the service route "/api/v2/export/machinepasses" and result repo "ExportReportMachinePassesResponse.json"
  And with parameter "projectUid" with value "<ProjectUID>"
  And with parameter "filterUid" with value "<FilterUID>"
  And with parameter "coordType" with value "<CoordType>"
  And with parameter "outputType" with value "<OutputType>"
  And with parameter "restrictOutput" with value "<RestrictOutput>"
  And with parameter "rawDataOutput" with value "<RawDataOutput>"
  And with parameter "fileName" with value "<FileName>"
  When I send a GET request with Accept header "application/zip" I expect response code 200
  Then the report result csv should match the "<ResultName>" from the repository
  Examples:
  | RequestName | ProjectUID                           | FilterUID                            | CoordType | OutputType | RestrictOutput | RawDataOutput | FileName | ResultName          |
  |             | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | d15e65e0-3cb1-476f-8fc6-08507a14a269 | 1         | 1          | false          | false         | Test     | NELastPassFilterRaw |
  | FilterArea  | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | a37f3008-65e5-44a8-b406-9a078ec62ece | 1         | 1          | false          | false         | Test     | FilterBoundary      |

Scenario Outline: ExportReportMachinePasses - Bad Request - NoFileName
  Given the service route "/api/v2/export/machinepasses" and result repo "ExportReportMachinePassesResponse.json"
  And with parameter "projectUid" with value "<ProjectUID>"
  And with parameter "filterUid" with value "<FilterUID>"
  And with parameter "coordType" with value "<CoordType>"
  And with parameter "outputType" with value "<OutputType>"
  And with parameter "restrictOutput" with value "<RestrictOutput>"
  And with parameter "rawDataOutput" with value "<RawDataOutput>"
  When I send the GET request I expect response code 400
  Then the response should contain message "<ErrorMessage>" and code "<ErrorCode>"
  Examples:
  | RequestName | ProjectUID                           | FilterUID                            | CoordType | OutputType | RestrictOutput | RawDataOutput | ErrorCode | ErrorMessage             |
  |             | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | d15e65e0-3cb1-476f-8fc6-08507a14a269 | 0         | 0          | false          | false         | -1        | Missing export file name |

Scenario Outline: ExportReportMachinePasses - Bad Request
  Given the service route "/api/v2/export/machinepasses" and result repo "ExportReportMachinePassesResponse.json"
  And with parameter "projectUid" with value "<ProjectUID>"
  And with parameter "filterUid" with value "<FilterUID>"
  And with parameter "coordType" with value "<CoordType>"
  And with parameter "outputType" with value "<OutputType>"
  And with parameter "restrictOutput" with value "<RestrictOutput>"
  And with parameter "rawDataOutput" with value "<RawDataOutput>"
  When I send the GET request I expect response code 400
  Then the response should contain message "<ErrorMessage>" and code "<ErrorCode>"
  Examples:
  | RequestName                       | ProjectUID                           | FilterUID                            | CoordType | OutputType | RestrictOutput | RawDataOutput | ErrorCode | ErrorMessage                                         |
  | InvalidCoordType                  | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | d15e65e0-3cb1-476f-8fc6-08507a14a269 | 2         | 0          | false          | false         | -1        | Invalid coordinates type for export report           |
  | InvalidOutputType                 | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | d15e65e0-3cb1-476f-8fc6-08507a14a269 | 0         | 5          | false          | false         | -1        | Invalid output type for export report                |
  | InvalidOutputTypeForMachinePasses | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | d15e65e0-3cb1-476f-8fc6-08507a14a269 | 0         | 2          | false          | false         | -1        | Invalid output type for machine passes export report |
