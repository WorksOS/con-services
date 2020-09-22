using System.Collections.Generic;
using Newtonsoft.Json;

namespace CCSS.WorksOS.Reports.Common.Models
{
  public class StationOffsetReportRow
  {
    [JsonProperty(PropertyName = "station")]
    public double? Station { get; protected set; }

    [JsonProperty(PropertyName = "offsets")]
    public IEnumerable<OffsetRow> Offsets;

    [JsonProperty(PropertyName = "minimum")]
    public ThreeDReportRow Minimums;

    [JsonProperty(PropertyName = "maximum")]
    public ThreeDReportRow Maximums;

    [JsonProperty(PropertyName = "average")]
    public ThreeDReportRow Averages;
  }
}
