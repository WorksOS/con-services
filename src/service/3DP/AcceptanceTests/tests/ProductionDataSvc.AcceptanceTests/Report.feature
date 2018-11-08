Feature: Report
  The report feature produce json object that is used by the reporting service to build an xls file. 

Scenario Outline: Grid Report
  Given the service route "/api/v2/report/grid" and result repo "ReportGridResponse.json"
  And with parameter "projectUid" with value "ff91dd40-1569-4765-a2bc-014321f76ace"
  And with parameter "filterUid" with value "<filterUid>"
  And with parameter "reportElevation" with value "<Elevation>"
  And with parameter "reportCMV" with value "<CMV>"
  And with parameter "reportMdp" with value "<MDP>"
  And with parameter "reportPassCount" with value "<PassCount>"
  And with parameter "reportTemperature" with value "<Temperature>"
  And with parameter "reportCutFill" with value "<CutFill>"
  And with parameter "cutfillDesignUid" with value "<cutfillDesignUid>"
  And with parameter "gridInterval" with value "<gridInterval>"
  And with parameter "gridReportOption" with value "<gridReportOption>"
  And with parameter "startNorthing" with value "<startNorthing>"
  And with parameter "startEasting" with value "<startEasting>"
  And with parameter "endNorthing" with value "<endNorthing>"
  And with parameter "endEasting" with value "<endEasting>"
  And with parameter "azimuth" with value "<azimuth>"
  When I send the GET request I expect response code <HttpCode>
  Then the complex response object should match "<ResultName>" from the repository
  Examples:
  | ResultName  | Elevation | CMV   | MDP   | PassCount | Temperature | CutFill | cutfillDesignUid                     | gridInterval | gridReportOption | startNorthing | startEasting | endNorthing | endEasting | azimuth | filterUid                            | HttpCode |
  | AllColumns  | true      | true  | true  | true      | true        | true    | dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff | 1.0          | Automatic        | 1146.36       | 2889.8       | 1300.0      | 2700.0     | 10.0    | 9c27697f-ea6d-478a-a168-ed20d6cd9a20 | 200      |
  | Elevation   | true      | false | false | false     | false       | false   |                                      | 1.5          | Automatic        | 1146.36       | 2889.8       | 1300.0      | 2700.0     | 5.0     | 9c27697f-ea6d-478a-a168-ed20d6cd9a20 | 200      |
  | ElevPcTemp  | true      | false | false | true      | true        | false   |                                      | 1.0          | Automatic        | 1146.36       | 2889.8       | 1300.0      | 2700.0     | 10.0    | 9c27697f-ea6d-478a-a168-ed20d6cd9a20 | 200      |
  | Cutfill     | false     | false | false | false     | false       | true    | dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff | 1.0          | Automatic        | 1146.36       | 2889.8       | 1300.0      | 2700.0     | 2.0     | 9c27697f-ea6d-478a-a168-ed20d6cd9a20 | 200      |
  | TempCutFill | false     | false | false | false     | true        | true    | dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff | 1.0          | Automatic        | 1146.36       | 2889.8       | 1300.0      | 2700.0     | 1.0     | 9c27697f-ea6d-478a-a168-ed20d6cd9a20 | 200      |
  | ResultCMV   | true      | true  | true  | true      | true        | false   |                                      | 1.0          | Automatic        | 0             | 0            | 0           | 0          | 10.0    | a37f3008-65e5-44a8-b406-9a078ec62ece | 200      |
  | ResultMDP   | true      | false | true  | true      | false       | false   |                                      | 1.0          | Automatic        | 0             | 0            | 0           | 0          | 10.0    | 3ef41e3c-d1f5-40cd-b012-99d11ff432ef | 200      |

Scenario Outline: Station offset Report - No Data
  Given the service route "/api/v2/report/stationoffset" and result repo "ReportStationOffsetResponse.json"
  And with parameter "projectUid" with value "ff91dd40-1569-4765-a2bc-014321f76ace"
  And with parameter "filterUid" with value "<filterUid>"
  And with parameter "reportElevation" with value "<Elevation>"
  And with parameter "reportCMV" with value "<CMV>"
  And with parameter "reportMdp" with value "<MDP>"
  And with parameter "reportPassCount" with value "<PassCount>"
  And with parameter "reportTemperature" with value "<Temperature>"
  And with parameter "reportCutFill" with value "<CutFill>"
  And with parameter "cutfillDesignUid" with value "<cutfillDesignUid>"
  And with parameter "alignmentUid" with value "<alignmentUid>"
  And with parameter "crossSectionInterval" with value "<crossSectionInterval>"
  And with parameter "startStation" with value "<startStation>"
  And with parameter "endStation" with value "<endStation>"
  And with array parameter "offsets" with values "<offsets>"
  When I send the GET request I expect response code <HttpCode>
  Examples:
  | ResultName  | Elevation | CMV   | MDP   | PassCount | Temperature | CutFill | filterUid                            | cutfillDesignUid                     | alignmentUid                         | crossSectionInterval | startStation | endStation | offsets           | HttpCode |
  | NoColumns   | false     | false | false | false     | false       | false   | 3ef41e3c-d1f5-40cd-b012-99d11ff432ef | dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff | 3ead0c55-1e1f-4d30-aaf8-873526a2ab82 | 0.5                  |           | 1.75       | -1, 0.5           | 400      |

Scenario Outline: Station offset Report
  Given the service route "/api/v2/report/stationoffset" and result repo "ReportStationOffsetResponse.json"
  And with parameter "projectUid" with value "ff91dd40-1569-4765-a2bc-014321f76ace"
  And with parameter "filterUid" with value "<filterUid>"
  And with parameter "reportElevation" with value "<Elevation>"
  And with parameter "reportCMV" with value "<CMV>"
  And with parameter "reportMdp" with value "<MDP>"
  And with parameter "reportPassCount" with value "<PassCount>"
  And with parameter "reportTemperature" with value "<Temperature>"
  And with parameter "reportCutFill" with value "<CutFill>"
  And with parameter "cutfillDesignUid" with value "<cutfillDesignUid>"
  And with parameter "alignmentUid" with value "<alignmentUid>"
  And with parameter "crossSectionInterval" with value "<crossSectionInterval>"
  And with parameter "startStation" with value "<startStation>"
  And with parameter "endStation" with value "<endStation>"
  And with array parameter "offsets" with values "<offsets>"
  When I send the GET request I expect response code <HttpCode>
  Then the complex response object should match "<ResultName>" from the repository
  Examples:
  | ResultName  | Elevation | CMV   | MDP   | PassCount | Temperature | CutFill | filterUid                            | cutfillDesignUid                     | alignmentUid                         | crossSectionInterval | startStation | endStation | offsets           | HttpCode |
  | AllColumns  | true      | true  | true  | true      | true        | true    | 3ef41e3c-d1f5-40cd-b012-99d11ff432ef | dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff | 3ead0c55-1e1f-4d30-aaf8-873526a2ab82 | 0.5                  |           | 1.75       | -1, 0.5           | 200      |
  | Elevation   | true      | false | false | false     | false       | false   | 3ef41e3c-d1f5-40cd-b012-99d11ff432ef | dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff | 6ece671b-7959-4a14-86fa-6bfe6ef4dd62 | 1.2                  |           | 4          | -1, -1.3, -2, 1.5 | 200      |
  | CMV         | false     | true  | false | false     | false       | true    | 3ef41e3c-d1f5-40cd-b012-99d11ff432ef | dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff | 6ece671b-7959-4a14-86fa-6bfe6ef4dd62 | 0.5                  |           | 4          | -1, -1.3, 1       | 200      |
  | MDP         | false     | false | true  | false     | false       | false   | 3ef41e3c-d1f5-40cd-b012-99d11ff432ef | dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff | 6ece671b-7959-4a14-86fa-6bfe6ef4dd62 | 0.5                  |           | 4          | -1, -1.3, 1       | 200      |
  | PassCount   | false     | false | false | true      | false       | false   | 3ef41e3c-d1f5-40cd-b012-99d11ff432ef | dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff | 6ece671b-7959-4a14-86fa-6bfe6ef4dd62 | 1.2                  |           | 4          | -1, -1.3, -2, 1.5 | 200      |
  | CutFill     | false     | false | false | false     | false       | true    | 3ef41e3c-d1f5-40cd-b012-99d11ff432ef | dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff | 6ece671b-7959-4a14-86fa-6bfe6ef4dd62 | 0.5                  |           | 4          | -1, -1.3, 1       | 200      |
  | Temperature | false     | false | false | false     | true        | false   | 3ef41e3c-d1f5-40cd-b012-99d11ff432ef | dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff | 6ece671b-7959-4a14-86fa-6bfe6ef4dd62 | 0.5                  |           | 4          | -1, -1.3, 1       | 200      |
