namespace VSS.Productivity3D.WebApi.Models.Compaction.Models.Reports
{
  /// <summary>
  /// Defines a grid report row.
  /// </summary>
  public class GridRow : ReportRowBase
  {
    /// <summary>
    /// Create an instance of the <see cref="GridRow"/> class.
    /// </summary>
    /// <returns>An instance of the <see cref="GridRow"/> class.</returns>
    public static GridRow CreateRow(
      double northing,
      double easting,
      double elevation,
      double cutFill,
      short cmv,
      short mdp,
      int passCount,
      double temperature)
    {
      return new GridRow
      {
        Northing = northing,
        Easting = easting,
        Elevation = elevation,
        CutFill = cutFill,
        CMV = cmv,
        MDP = mdp,
        PassCount = passCount,
        Temperature = temperature
      };
    }
  }
}