namespace VSS.TRex.SiteModels.Interfaces
{
  public enum DeleteSiteModelResult
  {
    OK,
    UnknownError,
    UnhandledException,
    UnableToLocateSiteModel,
    RequestNotMadeToMutableGrid,
    FailedToRemoveSubGrids,
    FailedToRemoveProjectMetadata,
    FailedToCommitPrimaryElementRemoval,
    FailedToCommitExistenceMapRemoval,
    FailedToRemoveSiteDesigns,
    FailedToRemoveSurveyedSurfaces,
    FailedToRemoveAlignments,
    FailedToRemoveCSIB
  }
}
