using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace VSS.DataOcean.Client.Models
{
  public class DataOceanFile
  {
    [JsonProperty(PropertyName = "id", Required = Required.Default)]
    public Guid Id { get; set; }
    [JsonProperty(PropertyName = "name", Required = Required.Always)]
    public string Name { get; set; }
    [JsonProperty(PropertyName = "status", Required = Required.Default)]
    public string Status { get; set; }
    [JsonProperty(PropertyName = "parent_id", Required = Required.Default)]
    public Guid? ParentId { get; set; }
    [JsonProperty(PropertyName = "multifile", Required = Required.Default)]
    public bool Multifile { get; set; }
    [JsonProperty(PropertyName = "region_preferences", Required = Required.Default)]
    public List<string> RegionPreferences { get; set; }
    [JsonProperty(PropertyName = "metadata", Required = Required.Default)]
    public DataOceanMetadata Metadata { get; set; }
    [JsonProperty(PropertyName = "download", Required = Required.Default)]
    public DataOceanTransfer DataOceanDownload { get; set; }
    [JsonProperty(PropertyName = "upload", Required = Required.Default)]
    public DataOceanTransfer DataOceanUpload { get; set; }

  }
}
