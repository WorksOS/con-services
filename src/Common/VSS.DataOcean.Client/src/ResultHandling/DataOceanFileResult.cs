using Newtonsoft.Json;
using VSS.DataOcean.Client.Models;

namespace VSS.DataOcean.Client.ResultHandling
{
  public class DataOceanFileResult
  {
    [JsonProperty(PropertyName = "file", Required = Required.Default)]
    public DataOceanFile File { get; set; }
  }
}
