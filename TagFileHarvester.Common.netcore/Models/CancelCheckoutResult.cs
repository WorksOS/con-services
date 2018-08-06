using Newtonsoft.Json;

namespace TagFileHarvester.Models
{
  public class CancelCheckoutResult : ApiResult
  {
    [JsonProperty(PropertyName = "path ", Required = Required.Default)]
    public string path;
  }
}
