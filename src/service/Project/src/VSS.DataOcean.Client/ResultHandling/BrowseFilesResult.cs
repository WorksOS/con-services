using System.Collections.Generic;
using Newtonsoft.Json;
using VSS.DataOcean.Client.Models;

namespace VSS.DataOcean.Client.ResultHandling
{
  public class BrowseFilesResult
  {
    [JsonProperty(PropertyName = "files", Required = Required.Default)]
    public List<DataOceanFile> Files { get; set; }

  }
}
