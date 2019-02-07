using System.Collections.Generic;
using Newtonsoft.Json;
using VSS.DataOcean.Client.Models;

namespace VSS.DataOcean.Client.ResultHandling
{
  public class BrowseDirectoriesResult
  {
    [JsonProperty(PropertyName = "directories", Required = Required.Default)]
    public List<DataOceanDirectory> Directories { get; set; }
  }
}
