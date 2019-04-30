Feature: CompactionProfile
  I should be able to request Compaction Profile data.

Scenario: Compaction Get Slicer Empty Profile
  Given the service route "/api/v2/profiles/productiondata/slicer" and result repo "Profiles/ProfileSummaryResponse.json"
  And with parameter "projectUid" with value "7925f179-013d-4aaf-aff4-7b9833bb06d6"
  And with parameter "startLatDegrees" with value "36.209310"
  And with parameter "startLonDegrees" with value "-115.019584"
  And with parameter "endLatDegrees" with value "36.209322"
  And with parameter "endLonDegrees" with value "-115.019574"
  When I send the GET request I expect response code 200
  Then the response should match "EmptyResponse" from the repository

#This test has always been flakey and has been ignored the the past because it was an outline,
# TODO fix. Raptor can be non deterministic in the results that are returned for this test 
# depending on how the query is processed in the psnodes.
#Scenario: Compaction Get Slicer Profile
#  Given the service route "/api/v2/profiles/productiondata/slicer" and result repo "Profiles/ProfileSummaryResponse.json"
#  And with parameter "projectUid" with value "7925f179-013d-4aaf-aff4-7b9833bb06d6"
#  And with parameter "startLatDegrees" with value "36.207310"
#  And with parameter "startLonDegrees" with value "-115.019584"
#  And with parameter "endLatDegrees" with value "36.207322"
#  And with parameter "endLonDegrees" with value "-115.019574"
#  And with parameter "cutfillDesignUid" with value "dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff"
#  When I send the GET request I expect response code 200 
#  Then the response should match "CutfillProfile" from the repository

Scenario: Compaction Get Slicer Profile Minimum Elevation
  Given the service route "/api/v2/profiles/productiondata/slicer" and result repo "Profiles/ProfileSummaryResponse.json"
  And with parameter "projectUid" with value "0583c019-f92a-4762-94c1-ad13a98ecab6"
  And with parameter "startLatDegrees" with value "-43.51319878922578"
  And with parameter "startLonDegrees" with value "172.43846892786217"
  And with parameter "endLatDegrees" with value "-43.51319982260281"
  And with parameter "endLonDegrees" with value "172.43847161007116"
  And with parameter "filterUid" with value "6c7c899c-7320-446d-a525-d7ed1f285c5c"
  When I send the GET request I expect response code 200 
  Then the response should match "MinimumElevationSingleCell" from the repository

Scenario: Compaction Get Slicer Summary Volumes Profile
  Given the service route "/api/v2/profiles/productiondata/slicer" and result repo "Profiles/ProfileSummaryResponse.json"
  And with parameter "projectUid" with value "ff91dd40-1569-4765-a2bc-014321f76ace"
  And with parameter "startLatDegrees" with value "36.206627682520867"
  And with parameter "startLonDegrees" with value "-115.0235567314591"
  And with parameter "endLatDegrees" with value "36.206612363570869"
  And with parameter "endLonDegrees" with value "-115.02356429221605"
  And with parameter "volumeCalcType" with value "GroundToGround"
  And with parameter "volumeTopUid" with value "A40814AA-9CDB-4981-9A21-96EA30FFECDD"
  And with parameter "volumeBaseUid" with value "F07ED071-F8A1-42C3-804A-1BDE7A78BE5B"
  When I send the GET request I expect response code 200
  Then the response should match "G2Gvolumes" from the repository

Scenario: Compaction Get Slicer Summary Volumes Profile - Explicit
  Given the service route "/api/v2/profiles/productiondata/slicer" and result repo "Profiles/ProfileSummaryResponse.json"
  And with parameter "projectUid" with value "ff91dd40-1569-4765-a2bc-014321f76ace"
  And with parameter "startLatDegrees" with value "36.207012189850786"
  And with parameter "startLonDegrees" with value "-115.02051150886813"
  And with parameter "endLatDegrees" with value "36.20701191932048"
  And with parameter "endLonDegrees" with value "-115.02030833153526"
  And with parameter "volumeCalcType" with value "GroundToGround"
  And with parameter "volumeTopUid" with value "ba24a225-12f3-4525-940b-ec8720e7a4f4"
  And with parameter "volumeBaseUid" with value "8d9c19f6-298f-4ec2-8688-cc72242aaceb"
  And with parameter "explicitFilters" with value "true"
  When I send the GET request I expect response code 200
  Then the response should match "G2GvolumesExplicit" from the repository

Scenario: Compaction Get Slicer Summary Volumes Profile - Implicit
  Given the service route "/api/v2/profiles/productiondata/slicer" and result repo "Profiles/ProfileSummaryResponse.json"
  And with parameter "projectUid" with value "ff91dd40-1569-4765-a2bc-014321f76ace"
  And with parameter "startLatDegrees" with value "36.207012189850786"
  And with parameter "startLonDegrees" with value "-115.02051150886813"
  And with parameter "endLatDegrees" with value "36.20701191932048"
  And with parameter "endLonDegrees" with value "-115.02030833153526"
  And with parameter "volumeCalcType" with value "GroundToGround"
  And with parameter "volumeTopUid" with value "ba24a225-12f3-4525-940b-ec8720e7a4f4"
  And with parameter "volumeBaseUid" with value "8d9c19f6-298f-4ec2-8688-cc72242aaceb"
  And with parameter "explicitFilters" with value "false"
  When I send the GET request I expect response code 200
  Then the response should match "G2GvolumesImplicit" from the repository