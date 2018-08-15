Feature: EditData
  I should be able to do and undo machine design and layer edits.

Background: 
  Given the edit data service URI "/api/v1/productiondata/edit" 
    And the get edit data service URI "/api/v1/productiondata/getedits"
    And all data edits are cleared for project 1001285
    #And GetLifts service "/api/v1/projects/{0}/liftids" only returns 13 real lifts for project 1001285
    #And GetMachineDesign service "/api/v1/projects/{0}/machinedesigns" only returns 3 real designs for project 1285
    And the following data edit details
    | EditID | assetId | startUTC                | endUTC                  | onMachineDesignName | liftNumber |
    | 0      | 1       | 2014-11-25T00:00:00.000 | 2014-11-25T00:00:01.000 | VirtualDesign       | null       |
    | 1      | 1       | 2014-11-25T00:00:02.000 | 2014-11-25T00:00:03.000 | null                | 100        |
    | 2      | 2       | 2014-11-25T00:00:04.000 | 2014-11-25T00:01:05.000 | VirtualDesign       | null       |
    | 3      | 2       | 2014-11-25T00:00:06.000 | 2014-11-25T00:00:07.000 | null                | 100        |
    | 4      | 2       | 2014-11-25T00:27:45.432 | 2014-11-25T00:27:45.434 | null                | 100        |
    | 5      | 2       | 2014-11-25T00:27:45.432 | 2014-11-25T00:27:45.434 | VirtualDesign       | null       |
    | 6      | 2       | 2014-11-25T00:27:45.432 | 2014-11-25T00:27:45.434 | VirtualDesign       | 100        |
    | 7      | 2       | 2014-11-25T00:27:45.434 | 2014-11-25T00:27:45.436 | VirtualDesign       | 200        |
    | 8      | 2       | 2014-11-25T00:27:45.432 | 2014-11-25T00:27:45.434 | Random              | Random     |
    | 9      | 2       | 2014-11-25T00:27:55.376 | 2014-11-25T00:38:45.559 | VirtualDesign       | 100        |

Scenario: EditData - Insert Design Edit
  Given I submit the following data edits to project 1001285 
  | EditId |
  | 0      |
  | 2      |
  When I try to get all edits for project 1001285
  Then the result should contain the following data edits
  | EditId |
  | 0      |
  | 2      |

Scenario: EditData - Insert Lift Edit
  Given I submit the following data edits to project 1001285
  | EditId |
  | 1      |
  | 3      |
  When I try to get all edits for project 1001285
  Then the result should contain the following data edits
  | EditId |
  | 1      |
  | 3      |

Scenario: EditData - Lift and Design Edits Consolidation
  Given I submit the following data edits to project 1001285
  | EditId |
  | 4      |
  | 5      |
  When I try to get all edits for project 1001285
  Then the result should contain the following data edits
  | EditId |
  | 6      |

Scenario: EditData - Insert Temporally Contiguous Edits
  Given I submit the following data edits to project 1001285
  | EditId |
  | 6      |
  | 7      |
  When I try to get all edits for project 1001285
  Then the result should contain the following data edits
  | EditId |
  | 6      |
  | 7      |

Scenario: EditData - Insert Lift Edit Overlapping Real Lift Exactly
  Given I submit the following data edits to project 1001285
        | EditId |
        | 9      |
  When I read back all machine designs from "/api/v1/projects/{0}/machinedesigns" for project 1001285
    And I read back all lifts from "/api/v1/projects/{0}/liftids" for project 1001285
  Then the lift list should contain the lift details in the following data edits
    | EditId |
    | 9      |
@ignore
Scenario: EditData - Read Back Edits
  Given I submit the following data edits to project 1001285
         | EditId |
         | 8      |
  When I read back all machine designs from "/api/v1/projects/{0}/machinedesigns" for project 1001285
    And I read back all lifts from "/api/v1/projects/{0}/liftids" for project 1001285
  Then the machine design list should contain the design details in the following data edits
    | EditId |
    | 8      |
    And the lift list should contain the lift details in the following data edits
      | EditId |
      | 8      |

Scenario: EditData - Undo Single Edit
  Given I submit the following data edits to project 1001285 
  | EditId |
  | 0      |
  When I try to get all edits for project 1001285
    And the result matches the following data edits
    | EditId |
    | 0      |
    And I try to undo the following edits for project 1001285
    | EditId |
    | 0      |
    And I try to get all edits for project 1001285
  Then the result should be empty

Scenario: EditData - Use Lift Edit in Filter
  Given I submit the following data edits to project 1001285
  | EditId |
  | 4      |
  When I request "Height" from resource "/api/v1/productiondata/cells/datum" at Grid Point (381447.523, 806857.580) for project 1001285 filtered by EditId 4
  Then the datum should be: displayMode = "0", returnCode = "0", value = "38.073001861572266", timestamp = "2014-11-25T00:27:45.433"

Scenario: EditData - Use Design Edit in Filter
  Given I submit the following data edits to project 1001285
  | EditId |
  | 5      |
  When I request "Height" from resource "/api/v1/productiondata/cells/datum" at Grid Point (381447.523, 806857.580) for project 1001285 filtered by EditId 5
  Then the datum should be: displayMode = "0", returnCode = "0", value = "38.073001861572266", timestamp = "2014-11-25T00:27:45.433"

Scenario: EditData - Use Both Lift and Design Edits in Filter
  Given I submit the following data edits to project 1001285
  | EditId |
  | 6      |
  When I request "Height" from resource "/api/v1/productiondata/cells/datum" at Grid Point (381447.523, 806857.580) for project 1001285 filtered by EditId 6
  Then the datum should be: displayMode = "0", returnCode = "0", value = "38.073001861572266", timestamp = "2014-11-25T00:27:45.433"

Scenario: EditData - Bad Request (Insert Overlapping Edits)
  Given I submit the following data edits to project 1001285
  | EditId |
  | 0      |
    And I submit data edit with EditId 0 to project 1001285 expecting HttpResponseCode 400
  Then I should get Error Code -1 and Message "Data edit overlaps"