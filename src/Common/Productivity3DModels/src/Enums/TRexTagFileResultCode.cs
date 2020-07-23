namespace VSS.Productivity3D.Models.Enums
{
  /// <summary>
  /// Result codes from tagFileSubmission process
  ///    From TRex, TFA and ProjectSvc
  /// These codes can be used by caller to determine
  ///   a) whether submission is re-try-able
  ///   b) category to archive the tagFile under 
  /// </summary>
  public enum TRexTagFileResultCode
  {
    // TFA codes are in the 3000 -3099 range
    // Trex Mutable Server codes are < 3000
    // CWS (via ProjectSvc codes are >= 3100

    // TRex 
    Valid = 0,
    TRexUnknownException = 1, // retry-able
            // "TRex unknown result (SubmitTAGFileResponse.Execute)"
    TRexBadRequestMissingProjectUid = 2, // internal test only (back door application of tag files)
    TRexInvalidTagfile = 3, 
    TRexTfaException = 4, // retry-able
            // "exception calling TFA"
    TRexQueueSubmissionError = 5, // retry-able
            //  "SubmitTAGFileResponse. Failed to submit tag file to processing queue. Request already exists"
    TRexTagFileReaderError = 6, 
            // message includes TAGReadResult e.g. InvalidDictionary
    TRexTagFileSubmissionQueueNotAvailable = 7, // retry-able
    TRexInvalidLatLong = 8,
            // $"#Progress# CheckFileIsProcessable. Unable to determine a tag file seed position. projectID {tagDetail.projectId} serialNumber {tagDetail.tagFileName} Lat {preScanState.SeedLatitude} Long {preScanState.SeedLongitude} northing {preScanState.SeedNorthing} easting {preScanState.SeedNorthing}";
    TRexMissingProjectIDRadioSerialAndEcmSerial = 9,
            // "#Progress# CheckFileIsProcessable. Must have either a valid RadioSerialNum or EC520SerialNum or ProjectUID"

    // TFA errors (via TRex) Validation errors  HttpStatusCode.BadRequest:
    TFABadRequestInvalidLatitude = 3021,
            // "Latitude should be between -90 degrees and 90 degrees"
    TFABadRequestInvalidLongitude = 3022,
            // "Longitude should be between -180 degrees and 180 degrees"
    TFABadRequestInvalidTimeOfPosition = 3023,
            // "TimeOfPosition must have occurred between 50 years ago and the next 2 days"
    TFABadRequestInvalidDeviceType = 3030,
            // "DeviceType is invalid"
    TFABadRequestInvalidProjectUid = 3036,
            // "ProjectUid is present, but invalid"
    TFABadRequestMissingRadioSerialAndEcmSerial = 3037,
    // "Auto Import: Either Radio Serial or ec520 Serial must be provided"

    // TFA Functional errors (via TRex) HttpStatusCode.OK   Response Code:
    TFAInternalServiceAccess = 3017, // retry-able
            // "A problem occurred accessing a service. Service: {0} Exception: {1}"
    TFANEtoLLError = 3018, 
            //"Manual Import: Unable to determine lat/long from northing/easting position"
    TFAManualProjectNotFound = 3038,
            // "Manual Import: Unable to find the Project requested"
    TFAManualNoIntersectingProjectsFound = 3041,
            // "Manual Import: project does not intersect the location provided"
    TFAManualProjectIsArchived = 3043,
            // "Manual Import: cannot import to an archived project"
    TFAAutoNoProjectsFoundAtLocation = 3044,
           // "Auto Import: No projects found at the location provided"
    TFAAutoNoDeviceFound = 3047,
           // "Auto Import: unable to identify the device by this serialNumber"
    TFAAutoDeviceFoundButNoProject = 3048,
           // "Auto Import: No projects found for this device"
    TFAAutoMultipleProjectsMatchCriteria = 3049,
            // "Auto Import: More than 1 project meets the location requirements"
    TFAManualProjectNotCorrectType = 3053,
            // "Manual Import: cannot import to a project which doesn't accept tag files"

    // cws errors (via ProjectSvc, via TFA, via TRex)
    CWSGetDeviceBySerialNumberNotFound = 3100,
           // "Unable to locate device by serialNumber in cws"
    CWSGetAccountForDeviceNotFound = 3102,
           // "Unable to locate any account for the device in cws"
    CWSGetAccountForDeviceFoundMultiple = 3103,
          // "There is >1 active account for the device in cws"
    CWSNoProjectsFoundForDevice = 3105,
          // "Unable to locate projects for device in cws"
    CWSEndpointException = 3124, // retry-able
           // "A problem occurred at the {0} endpoint. Exception: {1}" // this comes from within ProjectSvc
  }
}
