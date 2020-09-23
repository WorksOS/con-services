using Newtonsoft.Json;
using VSS.Productivity3D.Productivity3D.Models.Compaction.ResultHandling;

namespace CCSS.WorksOS.Reports.Common.Models
{
  public class PassCountDetails : SummaryDataBase
  {
    public PassCountDetailsData PassCountDetailsData { get; set; }
    [JsonIgnore]
    public bool IsEmpty => PassCountDetailsData == null;
  }
  public class PassCountDetailsData
  {
    /// <summary>
    /// Collection of passcount percentages where each element represents the percentage of the matching index passcount number provided in the 
    /// passCounts member of the pass count request representation.
    /// </summary>
    [JsonProperty(PropertyName = "percents")]
    public double[] Percents { get; set; }
    /// <summary>
    /// Gets the total coverage area for the production data - not the total area specified in filter
    /// </summary>
    /// <value>
    /// The total coverage area in sq meters.
    /// </value>
    [JsonProperty(PropertyName = "totalCoverageArea")]
    public double TotalCoverageArea { get; set; }
    /// <summary>
    /// Pass count machine target and whether it is constant or varies.
    /// </summary>
    [JsonProperty(PropertyName = "passCountTarget")]
    public PassCountTargetData PassCountTarget { get; set; }

  }

}
