using System;
using Newtonsoft.Json;

namespace VSS.Pegasus.Client.Models
{
  public class PegasusBoundParameters
  {
    [JsonProperty(PropertyName = "dataocean_write:new_file_id", Required = Required.Default)]
    public Guid DataOceanWriteNewFileId { get; set; }
  }
}
