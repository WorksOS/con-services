using Newtonsoft.Json;

namespace VSS.DataOcean.Client.Models
{
  public class DataOceanTransfer
  {
    [JsonProperty(PropertyName = "url", Required = Required.Default)]
    public string Url { get; set; }
  }
}
