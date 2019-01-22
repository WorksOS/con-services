using System;
using Newtonsoft.Json;
using VSS.Productivity3D.Models.Models.Reports;
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
    public double CMV { get; protected set; }

    /// <summary>
    /// MDP value
    /// </summary>
    [JsonProperty(Required = Required.Default)]
    public double MDP { get; protected set; }

    /// <summary>
    /// Pass Count value
    /// </summary>
    [JsonProperty(Required = Required.Default)]
    public double PassCount { get; protected set; }//double needed for pass count average for station & offset

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

    public virtual bool ShouldSerializeNorthing()
    {
      return true;
    }
    public virtual bool ShouldSerializeEasting()
    {
      return true;
    }
    public bool ShouldSerializeElevation() => ElevationReport && Math.Abs(Elevation - VelociraptorConstants.NULL_SINGLE) > 0.001;
    public bool ShouldSerializeCutFill() => CutFillReport && Math.Abs(CutFill - VelociraptorConstants.NULL_SINGLE) > 0.001;
    public bool ShouldSerializeCMV() => CMVReport && CMV != VelociraptorConstants.NO_CCV;
    public bool ShouldSerializeMDP() => MDPReport && MDP != VelociraptorConstants.NO_MDP;
    public bool ShouldSerializePassCount() => PassCountReport && PassCount != VelociraptorConstants.NO_PASSCOUNT;
    public bool ShouldSerializeTemperature() => TemperatureReport && Temperature != VelociraptorConstants.NO_TEMPERATURE;

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

    /// <summary>
    /// Converts the raw values from Raptor to values to return.
    /// </summary>
    public void SetValues(double northing, double easting, double elevation, double cutFill, double cmv, double mdp, double passCount, double temperature)
    {
      Northing = northing;
      Easting = easting;
      Elevation = elevation != VelociraptorConstants.NULL_SINGLE ? elevation : VelociraptorConstants.NULL_SINGLE;
      CutFill = cutFill != VelociraptorConstants.NULL_SINGLE ? cutFill : VelociraptorConstants.NULL_SINGLE;
      CMV = cmv != VelociraptorConstants.NO_CCV ? (double)cmv / 10 : VelociraptorConstants.NO_CCV;
      MDP = mdp != VelociraptorConstants.NO_MDP ? (double)mdp / 10 : VelociraptorConstants.NO_MDP;
      PassCount = passCount != VelociraptorConstants.NO_PASSCOUNT ? passCount : VelociraptorConstants.NO_PASSCOUNT;
      Temperature = temperature != VelociraptorConstants.NO_TEMPERATURE
        ? (double)temperature / 10
        : VelociraptorConstants.NO_TEMPERATURE;
    }
  }
}
