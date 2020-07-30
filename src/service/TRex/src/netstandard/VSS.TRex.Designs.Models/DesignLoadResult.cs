namespace VSS.TRex.Designs.Models
{
  public enum DesignLoadResult
  {
    UnknownFailure, 
    Success,
    NoAlignmentsFound,
    NoMasterAlignmentsFound,
    UnableToLoadSubGridIndex,
    UnableToLoadSpatialIndex,
    DesignDoesNotExist,
    UnableToLoadBoundary,
    SiteModelNotFound
  }
}
