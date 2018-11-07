Feature: CompactionPatch
  I should be able to request Production Data Patch

Scenario Outline: Patch - Good Request
  Given the service route "/api/v2/patches" and result repo "CompactionPatchResponse.json"
  And with parameter "projectUid" with value "<ProjectUID>"
  And with parameter "filterUid" with value "<FilterUID>"
  And with parameter "patchId" with value "<PatchId>"
  And with parameter "mode" with value "<Mode>"
  And with parameter "patchSize" with value "<PatchSize>"
  And with parameter "includeTimeOffsets" with value "<IncludeTimeOffsets>"
  When I send the GET request I expect response code <HttpCode>
  Then the response should match "<ResultName>" from the repository
  Examples: 
  | ProjectUID                           | FilterUID                            | PatchId | Mode | PatchSize | IncludeTimeOffsets | ResultName           | HttpCode |
  | ff91dd40-1569-4765-a2bc-014321f76ace |                                      | 0       | 0    | 1         | false              | HeightNoFilterNoTime | 200      |
  | ff91dd40-1569-4765-a2bc-014321f76ace |                                      | 0       | 0    | 1         | true               | HeightNoFilter       | 200      |
  | ff91dd40-1569-4765-a2bc-014321f76ace | 3ef41e3c-d1f5-40cd-b012-99d11ff432ef | 0       | 0    | 1         | true               | HeightAreaFilter     | 200      |
  | ff91dd40-1569-4765-a2bc-014321f76ace | 3ef41e3c-d1f5-40cd-b012-99d11ff432ef | 1       | 0    | 1         | true               | Patch1WithFilter     | 200      |

Scenario Outline: Patch - Good Request Protobuf
  Given the service route "/api/v2/patches" and result repo "CompactionPatchResponse.json"
  And with parameter "projectUid" with value "<ProjectUID>"
  And with parameter "filterUid" with value "<FilterUID>"
  And with parameter "patchId" with value "<PatchId>"
  And with parameter "mode" with value "<Mode>"
  And with parameter "patchSize" with value "<PatchSize>"
  And with parameter "includeTimeOffsets" with value "<IncludeTimeOffsets>"
  When I send a GET request with Accept header "application/x-protobuf" I expect response code <HttpCode>
  Then the deserialized result should match the "<ResultName>" result from the repository
  Examples: 
  | ProjectUID                           | FilterUID                            | PatchId | Mode | PatchSize | IncludeTimeOffsets | ResultName                   | HttpCode | AcceptHeader           |
  | ff91dd40-1569-4765-a2bc-014321f76ace |                                      | 0       | 0    | 1         | false              | HeightNoFilterProtobufNoTime | 200      | application/x-protobuf |
  | ff91dd40-1569-4765-a2bc-014321f76ace |                                      | 0       | 0    | 1         | true               | HeightNoFilterProtobuf       | 200      | application/x-protobuf |
  | ff91dd40-1569-4765-a2bc-014321f76ace | 3ef41e3c-d1f5-40cd-b012-99d11ff432ef | 0       | 0    | 1         | true               | HeightAreaFilterProtobuf     | 200      | application/x-protobuf |
  | ff91dd40-1569-4765-a2bc-014321f76ace | 3ef41e3c-d1f5-40cd-b012-99d11ff432ef | 1       | 0    | 1         | true               | Patch1WithFilterProtobuf     | 200      | application/x-protobuf |

Scenario Outline: Patch - Bad Request
  Given the service route "/api/v2/patches" and result repo "CompactionPatchResponse.json"
  And with parameter "projectUid" with value "<ProjectUID>"
  And with parameter "patchId" with value "<PatchId>"
  And with parameter "mode" with value "<Mode>"
  And with parameter "patchSize" with value "<PatchSize>"
  And with parameter "includeTimeOffsets" with value "<IncludeTimeOffsets>"
  When I send the GET request I expect response code <HttpCode>
  Then the response should match "<ResultName>" from the repository
  Examples: 
  | ProjectUID                           | PatchId | Mode | PatchSize | IncludeTimeOffsets | ResultName        | HttpCode |
  |                                      | 0       | 0    | 1         | true               | InvalidProjectUid | 400      |
  | 00000000-0000-0000-0000-000000000000 | 0       | 0    | 1         | true               | NullProjectUid    | 401      |
  | ff91dd40-1569-4765-a2bc-014321f76ace | 0       | 0    | -1        | true               | InvalidPatchSize  | 400      |
  | ff91dd40-1569-4765-a2bc-014321f76ace | 0       | 0    | 1001      | true               | InvalidPatchSize  | 400      |
  | ff91dd40-1569-4765-a2bc-014321f76ace | -1      | 0    | 1         | true               | InvalidPatchId    | 400      |
  | ff91dd40-1569-4765-a2bc-014321f76ace | 1001    | 0    | 1         | true               | InvalidPatchId    | 400      |
