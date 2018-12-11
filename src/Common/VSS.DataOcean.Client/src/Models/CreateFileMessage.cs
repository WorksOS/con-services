using Newtonsoft.Json;

namespace VSS.DataOcean.Client.Models
{
  public class CreateFileMessage
  {
    [JsonProperty(PropertyName = "file", Required = Required.Default)]
    public DataOceanFile File { get; set; }
  }
}
