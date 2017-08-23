using Newtonsoft.Json;

namespace VSS.Productivity3D.TagFileHarvester.Models
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