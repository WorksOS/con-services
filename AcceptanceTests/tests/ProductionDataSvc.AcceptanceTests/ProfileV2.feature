@ignored
Feature: ProfileV2
  I should be able to request Profile data.

Background: 
  Given the Profile service URI "/api/v2/profiles/design"
  And projectUid "<ProjectUID>"
  And startLatDegrees "<StartLatDegrees>" and endLatDegrees "<EndLatDegrees>"
  And startLonDegrees "<StartLonDegrees>" and endLonDegrees "<EndLonDegrees>"
  And importedFileUid "<ImportedFileUID>"
  And importedFileTypeId "<ImportedFileTypeId>"
  And filterUid "<FilterUID>"

Scenario Outline: Profile - Bad Request
  #When I request Profile supplying "<ParameterName>" parameters from the repos
  itory expecting http error code <httpCode>
  When I request result
  Then the response should contain error code <errorCode>
  Examples: 
	| ProjectUID | StartUTC | EndUTC | ResultName |
	| ff91dd40-1569-4765-a2bc-014321f76ace | 2017-01-01 | 2017-01-01 | NoData_ER  |