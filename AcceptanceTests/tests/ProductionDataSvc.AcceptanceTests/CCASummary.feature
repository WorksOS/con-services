Feature: CCASummary
	I should be able to request CCA summary.

Background: 
	Given the CCA Summary service URI '/api/v1/compaction/cca/summary', request repo 'CCASummaryRequest.json' and result repo 'CCASummaryResponse.json'

Scenario Outline: CCASummary
	When I request CCA Summary supplying '<ParameterName>' paramters from the repository
	Then the CCA Summary response should match '<ResultName>' result from the repository
	Examples: 
	| ParameterName        | ResultName           |
	| NoFilterAtAll        | NoFilterAtAll        |
	| Over33Under33On33    | Over33Under33On33    |
	| FilterByLayerAndTime | FilterByLayerAndTime |