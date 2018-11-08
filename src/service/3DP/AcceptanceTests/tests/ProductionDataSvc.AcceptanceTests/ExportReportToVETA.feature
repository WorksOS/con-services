Feature: ExportReportToVETA
  I should be able to request production data export report for import to VETA.

Scenario Outline: ExportReportToVETA - Good Request
  Given the service route "/api/v2/export/veta" and result repo "ExportReportToVETAResponse.json"
  And with parameter "projectUid" with value "<ProjectUID>"
  And with parameter "filterUid" with value "<FilterUID>"
  And with parameter "machineNames" with value "<MachineNames>"
  And with parameter "fileName" with value "<FileName>"
  And with parameter "coordType" with value "<CoordType>"
  When I send a GET request with Accept header "application/zip" I expect response code 200
  Then the report result csv should match the "<ResultName>" from the repository
Examples: 
  | RequestName            | ProjectUID                           | FilterUID                            | MachineNames                                                                                         | FileName | CoordType | ResultName                    |
  | Selected Machines      | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | d15e65e0-3cb1-476f-8fc6-08507a14a269 | D61 SATD PE,KOMATSU PC210,ACOM,LIEBHERR 924C,CAT CS56B,VOLVO G946B,CASE CX160C,LIEBHERR724,JD 764 CV | Test     | 0         | AllMachinesLongDatesNorthEast |
  | All Machines NorthEast | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | d15e65e0-3cb1-476f-8fc6-08507a14a269 | All                                                                                                  | Test     | 0         | AllMachinesLongDatesNorthEast |
  | All Machines LatLon    | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | d15e65e0-3cb1-476f-8fc6-08507a14a269 | All                                                                                                  | Test     | 1         | AllMachinesLongDatesLatLon    |

Scenario Outline: ExportReportToVETA - Good Request - No Machines
  Given the service route "/api/v2/export/veta" and result repo "ExportReportToVETAResponse.json"
  And with parameter "projectUid" with value "<ProjectUID>"
  And with parameter "filterUid" with value "<FilterUID>"
  And with parameter "fileName" with value "<FileName>"
  And with parameter "coordType" with value "<CoordType>"
  When I send a GET request with Accept header "application/zip" I expect response code 200
  Then the report result csv should match the "<ResultName>" from the repository
Examples: 
  | RequestName | ProjectUID                           | FilterUID                            | FileName | CoordType | ResultName                   |
  | NorthEast   | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | d15e65e0-3cb1-476f-8fc6-08507a14a269 | Test     | 0         | NoMachinesLongDatesNorthEast |
  | LatLon      | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | d15e65e0-3cb1-476f-8fc6-08507a14a269 | Test     | 1         | NoMachinesLongDatesLatLon    |

Scenario Outline: ExportReportToVETA - Bad Request - NoProjectUID
  Given the service route "/api/v2/export/veta" and result repo "ExportReportToVETAResponse.json"
  And with parameter "filterUid" with value "<FilterUID>"
  And with parameter "machineNames" with value "<MachineNames>"
  And with parameter "fileName" with value "<FileName>"
  When I send the GET request I expect response code 400
  Then the response should contain message "<ErrorMessage>" and code "<ErrorCode>"
Examples:
  | RequestName | FilterUID                            | MachineNames | FileName | ErrorCode | ErrorMessage                                  |
  |             | d15e65e0-3cb1-476f-8fc6-08507a14a269 | All          | Test     | -1        | ProjectId and ProjectUID cannot both be null. |

Scenario Outline: ExportReportToVETA - Good Request - NoDateRange
  Given the service route "/api/v2/export/veta" and result repo "ExportReportToVETAResponse.json"
  And with parameter "projectUid" with value "<ProjectUID>"
  And with parameter "machineNames" with value "<MachineNames>"
  And with parameter "fileName" with value "<FileName>"
  And with parameter "coordType" with value "<CoordType>"
  When I send a GET request with Accept header "application/zip" I expect response code 200
  Then the report result csv should match the "<ResultName>" from the repository
Examples:
  | RequestName | ProjectUID                           | MachineNames | FileName | CoordType | ResultName           |
  | NorthEast   | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | All          | Test     | 0         | NoDateRangeNorthEast |
  | LatLon      | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | All          | Test     | 1         | NoDateRangeLatLon    |

Scenario Outline: ExportReportToVETA - Bad Request - NoFileName
  Given the service route "/api/v2/export/veta" and result repo "ExportReportToVETAResponse.json"
  And with parameter "projectUid" with value "<ProjectUID>"
  And with parameter "filterUid" with value "<FilterUID>"
  And with parameter "machineNames" with value "<MachineNames>"
  When I send the GET request I expect response code 400
  Then the response should contain message "<ErrorMessage>" and code "<ErrorCode>"
Examples:
  | RequestName | ProjectUID                           | FilterUID                            | MachineNames | ErrorCode | ErrorMessage             |
  |             | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | d15e65e0-3cb1-476f-8fc6-08507a14a269 | All          | -1        | Missing export file name |

Scenario Outline: ExportReportToVETA - Bad Request with Filter - No Machines
  Given the service route "/api/v2/export/veta" and result repo "ExportReportToVETAResponse.json"
  And with parameter "projectUid" with value "<ProjectUID>"
  And with parameter "filterUid" with value "<FilterUID>"
  And with parameter "fileName" with value "<FileName>"
  When I send the GET request I expect response code 400
  Then the response should contain message "<ErrorMessage>" and code "<ErrorCode>"
Examples: 
  | RequestName | ProjectUID                           | FilterUID                            | FileName | ErrorCode | ErrorMessage                                                                                   |
  |             | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | 1cf81668-1739-42d5-b068-ea025588796a | Test     | -4        | Failed to get/update data requested by CompactionExportExecutor with error: No data for export |

Scenario Outline: ExportReportToVETA - Good Request with Filter - No Machines
  Given the service route "/api/v2/export/veta" and result repo "ExportReportToVETAResponse.json"
  And with parameter "projectUid" with value "<ProjectUID>"
  And with parameter "filterUid" with value "<FilterUID>"
  And with parameter "fileName" with value "<FileName>"
  And with parameter "coordType" with value "<CoordType>"
  When I send a GET request with Accept header "application/zip" I expect response code 200
  Then the report result csv should match the "<ResultName>" from the repository
Examples: 
  | RequestName | ProjectUID                           | FilterUID                            | FileName | CoordType | ResultName          |
  | NorthEast   | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | 81422acc-9b0c-401c-9987-0aedbf153f1d | Test     | 0         | FilterDataNorthEast |
  | LatLon      | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | 81422acc-9b0c-401c-9987-0aedbf153f1d | Test     | 1         | FilterDataLatLon    |
