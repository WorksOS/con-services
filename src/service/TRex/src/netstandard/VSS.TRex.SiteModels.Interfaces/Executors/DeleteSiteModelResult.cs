namespace VSS.TRex.SiteModels.Interfaces
{
  public enum DeleteSiteModelResult : byte
  {
    UnknownError = 0,
    OK = 1,
    UnhandledException = 2,
    UnableToLocateSiteModel = 3,
    FailedToRemoveSubGrids = 4,
    FailedToRemoveProjectMetadata = 5,
    FailedToCommitPrimaryElementRemoval = 6,
    FailedToCommitExistenceMapRemoval = 7,
    FailedToRemoveSiteDesigns = 8,
    FailedToRemoveSurveyedSurfaces = 9,
    FailedToRemoveAlignments = 10,
    FailedToRemoveCSIB = 11,

    /// <summary>
    /// Notes if another request (eg: project rebuilding) is pending a deletion operation
    /// </summary>
    Pending = 12
  }
}
