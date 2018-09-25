namespace VSS.Productivity3D.Models.Enums
{
  /// <summary>
  /// TagFile submitted to TRex Gateway result codes
  /// </summary>
  public enum TRexTagFileResultCode
  {
    // TFA codes are in the 3000 range
    // Trex Mutable Server codes are < 3000
    Valid = 0,
    TRexUnknownException = 1,
    TRexBadRequestMissingProjectUid = 2,
    TRexInvalidTagfile = 3,
    TfaException = 4,
    TRexQueueSubmissionError = 5,
    TrexTagFileReaderError = 6,

    TFABadRequestInvalidLatitude = 3021,
    TFABadRequestInvalidLongitude = 3022,
    TFABadRequestInvalidTimeOfPosition = 3023,
    TFAInternalDatabaseException = 3028,
    TFABadRequestInvalidDeviceType = 3030,
    TFABadRequestInvalidProjectUid = 3036,
    TFABadRequestMissingRadioSerialAndTccOrgId = 3037,
    TFAManualProjectNotFound = 3038,
    TFAManualAssetFoundButNoSubsOrProjectFound = 3039,
    TFAManualNoAssetFoundAndNoProjectSubs = 3040,
    TFAManualNoIntersectingProjectsFound = 3041,
    TFAManualProjectDoesNotIntersectTimeAndLocation = 3042,
    TFAManualProjectIsArchived = 3043,
    TFAManualProjectIsCivilType = 3044,
    TFAManualLandfillHasNoSubsAtThisTime = 3045,
    TFAManualInternalErrorUnhandledPath = 3046,
    TFAAutoNoAssetOrTccOrgIdFound = 3047,
    TFAAutoAssetOrTccOrgIdFoundButNoProject = 3048,
    TFAAutoMultipleProjectsMatchCriteria = 3049,
    TFAManualValidProjectsFoundButNotRequestedOne = 3050,
  }
}