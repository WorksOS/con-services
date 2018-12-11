using Newtonsoft.Json;

namespace VSS.DataOcean.Client.Models
{
  public class CreateDirectoryMessage
  {
    [JsonProperty(PropertyName = "directory", Required = Required.Default)]
    public DataOceanDirectory Directory { get; set; }
  }
}
