namespace VSS.TRex.Types
{
  /// <summary>
  /// Notes the type of surveyed surface patch result required from a surveyed surface patch request
  /// </summary>
  public enum SurveyedSurfacePatchType : byte
  {
    /// <summary>
    /// The latest (in time) available elevation at each location from a set of surveyed surfaces
    /// </summary>
    LatestSingleElevation,

    /// <summary>
    /// The earliest (in time) available elevation at each location from a set of surveyed surfaces
    /// </summary>
    EarliestSingleElevation,

    /// <summary>
    /// THe analyzed first, last, lowest and highest elevations at each location from a set of surveyed surfaces
    /// </summary>
    CompositeElevations
  }
}
