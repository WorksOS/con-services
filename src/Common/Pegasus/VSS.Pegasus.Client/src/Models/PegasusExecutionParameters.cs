using System;
using Newtonsoft.Json;

namespace VSS.Pegasus.Client.Models
{
  public class PegasusExecutionParameters
  {
    [JsonProperty(PropertyName = "parent_id", Required = Required.Default)]
    public Guid? ParentId { get; set; }
    [JsonProperty(PropertyName = "name", Required = Required.Default)]
    public string Name { get; set; }
    [JsonProperty(PropertyName = "tile_order", Required = Required.Default)]
    public string TileOrder { get; set; }
    [JsonProperty(PropertyName = "multifile", Required = Required.Default)]
    public string MultiFile { get; set; }
    [JsonProperty(PropertyName = "public", Required = Required.Default)]
    public string Public { get; set; }
  }
}
