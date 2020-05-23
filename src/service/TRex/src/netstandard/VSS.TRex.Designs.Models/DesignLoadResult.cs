namespace VSS.TRex.Designs.Models
{
  public enum DesignLoadResult
  {
    UnknownFailure, 
    Success,
    NoAlignmentsFound,
    UnableToLoadSubGridIndex,
    UnableToLoadSpatialIndex,
    DesignDoesNotExist,
    UnableToLoadBoundary
  }
}
