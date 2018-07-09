namespace VSS.Productivity3D.Models.Enums
{
  /// <summary>
  /// The different volume computation types available
  /// </summary>
  public enum VolumesType
  {
    /// <summary>
    /// Null value
    /// </summary>
    None = 0,

    /// <summary>
    /// Compute volumes above a specific elevation. Not currently available.
    /// </summary>
    AboveLevel = 1,

    /// <summary>
    /// Compute volumes between two specific elevations. Not currently available.
    /// </summary>
    Between2Levels = 2,

    /// <summary>
    /// Compute volumes above the levels defined using a supplied filter
    /// </summary>
    AboveFilter = 3,

    /// <summary>
    /// Compute volumes between the levels defined using a pair of supplied filters.
    /// </summary>
    Between2Filters = 4,

    /// <summary>
    /// Compute volumes defined by a filter (lower bound) and a design surface (upper bound)
    /// </summary>
    BetweenFilterAndDesign = 5,

    /// <summary>
    /// Compute volumes defined by a design surface (lower bound) and a filter (upper bound)
    /// </summary>
    BetweenDesignAndFilter = 6
  }
}
