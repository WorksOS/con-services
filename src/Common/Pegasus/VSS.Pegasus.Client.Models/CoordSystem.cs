using Newtonsoft.Json;

namespace VSS.Pegasus.Client.Models
{
  public class CoordSystem
  {
    [JsonProperty(PropertyName = "type", Required = Required.Default)]
    public string Type { get; set; }
    [JsonProperty(PropertyName = "value", Required = Required.Default)]
    public string Value { get; set; }

  }
}
