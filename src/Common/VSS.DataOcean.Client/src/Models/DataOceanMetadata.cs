using Newtonsoft.Json;
namespace VSS.DataOcean.Client.Models
{
  public class DataOceanMetadata
  {
    [JsonProperty(PropertyName = "info", Required = Required.Default)]
    public string Info { get; set; }
  }
}
