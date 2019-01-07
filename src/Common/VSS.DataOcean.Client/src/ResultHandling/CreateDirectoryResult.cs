using Newtonsoft.Json;
using VSS.DataOcean.Client.Models;

namespace VSS.DataOcean.Client.ResultHandling
{
  public class CreateDirectoryResult
  {
    [JsonProperty(PropertyName = "directory", Required = Required.Default)]
    public DataOceanDirectory Directory { get; set; }
  }
}
