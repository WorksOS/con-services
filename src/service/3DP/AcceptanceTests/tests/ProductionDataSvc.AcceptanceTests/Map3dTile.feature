Feature: Map3dTile
I should be able to request tiles for use in the Map 3d

Scenario Outline: Get Heightmap with no design filter for all modes
Given the Map3d service URI "/api/v2/map3d"
And the result file "Map3dGetDataTilesResponse.json"
And projectUid "<ProjectUID>"
And mode "<Mode>" and type "<Type>" and bbox "<BBox>" and width "<Width>" and height "<Height>"
When I request result
Then the result tile should match the "<ResponseName>" from the repository within "<Difference>" percent
Examples: 
| RequestName            | ResponseName      | ProjectUID                           | BBox                                                                  | Width | Height | Mode | Type | Difference |
| Height Mode Request    | HeightMapNoFilter | ff91dd40-1569-4765-a2bc-014321f76ace | 36.2063171248096,-115.021142444626,36.2083428474075,-115.017457089439 | 256   | 256    | 0    | 0    | 5          |
| CMV Mode Request       | HeightMapNoFilter | ff91dd40-1569-4765-a2bc-014321f76ace | 36.2063171248096,-115.021142444626,36.2083428474075,-115.017457089439 | 256   | 256    | 1    | 0    | 5          |
| PassCount Mode Request | HeightMapNoFilter | ff91dd40-1569-4765-a2bc-014321f76ace | 36.2063171248096,-115.021142444626,36.2083428474075,-115.017457089439 | 256   | 256    | 4    | 0    | 5          |
| Amplitude Mode Request | HeightMapNoFilter | ff91dd40-1569-4765-a2bc-014321f76ace | 36.2063171248096,-115.021142444626,36.2083428474075,-115.017457089439 | 256   | 256    | 7    | 0    | 5          |

Scenario Outline: Get textures
Given the Map3d service URI "/api/v2/map3d"
And the result file "Map3dGetDataTilesResponse.json"
And projectUid "<ProjectUID>"
And mode "<Mode>" and type "<Type>" and bbox "<BBox>" and width "<Width>" and height "<Height>"
When I request result
Then the result tile should match the "<ResponseName>" from the repository within "<Difference>" percent
Examples: 
| RequestName             | ResponseName                 | ProjectUID                           | BBox                                                                  | Width | Height | Mode | Type | Difference |
| Height Mode Texture     | HeightModeTextureNoFilter    | ff91dd40-1569-4765-a2bc-014321f76ace | 36.2063171248096,-115.021142444626,36.2083428474075,-115.017457089439 | 256   | 256    | 0    | 2    | 5          |
| CCV Mode Texture        | CCVModeTextureNoFilter       | ff91dd40-1569-4765-a2bc-014321f76ace | 36.2063171248096,-115.021142444626,36.2083428474075,-115.017457089439 | 256   | 256    | 1    | 2    | 5          |
| Pass Count Mode Texture | PassCountModeTextureNoFilter | ff91dd40-1569-4765-a2bc-014321f76ace | 36.2063171248096,-115.021142444626,36.2083428474075,-115.017457089439 | 256   | 256    | 4    | 2    | 5          |

Scenario Outline: Get height maps and textures with filters
Given the Map3d service URI "/api/v2/map3d"
And the result file "Map3dGetDataTilesResponse.json"
And projectUid "<ProjectUID>"
And filterUid "<FilterUID>"
And mode "<Mode>" and type "<Type>" and bbox "<BBox>" and width "<Width>" and height "<Height>"
When I request result
Then the result tile should match the "<ResponseName>" from the repository within "<Difference>" percent
Examples: 
| RequestName                                                             | ResponseName               | ProjectUID                           | FilterUID                            | BBox                                                                  | Width | Height | Mode | Type | Difference |
| Height Map and mode with Filter                                         | HeightMapFilter            | ff91dd40-1569-4765-a2bc-014321f76ace | 3ef41e3c-d1f5-40cd-b012-99d11ff432ef | 36.2063171248096,-115.021142444626,36.2083428474075,-115.017457089439 | 256   | 256    | 0    | 0    | 5          |
| CCV Mode for Height Map (Should return same as height mode) with filter | HeightMapFilter            | ff91dd40-1569-4765-a2bc-014321f76ace | 3ef41e3c-d1f5-40cd-b012-99d11ff432ef | 36.2063171248096,-115.021142444626,36.2083428474075,-115.017457089439 | 256   | 256    | 1    | 0    | 5          |
| Height Mode Texture with Filter                                         | HeightModeTextureFilter    | ff91dd40-1569-4765-a2bc-014321f76ace | 3ef41e3c-d1f5-40cd-b012-99d11ff432ef | 36.2063171248096,-115.021142444626,36.2083428474075,-115.017457089439 | 256   | 256    | 0    | 2    | 5          |
| CCV Mode Texture with Filter                                            | CCVModeTextureFilter       | ff91dd40-1569-4765-a2bc-014321f76ace | 3ef41e3c-d1f5-40cd-b012-99d11ff432ef | 36.2063171248096,-115.021142444626,36.2083428474075,-115.017457089439 | 256   | 256    | 1    | 2    | 5          |
| Pass Count Mode Texture with Filter                                     | PassCountModeTextureFilter | ff91dd40-1569-4765-a2bc-014321f76ace | 3ef41e3c-d1f5-40cd-b012-99d11ff432ef | 36.2063171248096,-115.021142444626,36.2083428474075,-115.017457089439 | 256   | 256    | 4    | 2    | 5          |

Scenario Outline: Attempt to get design
Given the Map3d service URI "/api/v2/map3d"
And the result file "Map3dGetDataTilesResponse.json"
And projectUid "<ProjectUID>"
And designUid "<DesignUID>"
And mode "<Mode>" and type "<Type>" and bbox "<BBox>" and width "<Width>" and height "<Height>"
When I request result
Then the result tile should match the "<ResponseName>" from the repository within "<Difference>" percent
Examples: 
| RequestName       | ResponseName    | ProjectUID                           | FilterUID                            | DesignUID                            | BBox                                                                  | Width | Height | Mode | Type | Difference |
| Design Height Map | DesignHeightMap | ff91dd40-1569-4765-a2bc-014321f76ace | 3ef41e3c-d1f5-40cd-b012-99d11ff432ef | dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff | 36.2063171248096,-115.021142444626,36.2083428474075,-115.017457089439 | 256   | 256    | 0    | 1    | 5          |