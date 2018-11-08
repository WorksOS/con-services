using Newtonsoft.Json;

namespace VSS.TCCFileAccess.Models
{
  public class LastDirChangeParams
  {
    [JsonProperty(PropertyName = "filespaceid", Required = Required.Always)]
    public string filespaceid;
    [JsonProperty(PropertyName = "path", Required = Required.Always)]
    public string path;
    [JsonProperty(PropertyName = "recursive", Required = Required.Always)]
    public bool recursive;

  }
}
