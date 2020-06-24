using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

namespace VSS.TRex.SiteModels.Interfaces
{
  public enum RebuildSiteModelResult : byte
  {
    UnknownError = 0,
    OK = 1,
    UnhandledException = 2,
    UnableToLocateSiteModel = 3,
    FailedToDeleteSiteModel = 4,
    Pending = 5,
    UnableToLocateTAGFileKeyCollection = 6,
    Aborted = 7
  }
}
