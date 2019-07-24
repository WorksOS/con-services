using Newtonsoft.Json;
using VSS.TRex.Common.Types;

namespace VSS.TRex.ConnectedSite.Gateway.Abstractions
{
  public interface IConnectedSiteMessage
  {
    [JsonProperty("lat", NullValueHandling = NullValueHandling.Ignore)]
    double? Latitude { get; set; }
    [JsonProperty("lon", NullValueHandling = NullValueHandling.Ignore)]
    double? Longitude { get; set; }
    [JsonIgnore]
    string HardwareID { get; set; }
    [JsonProperty("h", NullValueHandling = NullValueHandling.Ignore)]
    double? Height { get; set; }
    [JsonIgnore]
    string Route { get; }
    [JsonIgnore]
    MachineControlPlatformType PlatformType { get; set; }
  }
}
