Feature: ElevationStatistics
	I should be able to request Elevation Statistics for a project.

Background: 
	Given the ElevationStatistics service URI "/api/v1/statistics/elevation", request repo "ElevationStatsRequest.json" and result repo "ElevationStatsResponse.json"

Scenario Outline: ElevationStatistics - Good Request
	When I request Elevation Statistics supplying "<ParameterName>" paramters from the repository
	Then the Elevation Statistics response should match "<ResultName>" result from the repository
	Examples: 
	| ParameterName       | ResultName          |
	| NoFilter            | NoFilter            |
	| FiveCells           | FiveCells           |
	| FiveCellsOneMachine | FiveCellsOneMachine |