Feature: ExportReport
	I should be able to request export report.

Background: 
	Given the Export Report service URI "/api/v1/export", request repo "ExportReportRequest.json" and result repo "ExportReportResponse.json"

Scenario Outline: ExportReport - Good Request
	When I request Export Report supplying "<RequestName>" from the request repository
	Then the result should match "<ResultName>" from the result repository
	Examples:
	| RequestName                                      | ResultName                                       |
	| PassCountAllPassesLastElevation                  | PassCountAllPassesLastElevation                  |
	| PassCountAllPassesFirstElevation                 | PassCountAllPassesFirstElevation                 |
	| PassCountAllPassesHighestElevation               | PassCountAllPassesHighestElevation               |
	| PassCountAllPassesLowestElevation                | PassCountAllPassesLowestElevation                |
	| PassCountAllPassesFirstElevationInclSuperseded   | PassCountAllPassesFirstElevationInclSuperseded   |
	| PassCountAllPassesHighestElevationInclSuperseded | PassCountAllPassesHighestElevationInclSuperseded |
	| VedaFinalPassAcomScom                            | VedaFinalPassAcomScom                            |
	| VedaAllPassesAutoMapReset                        | VedaAllPassesAutoMapReset                        |
	| VedaFinalPassAcomAutoMapReset                    | VedaFinalPassAcomAutoMapReset                    |
	| VedaFinalPassScomAutoMapReset                    | VedaFinalPassScomAutoMapReset                    |
	| VedaAllPassesScomAutoMapReset                    | VedaAllPassesScomAutoMapReset                    |

Scenario Outline: ExportReport - No Content
	When I request Export Report supplying "<RequestName>" from the request repository expecting NoContent
	Examples:
	| RequestName | ErrorCode | ErrorMessage                        |
	| NoDateRange |           |                                     |
