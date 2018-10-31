using Newtonsoft.Json;

namespace VSS.TRex.CoordinateSystems.Models
{
  public struct LLH
  {
    [JsonProperty("latitude")]
    public double Latitude;

    [JsonProperty("longitude")]
    public double Longitude;

    [JsonProperty("height")]
    public double Height;
  }
}
