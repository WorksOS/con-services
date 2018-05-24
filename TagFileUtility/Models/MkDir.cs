using Newtonsoft.Json;

namespace TagFileHarvester.Models
{
  public class MkDir
  {
    [JsonProperty(PropertyName = "filespaceid", Required = Required.Always)]
    public string filespaceid;
    [JsonProperty(PropertyName = "path", Required = Required.Always)]
    public string path;
    [JsonProperty(PropertyName = "newfilespaceid", Required = Required.Always)]
    public bool force;
  }
}