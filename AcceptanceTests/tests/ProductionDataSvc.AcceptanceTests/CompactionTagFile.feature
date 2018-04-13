Feature: CompactionTagFile
	I should be able to POST tag files for compaction.

Background: 
  Given the Tag file service URI "/api/v2/tagfiles" and request repo "CompactionTagFileRequest.json"

Scenario Outline: TagFile - Bad Tag File
  When I POST a tag file with Code <Code> from the repository expecting bad request return
  Then the Tag Process Service response should contain Code <Code> and Message <Message>
  Examples: 
  | Code | Message                                                                                                    |
  | 2005 | "Failed to process tagfile with error: The TAG file was found to be corrupted on its pre-processing scan." |
  | 2008 | "Failed to process tagfile with error: OnChooseMachine. Machine Subscriptions Invalid."                    |

Scenario Outline: TagFile - Bad Request
  When I POST a Tag file with name "<paramName>" from the repository expecting bad request return
  Then the Tag Process Service response should contain Error Code <code>
  Examples: 
  | paramName       | code |
  | NullFileName    | -1   |
  | NullData        | -1   |
  | FilenameTooLong | -1   |
  | NullProjectUid  | 2008 |
