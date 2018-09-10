Feature: CompactionMachineLiftIds
  I should be able to request all machine lift Ids of a project

Scenario Outline: CompactionMachineLiftIds
  Given the Compaction service URI "/api/v2/projects/{projectUid}/machinelifts" for operation "All"
  And the result file "CompactionMachineLiftIdsResponse.json"
  And projectUid "<ProjectUid>"
  When I send the request with expected HTTP status code "<HttpCode>"
  Then the result should match the "<ResultName>" result from the repository
  Examples:
  | RequestName           | ProjectUid                           | HttpCode | ResultName            |
  | MachineLiftsOneAsset  | 04c94921-6343-4ffb-9d35-db9d281743fc | 200      | MachineLiftsOneAsset  |
  | MachineLiftsTwoAssets | ff91dd40-1569-4765-a2bc-014321f76ace | 200      | MachineLiftsTwoAssets |
  | InvalidProjectUid     | 02c94921-6343-4ffb-9d35-db9d281743fc | 401      | InvalidProjectUid     |
