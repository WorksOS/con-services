Feature: CompactionMachineLiftIds
  I should be able to request all lift Ids of a project

Scenario Outline: CompactionMachineLiftIds
  Given the service route "/api/v2/projects/<ProjectUID>/liftids" and result repo "CompactionMachineLiftIdsResponse.json"
  When I send the GET request I expect response code <HttpCode>
  Then the response should match "<ResultName>" from the repository
  Examples:
  | RequestName             | ProjectUID                           | HttpCode | ResultName              |
  | MachineLiftIdsOneAsset  | 04c94921-6343-4ffb-9d35-db9d281743fc | 200      | MachineLiftIdsOneAsset  |
  | MachineLiftIdsTwoAssets | ff91dd40-1569-4765-a2bc-014321f76ace | 200      | MachineLiftIdsTwoAssets |
  | InvalidProjectUid       | 02c94921-6343-4ffb-9d35-db9d281743fc | 401      | InvalidProjectUid       |
