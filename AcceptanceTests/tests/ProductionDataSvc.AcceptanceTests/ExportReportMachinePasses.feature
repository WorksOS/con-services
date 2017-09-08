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

Scenario Outline: ExportReportMachinePasses - No Content - NoDateRange
	And projectUid "<ProjectUID>"
  And coordType "<CoordType>" 
  And outputType "<OutputType>"
  And restrictOutput "<RestrictOutput>"
  And rawDataOutput "<RawDataOutput>"
	And fileName is "<FileName>"	
	When I request an Export Report Machine Passes expecting NoContent
	Examples:
	| RequestName | ProjectUID                           | CoordType | OutputType | RestrictOutput | RawDataOutput | FileName |
	|             | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | 0         | 0          | false          | false         | Test     |

Scenario Outline: ExportReportMachinePasses - No Content with Filter
  And projectUid "<ProjectUID>"
	And filterUid "<FilterUID>"
  And startUtc "<StartDate>" and endUtc "<EndDate>"
  And coordType "<CoordType>" 
  And outputType "<OutputType>"
  And restrictOutput "<RestrictOutput>"
  And rawDataOutput "<RawDataOutput>"
  And fileName is "<FileName>"
	When I request an Export Report Machine Passes expecting NoContent
	Examples:
| RequestName | ProjectUID                           | StartDate  | EndDate    | CoordType | OutputType | RestrictOutput | RawDataOutput | FileName | FilterUID                            |
	|             | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | 2012-11-05 | 2012-11-06 | 1         | 1          | false          | false         | Test     |1cf81668-1739-42d5-b068-ea025588796a |

Scenario Outline: ExportReportMachinePasses - Good Request with Filter
  And projectUid "<ProjectUID>"
  And filterUid "<FilterUID>"
	And startUtc "<StartDate>" and endUtc "<EndDate>"
  And coordType "<CoordType>" 
  And outputType "<OutputType>"
  And restrictOutput "<RestrictOutput>"
  And rawDataOutput "<RawDataOutput>"
  And fileName is "<FileName>"
  When I request an Export Report Machine Passes
  Then the report result should match the "<ResultName>" from the repository
	Examples:
  | RequestName | ProjectUID                           | StartDate  | EndDate    | CoordType | OutputType | RestrictOutput | RawDataOutput | FileName | FilterUID                            | ResultName                    |
	|             | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | 2012-11-05 | 2012-11-06 | 1         | 1          | false          | false         | Test     | d15e65e0-3cb1-476f-8fc6-08507a14a269 | NELastPassFilterRaw           |

Scenario Outline: ExportReportMachinePasses - No Content - NoFileName
  And projectUid "<ProjectUID>"
	And startUtc "<StartDate>" and endUtc "<EndDate>"
  And coordType "<CoordType>" 
  And outputType "<OutputType>"
  And restrictOutput "<RestrictOutput>"
  And rawDataOutput "<RawDataOutput>"
	When I request an Export Report Machine Passes expecting NoContent
	Examples:
	| RequestName | ProjectUID                           | StartDate  | EndDate    | CoordType | OutputType | RestrictOutput | RawDataOutput |
	|             | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | 2005-01-01 | 2017-06-23 | 0         | 0          | false          | false         |

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
  | InvalidCoordType                  | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | 2005-01-01 | 2017-06-23 | 2         | 0          | false          | false         | -1        | Invalid coordinates type for export report           |
  | InvalidOutputType                 | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | 2005-01-01 | 2017-06-23 | 0         | 5          | false          | false         | -1        | Invalid output type for export report                |
  | InvalidOutputTypeForMachinePasses | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | 2005-01-01 | 2017-06-23 | 0         | 2          | false          | false         | -1        | Invalid output type for machine passes export report |