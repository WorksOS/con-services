Feature: ExportGriddedCSV
  I should be able to request Gridded CSV Exports for a project.

Scenario Outline: ExportGriddedCSV - Good Request
  Given the service route "/api/v1/export/gridded/csv" request repo "ExportGriddedCSVRequest.json" and result repo "ExportGriddedCSVResponse.json"
  When I POST with parameter "<ParameterName>" I expect response code <HttpCode>
  Then the response should match "<ResultName>" from the repository
  Examples:
  | ParameterName                                        | ResultName                                           | HttpCode |
  | FullProjectLatestDateElevationOnlyGriddedCSVExport   | FullProjectLatestDateElevationOnlyGriddedCSVExport   | 200      |
  | FullProjectSpecificDateElevationOnlyGriddedCSVExport | FullProjectSpecificDateElevationOnlyGriddedCSVExport | 200      |

Scenario Outline: ExportGriddedCSV - Bad Request
  Given the service route "/api/v1/export/gridded/csv" request repo "ExportGriddedCSVRequest.json" and result repo "ExportGriddedCSVResponse.json"
  When I POST with parameter "<ParameterName>" I expect response code <HttpCode>
  Then the response should contain message "<ErrorMessage>" and code "<ErrorCode>"
  Examples:
  | ParameterName                      | ErrorCode | ErrorMessage                                                                                | HttpCode |
  | BadRequestNoReportType             | -1        | Grid report type must be either 1 ('Gridded') or 2 ('Alignment'). Actual value supplied: 0  | 400      |
  | BadRequestUnknownReportType        | -1        | Grid report type must be either 1 ('Gridded') or 2 ('Alignment'). Actual value supplied: 10 | 400      |
  | BadRequestIntervalTooSmall         | -1        | Interval must be >= 0.1m and <= 100.0m. Actual value: 0.09                                  | 400      |
  | BadRequestIntervalTooLarge         | -1        | Interval must be >= 0.1m and <= 100.0m. Actual value: 101                                   | 400      |
  | BadRequestNoOutputFieldsConfigured | -1        | There are no selected fields to be reported on                                              | 400      |
