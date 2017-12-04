namespace VSS.Productivity3D.WebApi.Models.Compaction.Models.Reports
{
  public class StationOffsetRow : ReportRowBase
  {
    public double Offset { get; private set; }
    public double Station { get; private set; }

    /// <summary>
    /// Create an instance of the GridRoW class.
    /// </summary>
    /// <returns>Returns an instance of <see cref="StationOffsetRow"/>.</returns>
    public static StationOffsetRow CreateRow(
      double northing,
      double easting,
      double elevation,
      double cutFill,
      short cmv,
      short mdp,
      int passCount,
      double temperature,
      double offset,
      double station)
    {
      return new StationOffsetRow
      {
        CMV = cmv,
        CutFill = cutFill,
        Easting = easting,
        Elevation = elevation,
        MDP = mdp,
        Northing = northing,
        Offset = offset,
        Station = station,
        Temperature = temperature
      };
    }
  }
}