using System.Collections.Generic;
using Newtonsoft.Json;

namespace VSS.Common.Abstractions.Clients.CWS.Models
{
  public class ProjectBoundary
  {
    /// <summary>
    /// e.g. Polygon
    /// </summary>
    [JsonProperty("type")]
    public string type { get; set; } = "POLYGON";

    /// <summary>
    /// long, lat
    /// </summary>
    [JsonProperty("coordinates")]
    public List<double[,]> coordinates { get; set; }

  }
}
