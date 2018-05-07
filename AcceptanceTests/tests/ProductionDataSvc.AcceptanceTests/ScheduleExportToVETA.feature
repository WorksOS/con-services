Feature: ScheduleExportToVETA
I should be able to request scheduling production data export to VETA.

Scenario Outline: ScheduleExportToVETA - Good Request - Schedule Job
Given the Export Report To VETA service URI "/api/v2/export/veta" for operation "schedulejob" and the result file "ScheduleExportToVETAResponse.json"
And projectUid "<ProjectUID>"
And filterUid "<FilterUID>"
And fileName "<FileName>"
When I request a Schedule Export To VETA
Then the result should match the "<ResultName>" from the repository
Examples: 
| RequestName | ProjectUID                           | FilterUID                            | FileName      | ResultName          |
|             | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | d15e65e0-3cb1-476f-8fc6-08507a14a269 | Test-success  | SuccessSchedule     |





