using Newtonsoft.Json;
using VSS.Productivity3D.WebApi.Models.Common;

namespace VSS.Productivity3D.WebApi.Models.Compaction.Models.Reports
{
  public abstract class ReportRowBase
  {
    /// <summary>
    /// Northing value
    /// </summary>
    [JsonProperty(Required = Required.Default)]
    public double Northing { get; protected set; }

    /// <summary>
    /// Easting value
    /// </summary>
    [JsonProperty(Required = Required.Default)]
    public double Easting { get; protected set; }

    /// <summary>
    /// Elevation value
    /// </summary>
    [JsonProperty(Required = Required.Default)]
    public double Elevation { get; protected set; }

    /// <summary>
    /// CutFill value
    /// </summary>
    [JsonProperty(Required = Required.Default)]
    public double CutFill { get; protected set; }

    /// <summary>
    /// CMV value
    /// </summary>
    [JsonProperty(Required = Required.Default)]
    public short CMV { get; protected set; }

    /// <summary>
    /// MDP value
    /// </summary>
    [JsonProperty(Required = Required.Default)]
    public short MDP { get; protected set; }

    /// <summary>
    /// Pass Count value
    /// </summary>
    [JsonProperty(Required = Required.Default)]
    public int PassCount { get; protected set; }

    /// <summary>
    /// Temperature value
    /// </summary>
    [JsonProperty(Required = Required.Default)]
    public double Temperature { get; protected set; }

    /// <summary>
    /// Indicates whether Elevation values are included in the report.
    /// </summary>
    [JsonIgnore]
    public bool ElevationReport { get; protected set; }

    /// <summary>
    /// Indicates whether Cut/Fill values are included in the report.
    /// </summary>
    [JsonIgnore] public bool CutFillReport { get; protected set; }

    /// <summary>
    /// Indicates whether CMV values are included in the report.
    /// </summary>
    [JsonIgnore]
    public bool CMVReport { get; protected set; }

    /// <summary>
    /// Indicates whether MDP values are included in the report.
    /// </summary>
    [JsonIgnore]
    public bool MDPReport { get; protected set; }

    /// <summary>
    /// Indicates wehther Pass Count values are included in the report.
    /// </summary>
    [JsonIgnore]
    public bool PassCountReport { get; protected set; }

    /// <summary>
    /// Indicates whether Temperature values are included in the report.
    /// </summary>
    [JsonIgnore]
    public bool TemperatureReport { get; protected set; }

    public bool ShouldSerializeElevation()
    {
      return ElevationReport;
    }
    public bool ShouldSerializeCutFill()
    {
      return CutFillReport;
    }
    public bool ShouldSerializeCMV()
    {
      return CMVReport && CMV != VelociraptorConstants.NO_CCV;
    }
    public bool ShouldSerializeMDP()
    {
      return MDPReport && MDP != VelociraptorConstants.NO_MDP;
    }
    public bool ShouldSerializePassCount()
    {
      return PassCountReport;
    }
    public bool ShouldSerializeTemperature()
    {
      return TemperatureReport && Temperature != VelociraptorConstants.NO_TEMPERATURE;
    }

    /// <summary>
    /// Sets flags that indicates which of the reported values present in the report.
    /// </summary> 
    public void SetReportFlags(CompactionReportRequest request)
    {
      ElevationReport = request.ReportElevation;
      CutFillReport = request.ReportCutFill;
      CMVReport = request.ReportCMV;
      MDPReport = request.ReportMDP;
      PassCountReport = request.ReportPassCount;
      TemperatureReport = request.ReportTemperature;
    }
  }
}