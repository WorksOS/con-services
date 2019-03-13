Feature: Map3dTile
    I should be able to request tiles for use in the Map 3d

Scenario Outline: Get Heightmap with no design filter for all modes
  Given the service route "/api/v2/map3d" and result repo "Map3dGetDataTilesResponse.json"
  And with parameter "projectUid" with value "<ProjectUID>"
  And with parameter "mode" with value "<Mode>"
  And with parameter "type" with value "<Type>"
  And with parameter "bbox" with value "<BBox>"
  And with parameter "width" with value "<Width>"
  And with parameter "height" with value "<Height>"
  When I send the GET request I expect response code <HttpCode>
  Then the resulting image should match "<ResultName>" from the response repository within <Difference> percent
  Examples: 
  | RequestName            | ResultName        | ProjectUID                           | BBox                                                                  | Width | Height | Mode | Type | Difference | HttpCode |
  | Height Mode Request    | HeightMapNoFilter | ff91dd40-1569-4765-a2bc-014321f76ace | 36.2063171248096,-115.021142444626,36.2083428474075,-115.017457089439 | 256   | 256    | 0    | 0    | 6          | 200      |
  | CMV Mode Request       | HeightMapNoFilter | ff91dd40-1569-4765-a2bc-014321f76ace | 36.2063171248096,-115.021142444626,36.2083428474075,-115.017457089439 | 256   | 256    | 1    | 0    | 6          | 200      |
  | PassCount Mode Request | HeightMapNoFilter | ff91dd40-1569-4765-a2bc-014321f76ace | 36.2063171248096,-115.021142444626,36.2083428474075,-115.017457089439 | 256   | 256    | 4    | 0    | 7          | 200      |
  | Amplitude Mode Request | HeightMapNoFilter | ff91dd40-1569-4765-a2bc-014321f76ace | 36.2063171248096,-115.021142444626,36.2083428474075,-115.017457089439 | 256   | 256    | 7    | 0    | 6          | 200      |

Scenario Outline: Get textures
  Given the service route "/api/v2/map3d" and result repo "Map3dGetDataTilesResponse.json"
  And with parameter "projectUid" with value "<ProjectUID>"
  And with parameter "mode" with value "<Mode>"
  And with parameter "type" with value "<Type>"
  And with parameter "bbox" with value "<BBox>"
  And with parameter "width" with value "<Width>"
  And with parameter "height" with value "<Height>"
  When I send the GET request I expect response code <HttpCode>
  Then the resulting image should match "<ResultName>" from the response repository within <Difference> percent
  Examples: 
  | RequestName             | ResultName                   | ProjectUID                           | BBox                                                                  | Width | Height | Mode | Type | Difference | HttpCode |
  | Height Mode Texture     | HeightModeTextureNoFilter    | ff91dd40-1569-4765-a2bc-014321f76ace | 36.2063171248096,-115.021142444626,36.2083428474075,-115.017457089439 | 256   | 256    | 0    | 2    | 6          | 200      |
  | CCV Mode Texture        | CCVModeTextureNoFilter       | ff91dd40-1569-4765-a2bc-014321f76ace | 36.2063171248096,-115.021142444626,36.2083428474075,-115.017457089439 | 256   | 256    | 1    | 2    | 6          | 200      |
  | Pass Count Mode Texture | PassCountModeTextureNoFilter | ff91dd40-1569-4765-a2bc-014321f76ace | 36.2063171248096,-115.021142444626,36.2083428474075,-115.017457089439 | 256   | 256    | 4    | 2    | 9          | 200      |

Scenario Outline: Get height maps and textures with filters
  Given the service route "/api/v2/map3d" and result repo "Map3dGetDataTilesResponse.json"
  And with parameter "projectUid" with value "<ProjectUID>"
  And with parameter "filterUid" with value "<FilterUID>"
  And with parameter "mode" with value "<Mode>"
  And with parameter "type" with value "<Type>"
  And with parameter "bbox" with value "<BBox>"
  And with parameter "width" with value "<Width>"
  And with parameter "height" with value "<Height>"
  When I send the GET request I expect response code <HttpCode>
  Then the resulting image should match "<ResultName>" from the response repository within <Difference> percent
  Examples: 
  | RequestName                                                             | ResultName                 | ProjectUID                           | FilterUID                            | BBox                                                                  | Width | Height | Mode | Type | Difference | HttpCode |
  | Height Map and mode with Filter                                         | HeightMapFilter            | ff91dd40-1569-4765-a2bc-014321f76ace | 3ef41e3c-d1f5-40cd-b012-99d11ff432ef | 36.2063171248096,-115.021142444626,36.2083428474075,-115.017457089439 | 256   | 256    | 0    | 0    | 6          | 200      |
  | CCV Mode for Height Map (Should return same as height mode) with filter | HeightMapFilter            | ff91dd40-1569-4765-a2bc-014321f76ace | 3ef41e3c-d1f5-40cd-b012-99d11ff432ef | 36.2063171248096,-115.021142444626,36.2083428474075,-115.017457089439 | 256   | 256    | 1    | 0    | 6          | 200      |
  | Height Mode Texture with Filter                                         | HeightModeTextureFilter    | ff91dd40-1569-4765-a2bc-014321f76ace | 3ef41e3c-d1f5-40cd-b012-99d11ff432ef | 36.2063171248096,-115.021142444626,36.2083428474075,-115.017457089439 | 256   | 256    | 0    | 2    | 6          | 200      |
  | CCV Mode Texture with Filter                                            | CCVModeTextureFilter       | ff91dd40-1569-4765-a2bc-014321f76ace | 3ef41e3c-d1f5-40cd-b012-99d11ff432ef | 36.2063171248096,-115.021142444626,36.2083428474075,-115.017457089439 | 256   | 256    | 1    | 2    | 6          | 200      |
  | Pass Count Mode Texture with Filter                                     | PassCountModeTextureFilter | ff91dd40-1569-4765-a2bc-014321f76ace | 3ef41e3c-d1f5-40cd-b012-99d11ff432ef | 36.2063171248096,-115.021142444626,36.2083428474075,-115.017457089439 | 256   | 256    | 4    | 2    | 6          | 200      |

Scenario Outline: Attempt to get design
  Given the service route "/api/v2/map3d" and result repo "Map3dGetDataTilesResponse.json"
  And with parameter "projectUid" with value "<ProjectUID>"
  And with parameter "designUid" with value "<DesignUID>"
  And with parameter "mode" with value "<Mode>"
  And with parameter "type" with value "<Type>"
  And with parameter "bbox" with value "<BBox>"
  And with parameter "width" with value "<Width>"
  And with parameter "height" with value "<Height>"
  When I send the GET request I expect response code <HttpCode>
  Then the resulting image should match "<ResultName>" from the response repository within <Difference> percent
  Examples: 
  | RequestName       | ResultName      | ProjectUID                           | FilterUID                            | DesignUID                            | BBox                                                                  | Width | Height | Mode | Type | Difference | HttpCode |
  | Design Height Map | DesignHeightMap | ff91dd40-1569-4765-a2bc-014321f76ace | 3ef41e3c-d1f5-40cd-b012-99d11ff432ef | dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff | 36.2063171248096,-115.021142444626,36.2083428474075,-115.017457089439 | 256   | 256    | 0    | 1    | 5          | 200      |