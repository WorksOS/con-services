using Newtonsoft.Json;

namespace VSS.TCCFileAccess.Models
{
  public class DirParams
  {
    [JsonProperty(PropertyName = "filespaceid", Required = Required.Always)]
    public string filespaceid;
    [JsonProperty(PropertyName = "path", Required = Required.Always)]
    public string path;
    [JsonProperty(PropertyName = "recursive", Required = Required.Default)]
    public bool recursive;
    [JsonProperty(PropertyName = "filterfolders", Required = Required.Default)]
    public bool filterfolders;
    [JsonProperty(PropertyName = "filemasks", Required = Required.Default)]
    public string filemasks;
  }
}
