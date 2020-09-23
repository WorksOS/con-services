using Newtonsoft.Json;
using VSS.Productivity3D.Productivity3D.Models.Compaction;

namespace CCSS.WorksOS.Reports.Common.Models
{
  public class CMVChange : SummaryDataBase
  {
    public CMVPercentageData CmvChangeData { get; set; }

    [JsonIgnore]
    public bool IsEmpty => CmvChangeData == null;
  }

  public class CMVPercentageData
  {
    /// <summary>
    /// The CMV percentage values
    /// </summary>
    [JsonProperty(PropertyName = "percents")]
    public double[] Percents { get; set; }

    /// <summary>
    /// The total area covered by non-null cells in the request area
    /// </summary>
    [JsonProperty(PropertyName = "totalAreaCoveredSqMeters")]
    public double TotalAreaCoveredSqMeters { get; set; }
  }

  public class CMVDetails : SummaryDataBase
  {
    public CmvDetailsData CmvDetailsData { get; set; }

    //CMV details, unlike all the others, doesn't have a nested data model from 3dpm
    [JsonIgnore]
    public bool IsEmpty => CmvDetailsData?.Percents == null;
  }

  public class CmvDetailsData
  {
    /// <summary>
    /// The CMV percentage values
    /// </summary>
    [JsonProperty(PropertyName = "percents")]
    public double[] Percents { get; set; }

    /// <summary>
    /// CMV machine target and whether it is constant or varies.
    /// </summary>
    [JsonProperty(PropertyName = "cmvTarget")]
    public CmvTargetData CmvTarget { get; set; }
    /// <summary>
    /// The minimum percentage the measured CMV may be compared to the cmvTarget from the machine
    /// </summary>
    [JsonProperty(PropertyName = "minCMVPercent", Required = Required.Default)]
    public double MinCMVPercent { get; set; }
    /// <summary>
    /// The maximum percentage the measured CMV may be compared to the cmvTarget from the machine
    /// </summary>
    [JsonProperty(PropertyName = "maxCMVPercent")]
    public double MaxCMVPercent { get; set; }
  }
}
