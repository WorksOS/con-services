Feature: GetLifts
  I should be able to request all lift ids of a project

Scenario: GetLifts - Project With One Asset
  Given only the service route "/api/v1/projects/1001280/liftids"
  When I send the GET request I expect response code 200
  Then the following lift details should be returned
  | AssetId          | DesignId | LayerId | StartDate               | EndDate                 |
  | 4035848580437757 | 1        | 8       | 2014-04-10T03:26:11.457 | 2014-04-10T03:51:06.274 |
  | 4035848580437757 | 0        | 8       | 2014-04-15T02:25:57.665 | 2014-04-15T02:32:46.369 |
  | 4035848580437757 | 1        | 8       | 2014-04-15T02:32:59.356 | 2014-04-15T03:25:39.495 |
  | 4035848580437757 | 4        | 8       | 2014-04-15T03:34:04.966 | 2014-04-15T03:59:05.352 |

Scenario: GetLifts - Project With Two Assets
  Given only the service route "/api/v1/projects/1001214/liftids"
  When I send the GET request I expect response code 200
  Then the following lift details should be returned
  | AssetId | DesignId | LayerId | StartDate               | EndDate                 |
  | 1111    | 1        | 3       | 2014-11-25T00:25:40.564 | 2014-11-25T00:30:59.227 |
  | 1111    | 2        | 2       | 2014-11-25T00:30:59.227 | 2014-11-25T00:31:25.827 |
  | 1111    | 2        | 3       | 2014-11-25T00:31:25.827 | 2014-11-25T00:32:24.927 |
  | 1111    | 2        | 2       | 2014-11-25T00:32:24.927 | 2014-11-25T00:32:28.327 |
  | 1111    | 2        | 1       | 2014-11-25T00:32:28.327 | 2014-11-25T00:35:40.827 |
  | 2222    | 1        | 2       | 2014-11-25T00:07:55.193 | 2014-11-25T00:11:06.989 |
  | 2222    | 1        | 3       | 2014-11-25T00:11:06.989 | 2014-11-25T00:14:05.788 |
  | 2222    | 1        | 4       | 2014-11-25T00:14:05.788 | 2014-11-25T00:17:55.588 |

#Scenario: GetLifts - Bad Request (Invalid Project ID)
#  Given a GetLifts project Id 0
#  When I request lift ids expecting Bad Request
#  Then the response should contain Code -2 and Message "Invalid project ID: 0"