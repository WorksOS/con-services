Feature: CompactionCutFill
I should be able to request Cut-Fill compaction data

Scenario Outline: Compaction Get Cut-Fill Details - No Design Filter
  Given the service route "/api/v2/cutfill/details" and result repo "CompactionGetCutFillDataResponse.json"
  And with parameter "projectUid" with value "<ProjectUID>"
  And with parameter "cutfillDesignUid" with value "<CutFillDesignUID>"
  When I send the GET request I expect response code <HttpCode>
  Then the response should match "<ResultName>" from the repository
  Examples: 
  | RequestName     | ProjectUID                           | CutFillDesignUID                     | ResultName                | HttpCode |
  |                 | ff91dd40-1569-4765-a2bc-014321f76ace | dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff | NoDesignFilter_Details    | 200      |
  | ProjectSettings | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff | NoDesignFilter_Details_PS | 200      |

Scenario Outline: Compaction Get Cut-Fill Details
  Given the service route "/api/v2/cutfill/details" and result repo "CompactionGetCutFillDataResponse.json"
  And with parameter "projectUid" with value "<ProjectUID>"
  And with parameter "filterUid" with value "<FilterUID>"
  And with parameter "cutfillDesignUid" with value "<CutFillDesignUID>"
  When I send the GET request I expect response code <HttpCode>
  Then the response should match "<ResultName>" from the repository
  Examples: 
  | RequestName       | ProjectUID                           | CutFillDesignUID                     | FilterUID                            | ResultName                     | HttpCode |
  | DesignOutside     | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff | 1cf81668-1739-42d5-b068-ea025588796a | DesignOutside_Details          | 200      |
  | DesignIntersects  | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | 220e12e5-ce92-4645-8f01-1942a2d5a57f | 3d9086f2-3c04-4d92-9141-5134932b1523 | DesignIntersects_Details       | 200      |
  | FilterAreaMachine | ff91dd40-1569-4765-a2bc-014321f76ace | dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff | 9c27697f-ea6d-478a-a168-ed20d6cd9a20 | BoundaryMachineFilterCFDetails | 200      |
  | AlignmentFilter   | ff91dd40-1569-4765-a2bc-014321f76ace | dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff | 2811c7c3-d270-4d63-97e2-fc3340bf6c7a | AlignmentFilter_Details        | 200      |
  | TemperatureFilter | ff91dd40-1569-4765-a2bc-014321f76ace | dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff | 1980fc8b-c892-4f9f-b673-bc09827bf2b5 | TemperatureFilter_Details      | 200      |
  | PassCountFilter   | ff91dd40-1569-4765-a2bc-014321f76ace | dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff | c5590172-a1bb-440a-bc7d-6c35ecc75724 | PassCountFilter_Details        | 200      |
