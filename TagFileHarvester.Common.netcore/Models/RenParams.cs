using Newtonsoft.Json;

namespace TagFileHarvester.Models
{
  public class RenParams
  {
    [JsonProperty(PropertyName = "filespaceid", Required = Required.Always)]
    public string filespaceid;

    [JsonProperty(PropertyName = "merge", Required = Required.Default)]
    public bool merge;

    [JsonProperty(PropertyName = "newfilespaceid", Required = Required.Always)]
    public string newfilespaceid;

    [JsonProperty(PropertyName = "newPath", Required = Required.Always)]
    public string newPath;

    [JsonProperty(PropertyName = "path", Required = Required.Always)]
    public string path;

    [JsonProperty(PropertyName = "replace", Required = Required.Default)]
    public bool replace;
  }
}