Feature: CompactionPatch
    I should be able to request Production Data Patch
    
Scenario Outline: Patch - Good Request
    Given the Compaction service URI "/api/v2/patches" for operation "All"
    And the result file "CompactionPatchResponse.json"
    And projectUid "<ProjectUID>"
    And filterUid "<FilterUID>"
    And patchId "<PatchId>"
    And mode "<Mode>"
    And patchSize "<PatchSize>"
    And cellDownSample "<CellDownSample>"
    When I request result with expected status result "<HttpCode>"
    Then the result should match the "<ResultName>" result from the repository
    Examples: 
    | RequestName       | ProjectUID                           | FilterUID                            | PatchId | Mode | PatchSize | CellDownSample | ResultName       | HttpCode |
    | PatchesNoFilter   | ff91dd40-1569-4765-a2bc-014321f76ace |                                      | 0       | 0    | 1         | 1              | HeightNoFilter   | 200      |
    | PatchesWithFilter | ff91dd40-1569-4765-a2bc-014321f76ace | 3ef41e3c-d1f5-40cd-b012-99d11ff432ef | 0       | 0    | 1         | 1              | HeightAreaFilter | 200      |
    | Patch1WithFilter  | ff91dd40-1569-4765-a2bc-014321f76ace | 3ef41e3c-d1f5-40cd-b012-99d11ff432ef | 1       | 0    | 1         | 1              | Patch1WithFilter | 200      |

Scenario Outline: Patch - Good Request Protobuf
    Given the Compaction service URI "/api/v2/patches" for operation "All"
    And the result file "CompactionPatchResponse.json"
    And projectUid "<ProjectUID>"
    And filterUid "<FilterUID>"
    And patchId "<PatchId>"
    And mode "<Mode>"
    And patchSize "<PatchSize>"
    And cellDownSample "<CellDownSample>"
    When I request result with Accept header "<AcceptHeader>" and expected status result "<HttpCode>"
    Then the deserialized result should match the "<ResultName>" result from the repository
    Examples: 
    | RequestName       | ProjectUID                           | FilterUID                            | PatchId | Mode | PatchSize | CellDownSample | ResultName               | HttpCode | AcceptHeader           |
    | PatchesNoFilter   | ff91dd40-1569-4765-a2bc-014321f76ace |                                      | 0       | 0    | 1         | 1              | HeightNoFilterProtobuf   | 200      | application/x-protobuf |
    | PatchesWithFilter | ff91dd40-1569-4765-a2bc-014321f76ace | 3ef41e3c-d1f5-40cd-b012-99d11ff432ef | 0       | 0    | 1         | 1              | HeightAreaFilterProtobuf | 200      | application/x-protobuf |
    | Patch1WithFilter  | ff91dd40-1569-4765-a2bc-014321f76ace | 3ef41e3c-d1f5-40cd-b012-99d11ff432ef | 1       | 0    | 1         | 1              | Patch1WithFilterProtobuf | 200      | application/x-protobuf |

Scenario Outline: Patch - Bad Request
    Given the Compaction service URI "/api/v2/patches" for operation "All"
    And the result file "CompactionPatchResponse.json"
    And projectUid "<ProjectUID>"
    And filterUid "<FilterUID>"
    And patchId "<PatchId>"
    And mode "<Mode>"
    And patchSize "<PatchSize>"
    And cellDownSample "<CellDownSample>"
    When I request result with expected status result "<HttpCode>"
    Then the result should match the "<ResultName>" result from the repository
    Examples: 
    | RequestName           | ProjectUID                           | FilterUID | PatchId | Mode | PatchSize | CellDownSample | ResultName            | HttpCode |
    | NullProjectUid        |                                      |           | 0       | 0    | 1         | 1              | InvalidProjectUid     | 401      |
    | InvalidProjectUid     | 00000000-0000-0000-0000-000000000000 |           | 0       | 0    | 1         | 1              | InvalidProjectUid     | 401      |
    | NegativePatchSize     | ff91dd40-1569-4765-a2bc-014321f76ace |           | 0       | 0    | -1        | 1              | InvalidPatchSize      | 400      |
    | TooLargePatchSize     | ff91dd40-1569-4765-a2bc-014321f76ace |           | 0       | 0    | 1001      | 1              | InvalidPatchSize      | 400      |
    | NegativePatchId       | ff91dd40-1569-4765-a2bc-014321f76ace |           | -1      | 0    | 1         | 1              | InvalidPatchId        | 400      |
    | TooLargePatchId       | ff91dd40-1569-4765-a2bc-014321f76ace |           | 1001    | 0    | 1         | 1              | InvalidPatchId        | 400      |
