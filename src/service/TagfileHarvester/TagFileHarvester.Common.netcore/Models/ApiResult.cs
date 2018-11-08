using Newtonsoft.Json;

namespace TagFileHarvester.Models
{
  public class ApiResult
  {
    [JsonProperty(PropertyName = "errorid", Required = Required.Default)]
    public string errorid;

    [JsonProperty(PropertyName = "message", Required = Required.Default)]
    public string message;

    [JsonProperty(PropertyName = "success", Required = Required.Default)]
    public bool success;
  }
}