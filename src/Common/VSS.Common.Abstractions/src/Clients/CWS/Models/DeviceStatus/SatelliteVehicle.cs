using Newtonsoft.Json;

namespace VSS.Common.Abstractions.Clients.CWS.Models.DeviceStatus
{
  public class SatelliteVehicle
  {
    [JsonProperty("gps")]
    public long? Gps { get; set; }
    [JsonProperty("gln")]
    public long? Gln { get; set; }
    [JsonProperty("bds")]
    public long? Bds { get; set; }
    [JsonProperty("gal")]
    public long? Gal { get; set; }
    [JsonProperty("irnss")]
    public long? Irnss { get; set; }

    public SatelliteVehicle() { }
  }
}
