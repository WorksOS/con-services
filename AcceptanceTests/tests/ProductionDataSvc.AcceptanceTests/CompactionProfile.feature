Feature: CompactionProfile
I should be able to request Compaction Profile data.

Scenario: Compaction Get Slicer Empty Profile
Given the Compaction Profile service URI "/api/v2/profiles/productiondata/slicer"
And the result file "Profiles/ProfileSummaryResponse.json"	
And a projectUid "7925f179-013d-4aaf-aff4-7b9833bb06d6"
And a startLatDegrees "36.209310" and a startLonDegrees "-115.019584" and an endLatDegrees "36.209322" And an endLonDegrees "-115.019574"
When I request a Compaction Profile 
Then the Compaction Profile result should be match expected "EmptyResponse"

#Until solved problem with variance in values returned from Raptor. Tests may have to compensate for inaccuraces at high decimal precion.
#@Ignore
Scenario: Compaction Get Slicer Profile
Given the Compaction Profile service URI "/api/v2/profiles/productiondata/slicer"
And the result file "Profiles/ProfileSummaryResponse.json"	
And a projectUid "7925f179-013d-4aaf-aff4-7b9833bb06d6"
And a startLatDegrees "36.207310" and a startLonDegrees "-115.019584" and an endLatDegrees "36.207322" And an endLonDegrees "-115.019574"
And a cutfillDesignUid "dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff"
When I request a Compaction Profile 
Then the Compaction Profile result should be match expected "CutfillProfile"

#Until solved problem with variance in values returned from Raptor. Tests may have to compensate for inaccuraces at high decimal precion.
#@Ignore
Scenario: Compaction Get Slicer Summary Volumes Profile
Given the Compaction Profile service URI "/api/v2/profiles/productiondata/slicer"
And the result file "Profiles/ProfileSummaryResponse.json"	
And a projectUid "ff91dd40-1569-4765-a2bc-014321f76ace"
And a startLatDegrees "36.206627682520867" and a startLonDegrees "-115.0235567314591" and an endLatDegrees "36.206612363570869" And an endLonDegrees "-115.02356429221605"
And a volumeCalcType "GroundToGround" and a topUid "A40814AA-9CDB-4981-9A21-96EA30FFECDD" and a baseUid "F07ED071-F8A1-42C3-804A-1BDE7A78BE5B"
When I request a Compaction Profile 
Then the Compaction Profile result should be match expected "G2Gvolumes"
