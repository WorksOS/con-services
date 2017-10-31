Feature: SummaryVolumes
	I should be able to request Summary Volumes.

Background: 
	Given the Summary Volumes service URI "/api/v1/volumes/summary"

@requireSurveyedSurface
Scenario Outline: SummaryVolumes - Good Request
  And the result file "CompactionSummaryVolumeResponse.json"
	When I request Summary Volumes supplying "<ParameterName>" paramters from the repository
	Then the response should match "<ResultName>" result from the repository
	Examples: 
	| RequestName       | ProjectUid                           | VolumeCalcType | BaseFilterUid                        | TopFilterUid                         | BaseDesignUid                                           | TopDesignUid                           |
	| SuccessNoDesigns  | ff91dd40-1569-4765-a2bc-014321f76ace | 4              | F07ED071-F8A1-42C3-804A-1BDE7A78BE5B | A40814AA-9CDB-4981-9A21-96EA30FFECDD |                                                         |                                        |