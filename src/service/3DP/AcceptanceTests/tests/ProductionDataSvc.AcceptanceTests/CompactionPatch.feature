Feature: CompactionPatch
  I should be able to request Production Data Patch

Scenario Outline: Patch - Good Request
  Given the service route "/api/v2/patches" and result repo "CompactionPatchResponse.json"
  And with parameter "ecSerial" with value "<EcSerial>"
  And with parameter "radioSerial" with value "<RadioSerial>"
  And with parameter "tccOrgUid" with value "<TccOrgUid>"
  And with parameter "machineLatitude" with value "<MachineLatitude>"
  And with parameter "machineLongitude" with value "<MachineLongitude>"
  And with parameter "bottomLeftX" with value "<BottomLeftX>"
  And with parameter "bottomLeftY" with value "<BottomLeftY>"
  And with parameter "topRightX" with value "<TopRightX>"
  And with parameter "topRightY" with value "<TopRightY>"
  When I send the GET request I expect response code <HttpCode>
  Then the response should match "<ResultName>" from the repository
  Examples: 
  | EcSerial    | RadioSerial | TccOrgUid | MachineLatitude | MachineLongitude | BottomLeftX | BottomLeftY | TopRightX | TopRightY | ResultName           | HttpCode |
  | 6667J006YU  | 5051593854  |           | -43.518         | 172.448          | 2709.12     | 1218.56     | 2710      | 1220      | OneSubGrid           | 200      |

Scenario Outline: Patch - Good Request Protobuf
  Given the service route "/api/v2/patches" and result repo "CompactionPatchResponse.json"
  And with parameter "ecSerial" with value "<EcSerial>"
  And with parameter "radioSerial" with value "<RadioSerial>"
  And with parameter "tccOrgUid" with value "<TccOrgUid>"
  And with parameter "machineLatitude" with value "<MachineLatitude>"
  And with parameter "machineLongitude" with value "<MachineLongitude>"
  And with parameter "bottomLeftX" with value "<BottomLeftX>"
  And with parameter "bottomLeftY" with value "<BottomLeftY>"
  And with parameter "topRightX" with value "<TopRightX>"
  And with parameter "topRightY" with value "<TopRightY>"
  When I send a GET request with Accept header "application/x-protobuf" I expect response code <HttpCode>
  Then the deserialized result should match the "<ResultName>" result from the repository
  Examples: 
  | EcSerial    | RadioSerial | TccOrgUid | MachineLatitude | MachineLongitude | BottomLeftX | BottomLeftY | TopRightX | TopRightY | ResultName           | HttpCode |
  | 6667J006YU  | 5051593854  |           | -43.518         | 172.448          | 2709.12     | 1218.56     | 2710      | 1220      | OneSubGridProtobuf   | 200      |

Scenario Outline: Patch - Bad Request
  Given the service route "/api/v2/patches" and result repo "CompactionPatchResponse.json"
  And with parameter "ecSerial" with value "<EcSerial>"
  And with parameter "radioSerial" with value "<RadioSerial>"
  And with parameter "tccOrgUid" with value "<TccOrgUid>"
  And with parameter "machineLatitude" with value "<MachineLatitude>"
  And with parameter "machineLongitude" with value "<MachineLongitude>"
  And with parameter "bottomLeftX" with value "<BottomLeftX>"
  And with parameter "bottomLeftY" with value "<BottomLeftY>"
  And with parameter "topRightX" with value "<TopRightX>"
  And with parameter "topRightY" with value "<TopRightY>"
  When I send the GET request I expect response code <HttpCode>
  Then the response should match "<ResultName>" from the repository
  Examples: 
  | EcSerial    | RadioSerial | TccOrgUid | MachineLatitude | MachineLongitude | BottomLeftX | BottomLeftY | TopRightX | TopRightY | ResultName       | HttpCode |
  | 6667J006YU  | 5051593854  |           | 0               | 0                | 2709.12     | 1218.56     | 2710      | 1220      | InvalidLatLong   | 400      |
