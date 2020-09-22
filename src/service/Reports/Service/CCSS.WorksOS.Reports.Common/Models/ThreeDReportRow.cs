using System.Collections.Generic;
using Newtonsoft.Json;

namespace CCSS.WorksOS.Reports.Common.Models
{
  public class ThreeDReportRow
  {
    /// <summary>
    /// Elevation value
    /// </summary>
    [JsonProperty(PropertyName = "elevation")]
    public double? Elevation { get; protected set; }

    /// <summary>
    /// CutFill value
    /// </summary>
    [JsonProperty(PropertyName = "cutfill")]
    public double? CutFill { get; protected set; }

    /// <summary>
    /// CMV value
    /// </summary>
    [JsonProperty(PropertyName = "cmv")]
    public double? CMV { get; protected set; }

    /// <summary>
    /// MDP value
    /// </summary>
    [JsonProperty(PropertyName = "mdp")]
    public double? MDP { get; protected set; }

    /// <summary>
    /// Pass Count value
    /// Note: double instead of int for station & offset averages
    /// </summary>
    [JsonProperty(PropertyName = "passcount")]
    public double? PassCount { get; protected set; }

    /// <summary>
    /// Temperature value
    /// </summary>
    [JsonProperty(PropertyName = "temperature")]
    public double? Temperature { get; protected set; }

    /// <summary>
    /// Returns the values as a dictionary so we can use iteration in the content generators.
    /// </summary>
    public IDictionary<string, double?> Values
    {
      get
      {
        var values = new Dictionary<string, double?>();
        foreach (var key in SummaryReportConstants.Keys)
        {
          switch (key)
          {
            case SummaryReportConstants.ReportElevationParameter:
              values.Add(key, Elevation);
              break;
            case SummaryReportConstants.ReportCutFillParameter:
              values.Add(key, CutFill);
              break;
            case SummaryReportConstants.ReportCmvParameter:
              values.Add(key, CMV);
              break;
            case SummaryReportConstants.ReportMdpParameter:
              values.Add(key, MDP);
              break;
            case SummaryReportConstants.ReportPassCountParameter:
              values.Add(key, PassCount);
              break;
            case SummaryReportConstants.ReportTemperatureParameter:
              values.Add(key, Temperature);
              break;
          }
        }
        return values;
      }
    }
  }
}
