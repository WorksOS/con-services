Feature: ExportGriddedCSV
	I should be able to request Gridded CSV Exports for a project.

Background: 
	Given the Export Gridded CSV service URI "/api/v1/export/gridded/csv", request repo "ExportGriddedCSVRequest.json" and result repo "ExportGriddedCSVResponse.json"

Scenario Outline: ExportGriddedCSV - Good Request
	When I request Export Gridded CSV supplying "<RequestName>" from the request repository
	Then the result should match "<ResultName>" from the result repository
	Examples:
	| RequestName                                          | ResultName                                           |
	| FullProjectLatestDateElevationOnlyGriddedCSVExport   | FullProjectLatestDateElevationOnlyGriddedCSVExport   |
	| FullProjectSpecificDateElevationOnlyGriddedCSVExport | FullProjectSpecificDateElevationOnlyGriddedCSVExport |

Scenario Outline: ExportGriddedCSV - Bad Request
	When I request Export Gridded CSV supplying "<RequestName>" from the request repository expecting BadRequest
	Then the result should contain error code <ErrorCode> and error message "<ErrorMessage>"
	Examples:
	| RequestName                        | ErrorCode | ErrorMessage                                                                                |
	| BadRequestNoReportType             | -1        | Grid report type must be either 1 ('Gridded') or 2 ('Alignment'). Actual value supplied: 0  |
	| BadRequestUnknownReportType        | -1        | Grid report type must be either 1 ('Gridded') or 2 ('Alignment'). Actual value supplied: 10 |
	| BadRequestIntervalTooSmall         | -1        | Interval must be >= 0.1m and <= 100.0m. Actual value: 0.09                                  |
	| BadRequestIntervalTooLarge         | -1        | Interval must be >= 0.1m and <= 100.0m. Actual value: 101                                   |
	| BadRequestNoOutputFieldsConfigured | -1        | There are no selected fields to be reported on                                              |

