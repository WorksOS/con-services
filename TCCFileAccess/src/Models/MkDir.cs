using Newtonsoft.Json;

namespace VSS.TCCFileAccess.Models
{
  public class MkDir
  {
    [JsonProperty(PropertyName = "filespaceid", Required = Required.Always)]
    public string filespaceid;
    [JsonProperty(PropertyName = "path", Required = Required.Always)]
    public string path;
    [JsonProperty(PropertyName = "force", Required = Required.Always)]
    public bool force;
  }
}