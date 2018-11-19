using System;
using Newtonsoft.Json;

namespace VSS.DataOcean.Client.Models
{
  public class DataOceanDirectory
  {
    [JsonProperty(PropertyName = "id", Required = Required.Always)]
    public Guid Id { get; set; }
    [JsonProperty(PropertyName = "name", Required = Required.Always)]
    public string Name { get; set; }
    [JsonProperty(PropertyName = "parent_id", Required = Required.Default)]
    public Guid? ParentId { get; set; }
    [JsonProperty(PropertyName = "metadata", Required = Required.Default)]
    public DataOceanMetadata Metadata { get; set; }
  }
}
