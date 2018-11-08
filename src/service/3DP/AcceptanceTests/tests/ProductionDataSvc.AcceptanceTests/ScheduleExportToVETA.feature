Feature: ScheduleExportToVETA
  I should be able to request scheduling production data export to VETA.

@ignore
Scenario Outline: ScheduleExportToVETA - Good Request - Schedule Job
  Given the service route "/api/v2/export/veta" and result repo "ScheduleExportToVETAResponse.json"
  And with parameter "projectUid" with value "<ProjectUID>"
  And with parameter "filterUid" with value "<FilterUID>"
  And with parameter "fileName" with value "<FileName>"
  When I send the GET request I expect response code <HttpCode>
  Then the response should match "<ResultName>" from the repository
  Examples: 
  | RequestName     | ProjectUID                           | FilterUID                            | FileName     | ResultName      | HttpCode |
  | SuccessDownload | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | d15e65e0-3cb1-476f-8fc6-08507a14a269 | Test-success | SuccessSchedule | 200      |
