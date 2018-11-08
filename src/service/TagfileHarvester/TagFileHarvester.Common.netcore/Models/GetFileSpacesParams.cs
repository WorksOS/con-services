using Newtonsoft.Json;

namespace TagFileHarvester.Models
{
  public class GetFileSpacesParams
  {
    [JsonProperty(PropertyName = "filter", Required = Required.Default)]
    public string filter;
  }
}