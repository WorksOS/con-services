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

Scenario Outline: ScheduleExportToVETA - Good Request - Get Status
Given the Export Report To VETA service URI "/api/v2/export/veta" for operation "status" and the result file "ScheduleExportToVETAResponse.json"
And projectUid "<ProjectUID>"
And JobId "<JobId>"
When I request a Export To VETA Status
Then the result should match the "<ResultName>" from the repository
Examples: 
| RequestName | ProjectUID                           | JobId        | ResultName          |
|             | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | Test_Job_1   | SuccessStatus       |
|             | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | Test_Job_3   | InProgressStatus    |

Scenario Outline: ScheduleExportToVETA - Bad Request - Get Status
Given the Export Report To VETA service URI "/api/v2/export/veta" for operation "status" and the result file "ScheduleExportToVETAResponse.json"
And projectUid "<ProjectUID>"
And JobId "<JobId>"
When I request an Export To VETA Status expecting InternalServerError
Then the report result should contain error code <ErrorCode> and error message "<ErrorMessage>"
Examples: 
| RequestName | ProjectUID                           | JobId        | ErrorCode | ErrorMessage               |
|             | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | Test_Job_2   |  -4       | Job failed for some reason |

# Causing errors in alpha
@Ignore
Scenario Outline: ScheduleExportToVETA - Good Request - Download
Given the Export Report To VETA service URI "/api/v2/export/veta" for operation "download" and the result file "ScheduleExportToVETAResponse.json"
And projectUid "<ProjectUID>"
And JobId "<JobId>"
When I request a Export To VETA Download
Then the result should match the "<ResultName>" from the repository
Examples: 
| RequestName | ProjectUID                           |  JobId        | ResultName      |
|             | 7925f179-013d-4aaf-aff4-7b9833bb06d6 |  Test_Job_1   | SuccessDownload |
#FilterData from ExportReportToVETA feature

Scenario Outline: ScheduleExportToVETA - Bad Request - Download
Given the Export Report To VETA service URI "/api/v2/export/veta" for operation "download" and the result file "ScheduleExportToVETAResponse.json"
And projectUid "<ProjectUID>"
And JobId "<JobId>"
When I request an Export To VETA Download expecting InternalServerError
Then the report result should contain error code <ErrorCode> and error message "<ErrorMessage>"
Examples: 
| RequestName | ProjectUID                           | JobId        | ErrorCode | ErrorMessage                              |
|             | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | Test_Job_2   |  -4       | File is likely not ready to be downloaded |



