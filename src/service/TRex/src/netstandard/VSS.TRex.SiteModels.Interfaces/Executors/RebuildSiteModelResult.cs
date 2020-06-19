namespace VSS.TRex.SiteModels.Interfaces
{
  public enum RebuildSiteModelResult
  {
    UnknownError = 0,
    OK = 1,
    UnhandledException = 2,
    UnableToLocateSiteModel = 3,
    FailedToDeleteSiteModel = 4,
    Pending = 5
  }
}
