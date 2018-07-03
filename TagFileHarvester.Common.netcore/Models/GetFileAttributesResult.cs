using Newtonsoft.Json;

namespace TagFileHarvester.Models
{
  public class GetFileAttributesResult : ApiResult
  {
    [JsonProperty(PropertyName = "attrHidden", Required = Required.Default)]
    public bool attrHidden;

    [JsonProperty(PropertyName = "entryName", Required = Required.Default)]
    public string entryName;
  }
}