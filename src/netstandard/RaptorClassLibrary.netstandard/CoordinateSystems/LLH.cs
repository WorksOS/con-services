using Newtonsoft.Json;

namespace VSS.TRex.CoordinateSystems
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
