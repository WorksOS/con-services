using ASNodeRaptorReports;
using Newtonsoft.Json;

namespace VSS.Productivity3D.WebApi.Models.Compaction.Models.Reports
{
  public class OffsetStatistics
  {
    [JsonProperty(PropertyName = "elevation")]
    public double? Elevation { get; protected set; }

    [JsonProperty(PropertyName = "cutfill")]
    public double? CutFill { get; protected set; }

    [JsonProperty(PropertyName = "cmv")]
    public double? CMV { get; protected set; }

    [JsonProperty(PropertyName = "mdp")]
    public double? MDP { get; protected set; }

    [JsonProperty(PropertyName = "passcount")]
    public double? PassCount { get; protected set; }

    [JsonProperty(PropertyName = "temperature")]
    public double? Temperature { get; protected set; }

    public bool ShouldSerializeElevation() => reportElevation;
    public bool ShouldSerializeCutFill() => reportCutFill;
    public bool ShouldSerializeCMV() => reportCmv;
    public bool ShouldSerializeMDP() => reportMdp;
    public bool ShouldSerializePassCount() => reportPassCount;
    public bool ShouldSerializeTemperature() => reportTemperature;

    private bool reportElevation;
    private bool reportCutFill;
    private bool reportCmv;
    private bool reportMdp;
    private bool reportPassCount;
    private bool reportTemperature;

    public static OffsetStatistics Create(OffsetStatisticType type, TStation station)
    {
      switch (type)
      {
        case OffsetStatisticType.Maximum:
          {
            return CreateStatisticsForMaximums(station);
          }
        case OffsetStatisticType.Minimum:
          {
            return CreateStatisticsForMinimums(station);
          }
        case OffsetStatisticType.Average:
          {
            return CreateStatisticsForAverages(station);
          }
      }

      return null;
    }

    private static OffsetStatistics CreateStatisticsForMaximums(TStation station)
    {
      var offsetStatistics = new OffsetStatistics
      {
        Elevation = station.ElevMax,
        CutFill = station.CutFillMax,
        PassCount = station.PassCountMax,
        CMV = station.CMVMax,
        Temperature = station.TemperatureMax,
        MDP = station.MDPMax
      };

      return offsetStatistics;
    }

    private static OffsetStatistics CreateStatisticsForMinimums(TStation station)
    {
      var offsetStatistics = new OffsetStatistics
      {
        Elevation = station.ElevMin,
        CutFill = station.CutFillMin,
        PassCount = station.PassCountMin,
        CMV = station.CMVMin,
        Temperature = station.TemperatureMin,
        MDP = station.MDPMin
      };

      return offsetStatistics;
    }

    private static OffsetStatistics CreateStatisticsForAverages(TStation station)
    {
      var offsetStatistics = new OffsetStatistics
      {
        Elevation = station.ElevAvg,
        CutFill = station.CutFillAvg,
        PassCount = station.PassCountAvg,
        CMV = station.CMVAvg,
        Temperature = station.TemperatureAvg,
        MDP = station.MDPAvg
      };

      return offsetStatistics;
    }

    public void SetReportFlags(bool elevation, bool cutFill, bool cmv, bool mdp, bool passCount, bool temperature)
    {
      reportElevation = elevation;
      reportCutFill = cutFill;
      reportCmv = cmv;
      reportMdp = mdp;
      reportPassCount = passCount;
      reportTemperature = temperature;
    }
  }
}