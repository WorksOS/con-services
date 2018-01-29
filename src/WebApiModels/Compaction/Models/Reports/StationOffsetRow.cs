using ASNodeRaptorReports;

namespace VSS.Productivity3D.WebApi.Models.Compaction.Models.Reports
{
  public class StationOffsetRow : ReportRowBase
  {
    public double Offset { get; private set; }
    public double Station { get; private set; }

    /// <summary>
    /// Static constructor for the <see cref="StationOffsetRow"/> type.
    /// </summary>
    /// <returns>Returns an instance of <see cref="StationOffsetRow"/> populated from the supplied <see cref="TStationOffset"/> object.</returns>
    public static StationOffsetRow CreateRow(TStationOffset offset)
    {
      return new StationOffsetRow
      {
        CMV = offset.CMV,
        CutFill = offset.CutFill,
        Easting = offset.Easting,
        Elevation = offset.Elevation,
        MDP = offset.MDP,
        PassCount = offset.PassCount,
        Northing = offset.Northing,
        Offset = offset.Position,
        Station = offset.Station,
        Temperature = offset.Temperature
      };
    }
  }
}