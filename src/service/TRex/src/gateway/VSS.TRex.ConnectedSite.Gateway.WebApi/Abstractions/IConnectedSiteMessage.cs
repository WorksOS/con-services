using Newtonsoft.Json;

namespace VSS.TRex.ConnectedSite.Gateway.WebApi.Abstractions
{
  public interface IConnectedSiteMessage
  {
    [JsonProperty("lat")]
    double? Lattitude { get; set; }
    [JsonProperty("lon")]
    double? Longitude { get; set; }
    [JsonIgnore]
    string HardwareID { get; set; }
    [JsonProperty("h")]
    double? Height { get; set; }
    [JsonIgnore]
    string Route { get; }
  }
}
