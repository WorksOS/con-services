Feature: GetProjectMachines
  I should be able to get the list of machines that have contributed TAG file information into a project.

Scenario: GetProjectMachines - Get All Machine Details of a Project
  Given only the service route "/api/v1/projects/1001214/machines"
  When I send the GET request I expect response code 200
  Then the following machines should be returned:
    | lastKnownDesignName          | lastKnownLayerId | lastKnownTimeStamp      | lastKnownLatitude   | lastKnownLongitude | lastKnownX         | lastKnownY        | assetID | machineName | isJohnDoe |
    | Pound Road_BaseCourse_140213 | 1                | 2014-11-25T00:35:40.827 | -0.7597157838976566 | 3.010641326432008  | 381414.97453135636 | 806868.0291519845 | 1111    | COMPACTOR   | False     |
    | Pound Road_SubBase_140213    | 4                | 2014-11-25T00:17:55.588 | -0.759717712410056  | 3.010648224187052  | 381446.956999995   | 806855.8410000018 | 2222    | ACOM1       | False     |

Scenario: GetProjectMachines - Get One Machine Details of a Project
  Given only the service route "/api/v1/projects/1001214/machines/1111"
  When I send the GET request I expect response code 200
  Then the following machines should be returned:
    | lastKnownDesignName          | lastKnownLayerId | lastKnownTimeStamp      | lastKnownLatitude   | lastKnownLongitude | lastKnownX         | lastKnownY        | assetID | machineName | isJohnDoe |
    | Pound Road_BaseCourse_140213 | 1                | 2014-11-25T00:35:40.827 | -0.7597157838976566 | 3.010641326432008  | 381414.97453135636 | 806868.0291519845 | 1111    | COMPACTOR   | False     |
