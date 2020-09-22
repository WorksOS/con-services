using Newtonsoft.Json;

namespace CCSS.WorksOS.Reports.Common.Models
{
  public class OffsetRow : ThreeDReportRow
  {
    [JsonProperty(PropertyName = "offset")]
    public double? Position { get; protected set; }

    [JsonProperty(PropertyName = "northing")]
    public double? Northing { get; protected set; }

    [JsonProperty(PropertyName = "easting")]
    public double? Easting { get; protected set; }
  }
}
