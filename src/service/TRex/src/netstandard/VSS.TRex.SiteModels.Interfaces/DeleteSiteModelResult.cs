namespace VSS.TRex.SiteModels.Interfaces
{
  public enum DeleteSiteModelResult
  {
    UnknownError = 0,
    OK = 1,
    UnhandledException = 2,
    UnableToLocateSiteModel = 3,
    RequestNotMadeToMutableGrid = 4,
    FailedToRemoveSubGrids = 5,
    FailedToRemoveProjectMetadata = 6,
    FailedToCommitPrimaryElementRemoval = 7,
    FailedToCommitExistenceMapRemoval = 8,
    FailedToRemoveSiteDesigns = 9,
    FailedToRemoveSurveyedSurfaces = 10,
    FailedToRemoveAlignments = 11,
    FailedToRemoveCSIB = 12
  }
}
