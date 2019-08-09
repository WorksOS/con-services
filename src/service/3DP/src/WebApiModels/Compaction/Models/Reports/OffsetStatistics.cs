#if RAPTOR
using ASNodeRaptorReports;
#endif
using VSS.Productivity3D.WebApi.Models.Report.ResultHandling;

namespace VSS.Productivity3D.WebApi.Models.Compaction.Models.Reports
{
  public class OffsetStatistics : ReportRowBase
  {
    //These are not used for statistics
    public override bool ShouldSerializeNorthing()
    {
      return false;
    }
    public override bool ShouldSerializeEasting()
    {
      return false;
    }

#if RAPTOR
    public static OffsetStatistics Create(OffsetStatisticType type, TStation station, CompactionReportRequest request)
    {
      var offsetStatistics = new OffsetStatistics();

      switch (type)
      {
        case OffsetStatisticType.Maximum:
          offsetStatistics.SetValues(0, 0, station.ElevMax, station.CutFillMax, station.CMVMax, station.MDPMax, station.PassCountMax, station.TemperatureMax);
          break;
        case OffsetStatisticType.Minimum:
          offsetStatistics.SetValues(0, 0, station.ElevMin, station.CutFillMin, station.CMVMin, station.MDPMin, station.PassCountMin, station.TemperatureMin);
          break;
        case OffsetStatisticType.Average:
          offsetStatistics.SetValues(0, 0, station.ElevAvg, station.CutFillAvg, station.CMVAvg, station.MDPAvg, station.PassCountAvg, station.TemperatureAvg);
          break;
      }
      offsetStatistics.SetReportFlags(request);
      return offsetStatistics;
    }
#endif

    public static OffsetStatistics Create(OffsetStatisticType type, StationOffsetReportDataRow station, CompactionReportRequest request)
    {
      var offsetStatistics = new OffsetStatistics();

      switch (type)
      {
        case OffsetStatisticType.Maximum:
          offsetStatistics.SetValues(0, 0, station.Maximum.Elevation, station.Maximum.CutFill, station.Maximum.Cmv, station.Maximum.Mdp, station.Maximum.PassCount, station.Maximum.Temperature);
          break;
        case OffsetStatisticType.Minimum:
          offsetStatistics.SetValues(0, 0, station.Minimum.Elevation, station.Minimum.CutFill, station.Minimum.Cmv, station.Minimum.Mdp, station.Minimum.PassCount, station.Minimum.PassCount);
          break;
        case OffsetStatisticType.Average:
          offsetStatistics.SetValues(0, 0, station.Average.Elevation, station.Average.CutFill, station.Average.Cmv, station.Average.Mdp, station.Average.PassCount, station.Average.Temperature);
          break;
      }
      offsetStatistics.SetReportFlags(request);
      return offsetStatistics;
    }
  }
}
