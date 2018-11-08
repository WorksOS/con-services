using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace VSS.TRex.ConnectedSite.Gateway.WebApi.Abstractions
{
  public interface IConnectedSiteMessage
  {
    [JsonProperty("ts")]
    DateTime? Timestamp { get; set; }
    [JsonProperty("lat")]
    double? Lattitude { get; set; }
    [JsonProperty("lon")]
    double? Longitude { get; set; }
    [JsonProperty("h")]
    double? Height { get; set; }
    [JsonIgnore]
    string Route { get; }
  }
}
