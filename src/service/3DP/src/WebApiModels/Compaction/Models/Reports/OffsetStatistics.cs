using ASNodeRaptorReports;
using Newtonsoft.Json;

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

    public static OffsetStatistics Create(OffsetStatisticType type, TStation station, CompactionReportRequest request)
    {
      var offsetStatistics = new OffsetStatistics();

      switch (type)
      {
        case OffsetStatisticType.Maximum:
          {
            offsetStatistics.SetValues(0, 0, station.ElevMax, station.CutFillMax, station.CMVMax, station.MDPMax, station.PassCountMax, station.TemperatureMax);
          }
          break;
        case OffsetStatisticType.Minimum:
          {
            offsetStatistics.SetValues(0, 0, station.ElevMin, station.CutFillMin, station.CMVMin, station.MDPMin, station.PassCountMin, station.TemperatureMin);
          }
          break;
        case OffsetStatisticType.Average:
          {
            offsetStatistics.SetValues(0, 0, station.ElevAvg, station.CutFillAvg, station.CMVAvg, station.MDPAvg, station.PassCountAvg, station.TemperatureAvg);
          }
          break;
      }
      offsetStatistics.SetReportFlags(request);
      return offsetStatistics;
    }
  }
}
