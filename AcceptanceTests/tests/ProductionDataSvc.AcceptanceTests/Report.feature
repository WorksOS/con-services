Feature: Report
The report feature produce xls files for the grid and station offset reports. 

@ignore
Scenario Outline: Grid Report
Given the report service uri "/api/v2/report/grid"
And I set request parameters projectUid 'ff91dd40-1569-4765-a2bc-014321f76ace' and filterUid '9c27697f-ea6d-478a-a168-ed20d6cd9a20'
And I select columns '<Elevation>' '<CMV>' '<MDP>' '<PassCount>' '<Temperature>' '<CutFill>'
And I select grid report parameters '<cutfillDesignUid>' '<gridInterval>' '<gridReportOption>' '<startNorthing>' '<startEasting>' '<endNorthing>' '<endEasting>' '<azimuth>'
And the result file 'ReportGridResponse.json'
When I request a report
Then the result should match the '<ResultName>' from the repository
Examples:
#  Y or N                                                                   Design UID                            metres          
| ResultName  | Elevation | CMV | MDP | PassCount | Temperature | CutFill | cutfillDesignUid                     | gridInterval | gridReportOption | startNorthing | startEasting | endNorthing | endEasting | azimuth |
| gridreport1 | Y         | Y   | Y   | Y         | Y           | Y       | dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff | 1.0          | Automatic        | 1146.36       | 2889.8       | 1300.0      | 2700.0     | 10.0    |
| gridreport2 | Y         | N   | N   | N         | N           | N       |                                      | 1.5          | Automatic        | 1146.36       | 2889.8       | 1300.0      | 2700.0     | 5.0     |
| gridreport3 | Y         | N   | N   | Y         | Y           | N       |                                      | 1.0          | Automatic        | 1146.36       | 2889.8       | 1300.0      | 2700.0     | 10.0    |
| gridreport4 | N         | N   | N   | N         | N           | Y       | dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff | 1.0          | Automatic        | 1146.36       | 2889.8       | 1300.0      | 2700.0     | 2.0     |
| gridreport5 | N         | N   | N   | N         | Y           | Y       | dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff | 1.0          | Automatic        | 1146.36       | 2889.8       | 1300.0      | 2700.0     | 1.0     |


@ignore
Scenario Outline: Station offset Report
Given the report service uri "/api/v2/report/stationoffset"
And I set request parameters projectUid 'ff91dd40-1569-4765-a2bc-014321f76ace' and filterUid '9c27697f-ea6d-478a-a168-ed20d6cd9a20'
And I select columns '<Elevation>' '<CMV>' '<MDP>' '<PassCount>' '<Temperature>' '<CutFill>'
And I select Station offset report parameters '<cutfillDesignUid>' '<alignmentDesignUid>' '<crossSectionInterval>' '<startStation>' '<endStation>' '<offsets>' 
And the result file 'ReportstationoffsetResponse.json'
When I request a report
Then the result should match the '<ResultName>' from the repository
Examples:                           
| ResultName     | Elevation | CMV | MDP | PassCount | Temperature | CutFill | cutfillDesignUid                     | alignmentDesignUid                   | crossSectionInterval | startStation | endStation | offsets  |
| stationreport1 | Y         | Y   | Y   | Y         | Y           | Y       | dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff | dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff | 1.4                  | 1146.36      | 2889.8     | 1.0, 3.5 | 
