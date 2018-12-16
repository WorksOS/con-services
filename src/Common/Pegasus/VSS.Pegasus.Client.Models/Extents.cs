using Newtonsoft.Json;

namespace VSS.Pegasus.Client.Models
{
  public class Extents
  {
    [JsonProperty(PropertyName = "north", Required = Required.Default)]
    public double North { get; set; }
    [JsonProperty(PropertyName = "south", Required = Required.Default)]
    public double South { get; set; }
    [JsonProperty(PropertyName = "east", Required = Required.Default)]
    public double East { get; set; }
    [JsonProperty(PropertyName = "west", Required = Required.Default)]
    public double West { get; set; }
    [JsonProperty(PropertyName = "coord_system", Required = Required.Default)]
    public CoordSystem CoordSystem { get; set; }
  }
}
