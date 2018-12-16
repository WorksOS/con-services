using Newtonsoft.Json;

namespace VSS.Pegasus.Client.Models
{
  public class TileMetadata
  {
    [JsonProperty(PropertyName = "extents", Required = Required.Default)]
    public Extents Extents { get; set; }
    [JsonProperty(PropertyName = "start-zoom", Required = Required.Default)]
    public int MinZoom { get; set; }
    [JsonProperty(PropertyName = "end-zoom", Required = Required.Default)]
    public int MaxZoom { get; set; }
    [JsonProperty(PropertyName = "tile-count", Required = Required.Default)]
    public int TileCount { get; set; }

  }
}
