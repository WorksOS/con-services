using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace VSS.TRex.TAGFiles.Classes.Validator
{
  /// <summary>
  /// Result of validation check
  /// </summary>
  public enum ValidationResult
  {
    // positive return types come from TFA
    // negative means problem occured in Mutable Server
    Valid = 0,
    Unknown = -1,
    BadRequest = -2,
    InvalidTagfile = -3,
    TfaFailedValidation = -4,
    TfaException = -5,
    MissingConfiguration = -6,
    QueueSubmissionError = -7
  }



  // todo
  /*
 public ContractExecutionStatesEnum()
    {
      DynamicAddwithOffset("success", 0);
      DynamicAddwithOffset("TccOrgId, if present, must be a valid Guid.", 1);
      DynamicAddwithOffset("AssetId, if present, must be >= -1", 2);
      DynamicAddwithOffset("ProjectId, if present, must be -1, -2, -3 or > 0", 3);
      DynamicAddwithOffset("Must have valid error number", 4);
      DynamicAddwithOffset("Must have tag file name", 5);
      DynamicAddwithOffset("TagFileName invalid as no DisplaySerialNumber", 6);
      DynamicAddwithOffset("TagFileName invalid as no MachineName", 7);
      DynamicAddwithOffset("TagFileName invalid as no valid CreatedUtc", 8);
      DynamicAddwithOffset("Must have assetId", 9);
      DynamicAddwithOffset("Radio Serial is invalid", 10);
      DynamicAddwithOffset("TagFileProcessingErrorV1Executor: Invalid request structure", 11);
      DynamicAddwithOffset("TagFileProcessingErrorV1Executor: Failed to create an alert for tag file processing error",   12);
      DynamicAddwithOffset("TagFileProcessingErrorV2Executor: Invalid request structure", 13);
      DynamicAddwithOffset("TagFileProcessingErrorV2Executor: Failed to create an alert for tag file processing error", 14);
      DynamicAddwithOffset("Failed to get legacy asset id", 15);
      DynamicAddwithOffset("Failed to get project boundaries", 16);
      DynamicAddwithOffset("TagFileUTC must have occurred within last 50 years", 17);
      DynamicAddwithOffset("Must have projectId", 18);
      DynamicAddwithOffset("Failed to get project id", 19);
      DynamicAddwithOffset("Must contain one or more of assetId or tccOrgId", 20);
      DynamicAddwithOffset("Latitude should be between -90 degrees and 90 degrees", 21);
      DynamicAddwithOffset("Longitude should be between -180 degrees and 180 degrees", 22);
      DynamicAddwithOffset("TimeOfPosition must have occurred within last 50 years", 23);
      DynamicAddwithOffset("Must have assetId and/or projectID", 24);
      DynamicAddwithOffset("AssetId must have valid deviceType", 25);
      DynamicAddwithOffset("A manual/unknown deviceType must have a projectID", 26);
      DynamicAddwithOffset("Failed to get project boundary", 27);
      DynamicAddwithOffset("A problem occurred accessing database. Exception: {0}", 28);
      DynamicAddwithOffset("Unable to identify any projects", 29);
      DynamicAddwithOffset("DeviceType is invalid", 30);
      DynamicAddwithOffset("Unable to create Kafka event. Reason: {0}.", 31);
      DynamicAddwithOffset("Multiple projects found", 32);
      DynamicAddwithOffset("Unable to identify RadioSerial in the 3dPM system", 33);
      DynamicAddwithOffset("Failed to get project uid", 34);
      DynamicAddwithOffset("GetProjectUid internal error", 35);
      DynamicAddwithOffset("ProjectUid is present, but invalid", 36);
      DynamicAddwithOffset("Auto Import: Either Radio Serial or TCCOrgId must be provided", 37);
      DynamicAddwithOffset("Unable to find the Project requested", 38);
      DynamicAddwithOffset("Manual Import: unable to locate any valid subscriptions", 39);
      DynamicAddwithOffset("Manual Import: unable to locate any valid projectSubscriptions or locate asset", 40);
      DynamicAddwithOffset("Manual Import: no intersecting projects found", 41);
      DynamicAddwithOffset("Manual Import: project does not intersect location and time provided", 42);
      DynamicAddwithOffset("Manual Import: cannot import to an archived project", 43);
      DynamicAddwithOffset("Manual Import: cannot import to a Civil type project", 44);
      DynamicAddwithOffset("Manual Import: landfill project does not have a valid subscription at that time", 45);
      DynamicAddwithOffset("Manual Import: internal unhandled path", 46);
      DynamicAddwithOffset("Auto Import: no asset or tccOrgId identified", 47);
      DynamicAddwithOffset("Auto Import: no project meets the time/location/subscription requirements", 48);
      DynamicAddwithOffset("Auto Import: more than 1 project meets the time/location/subscription requirements", 49);
    }   
   *
   */


}
