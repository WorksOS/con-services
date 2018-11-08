using Newtonsoft.Json;

namespace VSS.TRex.CoordinateSystems.Models
{
  public struct NEE
  {
    [JsonProperty("northing")]
    public double North;

    [JsonProperty("easting")]
    public double East;
    
    [JsonProperty("elevation")]
    public double Elevation;
  }
}
