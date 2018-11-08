Feature: CompactionProfile
  I should be able to request Compaction Profile data.

Scenario Outline: Compaction Get Slicer Empty Profile
  Given the service route "/api/v2/profiles/productiondata/slicer" and result repo "Profiles/ProfileSummaryResponse.json"
  And with parameter "projectUid" with value "7925f179-013d-4aaf-aff4-7b9833bb06d6"
  And with parameter "startLatDegrees" with value "36.209310"
  And with parameter "startLonDegrees" with value "-115.019584"
  And with parameter "endLatDegrees" with value "36.209322"
  And with parameter "endLonDegrees" with value "-115.019574"
  When I send the GET request I expect response code 200
  Then the response should match "EmptyResponse" from the repository

Scenario Outline: Compaction Get Slicer Profile
  Given the service route "/api/v2/profiles/productiondata/slicer" and result repo "Profiles/ProfileSummaryResponse.json"
  And with parameter "projectUid" with value "7925f179-013d-4aaf-aff4-7b9833bb06d6"
  And with parameter "startLatDegrees" with value "36.207310"
  And with parameter "startLonDegrees" with value "-115.019584"
  And with parameter "endLatDegrees" with value "36.207322"
  And with parameter "endLonDegrees" with value "-115.019574"
  And with parameter "cutfillDesignUid" with value "dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff"
  When I send the GET request I expect response code 200 
  Then the response should match "CutfillProfile" from the repository

Scenario Outline: Compaction Get Slicer Summary Volumes Profile
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
