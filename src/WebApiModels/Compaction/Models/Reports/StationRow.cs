using ASNodeRaptorReports;

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

      return new StationRow
      {
        Station = station.Station,
        Offsets = new StationOffsetRow[offsetCount],
        Maximum = OffsetStatistics.Create(OffsetStatisticType.Maximum, station),
        Minimum = OffsetStatistics.Create(OffsetStatisticType.Minimum, station),
        Average = OffsetStatistics.Create(OffsetStatisticType.Average, station)
      };
    }

    /// <summary>
    /// Sets flag indicating whether the property should be serialized into the response.
    /// </summary>
    public void SetStatisticsReportFlags()
    {
      var reportElevation = false;
      var reportCutFill = false;
      var reportCmv = false;
      var reportMdp = false;
      var reportPassCount = false;
      var reportTemperature = false;

      foreach (var offset in Offsets)
      {
        if (!reportElevation && offset.ShouldSerializeElevation())
        {
          reportElevation = true;
        }

        if (!reportCutFill && offset.ShouldSerializeCutFill())
        {
          reportCutFill = true;
        }

        if (!reportCmv && offset.ShouldSerializeCMV())
        {
          reportCmv = true;
        }

        if (!reportMdp && offset.ShouldSerializeMDP())
        {
          reportMdp = true;
        }

        if (!reportPassCount && offset.ShouldSerializePassCount())
        {
          reportPassCount = true;
        }

        if (!reportTemperature && offset.ShouldSerializeTemperature())
        {
          reportTemperature = true;
        }
      }

      Minimum.SetReportFlags(reportElevation, reportCutFill, reportCmv, reportMdp, reportPassCount, reportTemperature);
      Maximum.SetReportFlags(reportElevation, reportCutFill, reportCmv, reportMdp, reportPassCount, reportTemperature);
      Average.SetReportFlags(reportElevation, reportCutFill, reportCmv, reportMdp, reportPassCount, reportTemperature);
    }
  }
}