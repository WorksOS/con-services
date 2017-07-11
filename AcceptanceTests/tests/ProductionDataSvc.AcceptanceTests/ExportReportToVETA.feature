Feature: ExportReportToVETA
I should be able to request production data export report for import to VETA.

Background: 
	Given the Export Report To VETA service URI "v2/export/veta" and the result file "ExportReportToVETAResponse.json"

Scenario Outline: ExportReportToVETA - Good Request
  And projectUid "7925f179-013d-4aaf-aff4-7b9833bb06d6"
	And startUtc "<StartDate>" and endUtc "<EndDate>"
	And machineNames "D61 SATD PE,KOMATSU PC210,ACOM,LIEBHERR 924C,CAT CS56B,VOLVO G946B,CASE CX160C,LIEBHERR724,JD 764 CV"
	And fileName is "<FileName>"
	When I request an Export Report To VETA
	Then the report result should match the "<ResultName>" from the repository
	Examples: 
	| ProjectUID                           | StartDate           | EndDate                      | MachineNames                                                                                         | FileName | ResultName           |
	| 7925f179-013d-4aaf-aff4-7b9833bb06d6 | 2005-01-01T00:00:00 | 2017-06-23T03:19:35.2130131Z | D61 SATD PE,KOMATSU PC210,ACOM,LIEBHERR 924C,CAT CS56B,VOLVO G946B,CASE CX160C,LIEBHERR724,JD 764 CV | Test     | AllMachinesLongDates |
	| 7925f179-013d-4aaf-aff4-7b9833bb06d6 | 2005-01-01T00:00:00 | 2017-06-23T03:19:35.2130131Z | All                                                                                                  | Test     | AllMachinesLongDates |
	 

@ignore
Scenario Outline: ExportReportToVETA - Bad Request
  And projectUid "<ProjectUID>"
	And startUtc "<StartDate>" and endUtc "<EndDate>"
	And machineNames "<MachineNames>"
	And fileName is "<FileName>"
	When I request an Export Report To VETA
	Then the report result should match the "<ResultName>" from the repository
	Examples: 
	| ProjectUID                           | StartDate                    | EndDate                      | MachineNames                                                                                         | FileName | ResultName |
	| null                                 | 2005-01-01T00:00:00          | 2017-06-23T03:19:35.2130131Z | D61 SATD PE,KOMATSU PC210,ACOM,LIEBHERR 924C,CAT CS56B,VOLVO G946B,CASE CX160C,LIEBHERR724,JD 764 CV | Test     | null       |
	| 7925f179-013d-4aaf-aff4-7b9833bb06d6 | 2017-06-23T03:19:35.2130131Z | 2005-01-01T00:00:00          | D61 SATD PE,KOMATSU PC210,ACOM,LIEBHERR 924C,CAT CS56B,VOLVO G946B,CASE CX160C,LIEBHERR724,JD 764 CV | Test     | null       |
	| 7925f179-013d-4aaf-aff4-7b9833bb06d6 | 2005-01-01T00:00:00          | 2017-06-23T03:19:35.2130131Z | All                                                                                                  | Test     | null       |


