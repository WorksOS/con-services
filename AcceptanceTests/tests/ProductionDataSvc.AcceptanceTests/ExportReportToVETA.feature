Feature: ExportReportToVETA
I should be able to request production data export report for import to VETA.

Background: 
	Given the Export Report To VETA service URI "/api/v2/export/veta" and the result file "ExportReportToVETAResponse.json"

Scenario Outline: ExportReportToVETA - Good Request
  And projectUid "<ProjectUID>"
	And startUtc "<StartDate>" and endUtc "<EndDate>"
	And machineNames "<MachineNames>"
	And fileName is "<FileName>"
	When I request an Export Report To VETA
	Then the report result should match the "<ResultName>" from the repository
	Examples: 
	| RequestName       | ProjectUID                           | StartDate           | EndDate             | MachineNames                                                                                         | FileName | ResultName           |
	| Selected Machines | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | 2012-11-05T00:00:00 | 2012-11-06T00:00:00 | D61 SATD PE,KOMATSU PC210,ACOM,LIEBHERR 924C,CAT CS56B,VOLVO G946B,CASE CX160C,LIEBHERR724,JD 764 CV | Test     | AllMachinesLongDates |
	| All Machines      | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | 2012-11-05T00:00:00 | 2012-11-06T00:00:00 | All                                                                                                  | Test     | AllMachinesLongDates |
	 
Scenario Outline: ExportReportToVETA - Good Request - No Machines
  And projectUid "<ProjectUID>"
	And startUtc "<StartDate>" and endUtc "<EndDate>"
	And fileName is "<FileName>"
	When I request an Export Report To VETA
	Then the report result should match the "<ResultName>" from the repository
	Examples: 
	| RequestName | ProjectUID                           | StartDate           | EndDate             | FileName | ResultName          |
	|             | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | 2012-11-05T00:00:00 | 2012-11-06T00:00:00 | Test     | NoMachinesLongDates |

Scenario Outline: ExportReportToVETA - Bad Request - NoProjectUID
	And startUtc "<StartDate>" and endUtc "<EndDate>"
	And machineNames "<MachineNames>"
	And fileName is "<FileName>"
	When I request an Export Report To VETA expecting Unauthorized
	Then the report result should contain error code <ErrorCode> and error message "<ErrorMessage>"
	Examples:
	| RequestName | StartDate  | EndDate    | MachineNames | FileName | ErrorCode | ErrorMessage        |
	|             | 2005-01-01 | 2017-06-23 | All          | Test     |  -5       |Missing Project or project does not belong to specified customer or don't have access to the project |

Scenario Outline: ExportReportToVETA - No Content - NoDateRange
	And projectUid "<ProjectUID>"
	And machineNames "<MachineNames>"
	And fileName is "<FileName>"
	When I request an Export Report To VETA expecting NoContent
	Examples:
	| RequestName | ProjectUID                           | MachineNames | FileName | ErrorCode | ErrorMessage                        |
	|             | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | All          | Test     |           |                                     |

Scenario Outline: ExportReportToVETA - Bad Request - NoFileName
	And projectUid "<ProjectUID>"
  And startUtc "<StartDate>" and endUtc "<EndDate>"
	And machineNames "<MachineNames>"
	When I request an Export Report To VETA expecting BadRequest
	Then the report result should contain error code <ErrorCode> and error message "<ErrorMessage>"
	Examples:
	| RequestName | ProjectUID                           | StartDate  | EndDate    | MachineNames | ErrorCode | ErrorMessage                        |
	|             | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | 2005-01-01 | 2017-06-23 | All          | -4        | Failed to get requested export data |


