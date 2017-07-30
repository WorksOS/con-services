Feature: ExportReportMachinePasses
	I should be able to request production data to machine passes export report.

Background: 
	Given the Machine Passes Export Report service URI "/api/v2/export/machinepasses" and the result file "ExportReportMachinePassesResponse.json"

Scenario Outline: ExportReportMachinePasses - Good Request
  And projectUid "<ProjectUID>"
  And startUtc "<StartDate>" and endUtc "<EndDate>"
  And coordType "<CoordType>" 
  And outputType "<OutputType>"
  And restrictOutput "<RestrictOutput>"
  And rawDataOutput "<RawDataOutput>"
	And fileName is "<FileName>"	
	When I request an Export Report Machine Passes
	Then the report result should match the "<ResultName>" from the repository
	Examples: 
	| RequestName                   | ProjectUID                           | StartDate  | EndDate    | CoordType | OutputType | RestrictOutput | RawDataOutput | FileName | ResultName                    |
	| NELastPassUnrestrictedNotRaw  | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | 2012-11-05 | 2012-11-06 | 0         | 0          | false          | false         | Test     | NELastPassUnrestrictedNotRaw  |
  | LLLastPassUnrestrictedNotRaw  | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | 2012-11-05 | 2012-11-06 | 1         | 0          | false          | false         | Test     | LLLastPassUnrestrictedNotRaw  |
  | NEAllPassesUnrestrictedNotRaw | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | 2012-11-05 | 2012-11-06 | 0         | 1          | false          | false         | Test     | NEAllPassesUnrestrictedNotRaw |
  | LLAllPassesUnrestrictedNotRaw | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | 2012-11-05 | 2012-11-06 | 1         | 1          | false          | false         | Test     | LLAllPassesUnrestrictedNotRaw |
  | NELastPassRestrictedNotRaw    | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | 2012-11-05 | 2012-11-06 | 0         | 0          | true           | false         | Test     | NELastPassRestrictedNotRaw    |
  | NELastPassUnrestrictedRaw     | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | 2012-11-05 | 2012-11-06 | 0         | 0          | false          | true          | Test     | NELastPassUnrestrictedRaw     |
  | NELastPassRestrictedRaw       | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | 2012-11-05 | 2012-11-06 | 0         | 0          | true           | true          | Test     | NELastPassRestrictedRaw       |

Scenario Outline: ExportReportMachinePasses - Bad Request - NoProjectUID
	And startUtc "<StartDate>" and endUtc "<EndDate>"
  And coordType "<CoordType>" 
  And outputType "<OutputType>"
  And restrictOutput "<RestrictOutput>"
  And rawDataOutput "<RawDataOutput>"
	And fileName is "<FileName>"	
	When I request an Export Report Machine Passes expecting Unauthorized
	Then the report result should contain error code <ErrorCode> and error message "<ErrorMessage>"
	Examples:
	| RequestName | StartDate  | EndDate    | CoordType | OutputType | RestrictOutput | RawDataOutput | FileName | ErrorCode | ErrorMessage        |
	|             | 2005-01-01 | 2017-06-23 | 0         | 0          | false          | false         | Test     |  -5       | Missing Project or project does not belong to specified customer or don't have access to the project |

Scenario Outline: ExportReportMachinePasses - Bad Request - NoDateRange
	And projectUid "<ProjectUID>"
  And coordType "<CoordType>" 
  And outputType "<OutputType>"
  And restrictOutput "<RestrictOutput>"
  And rawDataOutput "<RawDataOutput>"
	And fileName is "<FileName>"	
	When I request an Export Report Machine Passes expecting BadRequest
	Then the report result should contain error code <ErrorCode> and error message "<ErrorMessage>"
	Examples:
	| RequestName | ProjectUID                           | CoordType | OutputType | RestrictOutput | RawDataOutput | FileName | ErrorCode | ErrorMessage                        |
	|             | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | 0         | 0          | false          | false         | Test     | -4        | Failed to get requested export data |

Scenario Outline: ExportReportMachinePasses - Bad Request - NoFileName
  And projectUid "<ProjectUID>"
	And startUtc "<StartDate>" and endUtc "<EndDate>"
  And coordType "<CoordType>" 
  And outputType "<OutputType>"
  And restrictOutput "<RestrictOutput>"
  And rawDataOutput "<RawDataOutput>"
	When I request an Export Report Machine Passes expecting BadRequest
	Then the report result should contain error code <ErrorCode> and error message "<ErrorMessage>"
	Examples:
	| RequestName | ProjectUID                           | StartDate  | EndDate    | CoordType | OutputType | RestrictOutput | RawDataOutput | ErrorCode | ErrorMessage                        |
	|             | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | 2005-01-01 | 2017-06-23 | 0         | 0          | false          | false         | -4        | Failed to get requested export data |

Scenario Outline: ExportReportMachinePasses - Bad Request
  And projectUid "<ProjectUID>"
	And startUtc "<StartDate>" and endUtc "<EndDate>"
  And coordType "<CoordType>" 
  And outputType "<OutputType>"
  And restrictOutput "<RestrictOutput>"
  And rawDataOutput "<RawDataOutput>"
	When I request an Export Report Machine Passes expecting BadRequest
	Then the report result should contain error code <ErrorCode> and error message "<ErrorMessage>"
	Examples:
	| RequestName                       | ProjectUID                           | StartDate  | EndDate    | CoordType | OutputType | RestrictOutput | RawDataOutput | ErrorCode | ErrorMessage                                         |
  | InvalidCoordType                  | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | 2005-01-01 | 2017-06-23 | 2         | 0          | false          | false         | -2        | Invalid coordinates type for export report           |
  | InvalidOutputType                 | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | 2005-01-01 | 2017-06-23 | 0         | 5          | false          | false         | -2        | Invalid output type for export report                |
  | InvalidOutputTypeForMachinePasses | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | 2005-01-01 | 2017-06-23 | 0         | 2          | false          | false         | -2        | Invalid output type for machine passes export report |