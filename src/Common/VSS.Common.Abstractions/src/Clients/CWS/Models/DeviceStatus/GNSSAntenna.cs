using Newtonsoft.Json;

namespace VSS.Common.Abstractions.Clients.CWS.Models.DeviceStatus
{
  public class GNSSAntenna
  {
    [JsonProperty("antennaLocation")]
    public string AntennaLocation { get; set; }
    [JsonProperty("antennaSerialNumber")]
    public string AntennaSerialNumber { get; set; }
    [JsonProperty("svsUsed")] 
    public SatelliteVehicle SvsUsed { get; set; }

    public GNSSAntenna()
    {
    }
  }
}

