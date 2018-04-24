Feature: Report
The report feature produce json object that is used by the reporting service to build an xls file. 

Scenario Outline: Grid Report
  Given the report service uri "/api/v2/report/grid"
  And the result file 'ReportGridResponse.json'
  And I set request parameters projectUid 'ff91dd40-1569-4765-a2bc-014321f76ace' and filterUid '<filterUid>'
  And I select columns '<Elevation>' '<CMV>' '<MDP>' '<PassCount>' '<Temperature>' '<CutFill>'
  And I select grid report parameters '<cutfillDesignUid>' '<gridInterval>' '<gridReportOption>' '<startNorthing>' '<startEasting>' '<endNorthing>' '<endEasting>' '<azimuth>'
  When I request a report
  Then the grid report result should match the '<ResultName>' from the repository
  Examples:
  #  Y or N                                                                   Design UID                            metres          
  | ResultName  | Elevation | CMV | MDP | PassCount | Temperature | CutFill | cutfillDesignUid                     | gridInterval | gridReportOption | startNorthing | startEasting | endNorthing | endEasting | azimuth | filterUid                            |
  | AllColumns  | Y         | Y   | Y   | Y         | Y           | Y       | dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff | 1.0          | Automatic        | 1146.36       | 2889.8       | 1300.0      | 2700.0     | 10.0    | 9c27697f-ea6d-478a-a168-ed20d6cd9a20 |
  | Elevation   | Y         | N   | N   | N         | N           | N       |                                      | 1.5          | Automatic        | 1146.36       | 2889.8       | 1300.0      | 2700.0     | 5.0     | 9c27697f-ea6d-478a-a168-ed20d6cd9a20 |
  | ElevPcTemp  | Y         | N   | N   | Y         | Y           | N       |                                      | 1.0          | Automatic        | 1146.36       | 2889.8       | 1300.0      | 2700.0     | 10.0    | 9c27697f-ea6d-478a-a168-ed20d6cd9a20 |
  | Cutfill     | N         | N   | N   | N         | N           | Y       | dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff | 1.0          | Automatic        | 1146.36       | 2889.8       | 1300.0      | 2700.0     | 2.0     | 9c27697f-ea6d-478a-a168-ed20d6cd9a20 |
  | TempCutFill | N         | N   | N   | N         | Y           | Y       | dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff | 1.0          | Automatic        | 1146.36       | 2889.8       | 1300.0      | 2700.0     | 1.0     | 9c27697f-ea6d-478a-a168-ed20d6cd9a20 |
  | ResultCMV   | Y         | Y   | Y   | Y         | Y           | N       |                                      | 1.0          | Automatic        | 0             | 0            | 0           | 0          | 10.0    | a37f3008-65e5-44a8-b406-9a078ec62ece |
  | ResultMDP   | Y         | N   | Y   | Y         | N           | N       |                                      | 1.0          | Automatic        | 0             | 0            | 0           | 0          | 10.0    | 3ef41e3c-d1f5-40cd-b012-99d11ff432ef |

Scenario Outline: Station offset Report
  Given the report service uri "/api/v2/report/stationoffset"
  And the result file 'ReportStationOffsetResponse.json'
  And I set request parameters projectUid 'ff91dd40-1569-4765-a2bc-014321f76ace' and filterUid '<filterUid>'
  And I select columns '<Elevation>' '<CMV>' '<MDP>' '<PassCount>' '<Temperature>' '<CutFill>'
  And I select Station offset report parameters '<cutfillDesignUid>' '<alignmentUid>' '<crossSectionInterval>' '<startStation>' '<endStation>' '<offsets>' 
  When I request a report the response body should contain http code '<httpCode>'
  Then the report '<errorCode>' and result should match the '<ResultName>' from the repository
  Examples:                           
  | ResultName  | Elevation | CMV | MDP | PassCount | Temperature | CutFill | filterUid                            | cutfillDesignUid                     | alignmentUid                         | crossSectionInterval | startStation | endStation | offsets           | httpCode | errorCode |
  | NoColumns   | N         | N   | N   | N         | N           | N       | 3ef41e3c-d1f5-40cd-b012-99d11ff432ef | dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff | 3ead0c55-1e1f-4d30-aaf8-873526a2ab82 | 0.5                  | 0.0          | 1.75       | -1, 0.5           | 400      | -1        |
  | AllColumns  | Y         | Y   | Y   | Y         | Y           | Y       | 3ef41e3c-d1f5-40cd-b012-99d11ff432ef | dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff | 3ead0c55-1e1f-4d30-aaf8-873526a2ab82 | 0.5                  | 0.0          | 1.75       | -1, 0.5           | 200      |  0        |
  | Elevation   | Y         | N   | N   | N         | N           | N       | 3ef41e3c-d1f5-40cd-b012-99d11ff432ef | dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff | 6ece671b-7959-4a14-86fa-6bfe6ef4dd62 | 1.2                  | 0.0          | 4          | -1, -1.3, -2, 1.5 | 200      |  0        |
  | CMV         | N         | Y   | N   | N         | N           | Y       | 3ef41e3c-d1f5-40cd-b012-99d11ff432ef | dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff | 6ece671b-7959-4a14-86fa-6bfe6ef4dd62 | 0.5                  | 0.0          | 4          | -1, -1.3, 1       | 200      |  0        |
  | MDP         | N         | N   | Y   | N         | N           | N       | 3ef41e3c-d1f5-40cd-b012-99d11ff432ef | dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff | 6ece671b-7959-4a14-86fa-6bfe6ef4dd62 | 0.5                  | 0.0          | 4          | -1, -1.3, 1       | 200      |  0        |
  | PassCount   | N         | N   | N   | Y         | N           | N       | 3ef41e3c-d1f5-40cd-b012-99d11ff432ef | dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff | 6ece671b-7959-4a14-86fa-6bfe6ef4dd62 | 1.2                  | 0.0          | 4          | -1, -1.3, -2, 1.5 | 200      |  0        |
  | CutFill     | N         | N   | N   | N         | N           | Y       | 3ef41e3c-d1f5-40cd-b012-99d11ff432ef | dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff | 6ece671b-7959-4a14-86fa-6bfe6ef4dd62 | 0.5                  | 0.0          | 4          | -1, -1.3, 1       | 200      |  0        |
  | Temperature | N         | N   | N   | N         | Y           | N       | 3ef41e3c-d1f5-40cd-b012-99d11ff432ef | dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff | 6ece671b-7959-4a14-86fa-6bfe6ef4dd62 | 0.5                  | 0.0          | 4          | -1, -1.3, 1       | 200      |  0        |
