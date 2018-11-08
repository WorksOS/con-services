Feature: CompactionTagFile
  I should be able to POST tag files for compaction.

Scenario Outline: TagFile - Bad Tag File
  Given the service route "/api/v2/tagfiles" and request repo "CompactionTagFileRequest.json"
  When I POST with parameter "<Code>" I expect response code <HttpCode>
  Then the response should contain the message "<ErrorMessage>"
  Examples: 
  | Code | ErrorMessage                                                                                             | HttpCode |
  | 2005 | Failed to process tagfile with error: The TAG file was found to be corrupted on its pre-processing scan. | 400      |
  | 2008 | Failed to process tagfile with error: OnChooseMachine. Machine Subscriptions Invalid.                    | 400      |

Scenario Outline: TagFile - Archived Project
  Given the service route "/api/v2/tagfiles" and request repo "CompactionTagFileRequest.json"
  When I POST with parameter "<Code>" I expect response code <HttpCode>
  Then the response should contain the message "<ErrorMessage>"
  Examples: 
  | Code | ErrorMessage                                                    | HttpCode |
  | -1   | The project has been archived and this function is not allowed. | 400      |

Scenario Outline: TagFile - Bad Request
  Given the service route "/api/v2/tagfiles" and request repo "CompactionTagFileRequest.json"
  When I POST with parameter "<Code>" I expect response code <HttpCode>
  Then the response should contain code "<ErrorCode>"
  Examples: 
  | Code            | ErrorCode | HttpCode |
  | NullFileName    | -1        | 400      |
  | NullData        | -1        | 400      |
  | FilenameTooLong | -1        | 400      |
  | NullProjectUid  | 2008      | 400      |

Scenario Outline: TagFile Direct Submission - Bad Tag File
  Given the service route "/api/v2/tagfiles/direct" and request repo "CompactionTagFileDirectSubmissionRequest.json"
  When I POST with parameter "<Code>" I expect response code <HttpCode>
  Then the response should contain the message "<ErrorMessage>"
  Examples: 
  | Code | ErrorMessage                                                       | HttpCode |
  | 5    | The TAG file was found to be corrupted on its pre-processing scan. | 400      |
  | 8    | OnChooseMachine. Machine Subscriptions Invalid.                    | 400      |

Scenario Outline: TagFile Direct Submission - Bad Request
  Given the service route "/api/v2/tagfiles/direct" and request repo "CompactionTagFileDirectSubmissionRequest.json"
  When I POST with parameter "<Code>" I expect response code <HttpCode>
  Then the response should contain code "<ErrorCode>"
  Examples: 
  | Code            | ErrorCode | HttpCode |
  | NullFileName    | -1        | 400      |
  | NullData        | -1        | 400      |
  | FilenameTooLong | -1        | 400      |
  | NullProjectUid  | 8        | 400      |
  | NullMachineId   | 8         | 400      |
  | NullBoundary    | 8         | 400      |
