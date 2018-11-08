using Newtonsoft.Json;

namespace TagFileHarvester.Models
{
  public class GetFileAttributesParams
  {
    [JsonProperty(PropertyName = "filespaceid", Required = Required.Default)]
    public string filespaceid;

    [JsonProperty(PropertyName = "path", Required = Required.Default)]
    public string path;
  }
}