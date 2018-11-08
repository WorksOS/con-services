Feature: ExportReport
  I should be able to request export report.

Scenario Outline: ExportReport - Good Request
  Given the service route "/api/v1/export" request repo "ExportReportRequest.json" and result repo "ExportReportResponse.json"
  When I POST with parameter "<ParameterName>" I expect response code <HttpCode>
  Then the response should match "<ResultName>" from the repository
  Examples:
  | ParameterName                                    | ResultName                                       | HttpCode |
  | PassCountAllPassesLastElevation                  | PassCountAllPassesLastElevation                  | 200      |
  | PassCountAllPassesFirstElevation                 | PassCountAllPassesFirstElevation                 | 200      |
  | PassCountAllPassesHighestElevation               | PassCountAllPassesHighestElevation               | 200      |
  | PassCountAllPassesLowestElevation                | PassCountAllPassesLowestElevation                | 200      |
  | PassCountAllPassesFirstElevationInclSuperseded   | PassCountAllPassesFirstElevationInclSuperseded   | 200      |
  | PassCountAllPassesHighestElevationInclSuperseded | PassCountAllPassesHighestElevationInclSuperseded | 200      |
  | VedaFinalPassAcomScom                            | VedaFinalPassAcomScom                            | 200      |
  | VedaAllPassesAutoMapReset                        | VedaAllPassesAutoMapReset                        | 200      |
  | VedaFinalPassAcomAutoMapReset                    | VedaFinalPassAcomAutoMapReset                    | 200      |
  | VedaFinalPassScomAutoMapReset                    | VedaFinalPassScomAutoMapReset                    | 200      |
  | VedaAllPassesScomAutoMapReset                    | VedaAllPassesScomAutoMapReset                    | 200      |

Scenario Outline: ExportReport - Bad Request
  Given the service route "/api/v1/export" request repo "ExportReportRequest.json" and result repo "ExportReportResponse.json"
  When I POST with parameter "<ParameterName>" I expect response code <HttpCode>
  Then the response should contain message "<ErrorMessage>" and code "<ErrorCode>"
  Examples:
  | ParameterName | ErrorCode | ErrorMessage                                                                                          | HttpCode |
  | NoDateRange   | -4        | Failed to get/update data requested by ExportReportExecutor with error: Invalid date range for export | 400      |
