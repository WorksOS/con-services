using Newtonsoft.Json;

namespace CCSS.WorksOS.Reports.Common.Models
{
  public class GridReportRow : ThreeDReportRow
  {
    /// <summary>
    /// Northing value
    /// </summary>
    [JsonProperty(PropertyName = "northing")]
    public double? Northing { get; protected set; }

    /// <summary>
    /// Easting value
    /// </summary>
    [JsonProperty(PropertyName = "easting")]
    public double? Easting { get; protected set; }
  }
}
