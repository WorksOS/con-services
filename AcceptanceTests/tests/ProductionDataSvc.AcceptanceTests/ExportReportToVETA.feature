Feature: ExportReportToVETA
I should be able to request production data export report for import to VETA.

Background: 
Given the Export Report To VETA service URI "/api/v2/export/veta" and the result file "ExportReportToVETAResponse.json"

Scenario Outline: ExportReportToVETA - Good Request
And projectUid "<ProjectUID>"
And filterUid "<FilterUID>"
And machineNames "<MachineNames>"
And fileName is "<FileName>"
When I request an Export Report To VETA
Then the report result csv should match the "<ResultName>" from the repository
Examples: 
| RequestName       | ProjectUID                           | FilterUID                            | MachineNames                                                                                         | FileName | ResultName           |
| Selected Machines | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | d15e65e0-3cb1-476f-8fc6-08507a14a269 | D61 SATD PE,KOMATSU PC210,ACOM,LIEBHERR 924C,CAT CS56B,VOLVO G946B,CASE CX160C,LIEBHERR724,JD 764 CV | Test     | AllMachinesLongDates |
| All Machines      | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | d15e65e0-3cb1-476f-8fc6-08507a14a269 | All                                                                                                  | Test     | AllMachinesLongDates |

Scenario Outline: ExportReportToVETA - Good Request - No Machines
And projectUid "<ProjectUID>"
And filterUid "<FilterUID>"
And fileName is "<FileName>"
When I request an Export Report To VETA
Then the report result csv should match the "<ResultName>" from the repository
Examples: 
| RequestName | ProjectUID                           | FilterUID                            | FileName | ResultName          |
|             | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | d15e65e0-3cb1-476f-8fc6-08507a14a269 | Test     | NoMachinesLongDates |

Scenario Outline: ExportReportToVETA - Bad Request - NoProjectUID
And filterUid "<FilterUID>"
And machineNames "<MachineNames>"
And fileName is "<FileName>"
When I request an Export Report To VETA expecting Unauthorized
Then the report result should contain error code <ErrorCode> and error message "<ErrorMessage>"
Examples:
| RequestName | FilterUID                            | MachineNames | FileName | ErrorCode | ErrorMessage                                                                                         |
|             | d15e65e0-3cb1-476f-8fc6-08507a14a269 | All          | Test     |  -5       | Missing Project or project does not belong to specified customer or don't have access to the project |

Scenario Outline: ExportReportToVETA - Good Request - NoDateRange
And projectUid "<ProjectUID>"
And machineNames "<MachineNames>"
And fileName is "<FileName>"
When I request an Export Report To VETA
Then the report result csv should match the "<ResultName>" from the repository
Examples:
| RequestName | ProjectUID                           | MachineNames | FileName | ResultName  |
|             | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | All          | Test     | NoDateRange |

Scenario Outline: ExportReportToVETA - Bad Request - NoFileName
And projectUid "<ProjectUID>"
And filterUid "<FilterUID>"
And machineNames "<MachineNames>"
When I request an Export Report To VETA expecting BadRequest
Then the report result should contain error code <ErrorCode> and error message "<ErrorMessage>"
Examples:
| RequestName | ProjectUID                           | FilterUID                            | MachineNames | ErrorCode | ErrorMessage             |
|             | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | d15e65e0-3cb1-476f-8fc6-08507a14a269 | All          | -1        | Missing export file name |

Scenario Outline: ExportReportToVETA - Bad Request with Filter - No Machines
And projectUid "<ProjectUID>"
And filterUid "<FilterUID>"
And fileName is "<FileName>"
When I request an Export Report To VETA expecting BadRequest
Then the report result should contain error code <ErrorCode> and error message "<ErrorMessage>"
Examples: 
| RequestName | ProjectUID                           | FilterUID                            | FileName | ErrorCode | ErrorMessage                                                       |
|             | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | 1cf81668-1739-42d5-b068-ea025588796a | Test     | 2002      | Failed to get requested export data with error: No data for export |

Scenario Outline: ExportReportToVETA - Good Request with Filter - No Machines
And projectUid "<ProjectUID>"
And filterUid "<FilterUID>"
And fileName is "<FileName>"
When I request an Export Report To VETA
Then the report result csv should match the "<ResultName>" from the repository
Examples: 
| RequestName | ProjectUID                           | FilterUID                            | FileName | ResultName |
|             | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | 81422acc-9b0c-401c-9987-0aedbf153f1d | Test     | FilterData | 