using Newtonsoft.Json;

namespace VSS.Productivity3D.TCCFileAccess.Models
{
  public class CreateFileJobParams
  {
    [JsonProperty(PropertyName = "filespaceid", Required = Required.Always)]
    public string filespaceid;
    [JsonProperty(PropertyName = "path", Required = Required.Always)]
    public string path;
    [JsonProperty(PropertyName = "type", Required = Required.Always)]
    public string type;
    [JsonProperty(PropertyName = "forcerender", Required = Required.Default)]
    public bool forcerender;
  }
}
