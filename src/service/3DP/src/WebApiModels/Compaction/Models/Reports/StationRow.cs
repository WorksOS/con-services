using ASNodeRaptorReports;
using VSS.Productivity3D.Models.Models.Reports;

namespace VSS.Productivity3D.WebApi.Models.Compaction.Models.Reports
{
  public class StationRow
  {
    public double Station { get; set; }
    public StationOffsetRow[] Offsets { get; set; }
    public OffsetStatistics Minimum { get; set; }
    public OffsetStatistics Maximum { get; set; }
    public OffsetStatistics Average { get; set; }

    public static StationRow Create(TStation station, CompactionReportStationOffsetRequest request)
    {
      var offsetCount = station.NumberOfOffsets;

      var row = new StationRow
      {
        Station = station.Station,
        Offsets = new StationOffsetRow[offsetCount],
        Maximum = OffsetStatistics.Create(OffsetStatisticType.Maximum, station, request),
        Minimum = OffsetStatistics.Create(OffsetStatisticType.Minimum, station, request),
        Average = OffsetStatistics.Create(OffsetStatisticType.Average, station, request)
      };
      return row;
    }
  }
}
