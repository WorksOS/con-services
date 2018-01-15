using Newtonsoft.Json;

namespace VSS.TCCFileAccess.Models
{
  public class CopyParams
  {
    [JsonProperty(PropertyName = "filespaceid", Required = Required.Always)]
    public string filespaceid;
    [JsonProperty(PropertyName = "path", Required = Required.Always)]
    public string path;
    [JsonProperty(PropertyName = "newfilespaceid", Required = Required.Always)]
    public string newfilespaceid;
    [JsonProperty(PropertyName = "newPath", Required = Required.Always)]
    public string newPath;
    [JsonProperty(PropertyName = "replace", Required = Required.Default)]
    public bool replace;
    [JsonProperty(PropertyName = "merge", Required = Required.Default)]
    public bool merge;
  }
}
