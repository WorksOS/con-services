using System;
using Newtonsoft.Json;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.Models
{
  public class LiftDetails
  {
    [JsonProperty(PropertyName = "designId")]
    public long DesignId { get; set; }

    [JsonProperty(PropertyName = "endUtc")]
    public DateTime EndUtc { get; set; }

    [JsonProperty(PropertyName = "layerId")]
    public long LayerId { get; set; }
  }
}
