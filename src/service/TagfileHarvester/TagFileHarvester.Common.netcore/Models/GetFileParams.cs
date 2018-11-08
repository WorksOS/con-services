using Newtonsoft.Json;

namespace TagFileHarvester.Models
{
  public class GetFileParams
  {
    [JsonProperty(PropertyName = "filespaceid", Required = Required.Always)]
    public string filespaceid;

    [JsonProperty(PropertyName = "path", Required = Required.Always)]
    public string path;
  }
}