using Newtonsoft.Json;

namespace TagFileHarvester.Models
{
  public class DelParams
  {
    [JsonProperty(PropertyName = "filespaceid", Required = Required.Always)]
    public string filespaceid;

    [JsonProperty(PropertyName = "path", Required = Required.Always)]
    public string path;

    [JsonProperty(PropertyName = "recursive", Required = Required.Always)]
    public string recursive;
  }
}