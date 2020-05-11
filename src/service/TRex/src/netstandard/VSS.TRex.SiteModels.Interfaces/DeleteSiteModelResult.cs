namespace VSS.TRex.SiteModels.Interfaces
{
  public enum DeleteSiteModelResult
  {
    OK,
    UnknownError,
    UnhandledException,
    RequestNotMadeToMutableGrid,
    FailedToRemoveSubGridSegment,
    FailedToRemoveSubGridDirectory,
  }
}
