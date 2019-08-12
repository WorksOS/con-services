#if RAPTOR
using ASNodeRaptorReports;
#endif
using VSS.Productivity3D.WebApi.Models.Report.ResultHandling;

namespace VSS.Productivity3D.WebApi.Models.Compaction.Models.Reports
{
  public class StationRow
  {
    public double Station { get; set; }
    public StationOffsetRow[] Offsets { get; set; }
    public OffsetStatistics Minimum { get; set; }
    public OffsetStatistics Maximum { get; set; }
    public OffsetStatistics Average { get; set; }
#if RAPTOR
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
#endif
    public static StationRow Create(StationOffsetReportDataRow station, CompactionReportStationOffsetRequest request)
    {
      var offsetCount = station.Offsets.Count;

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
