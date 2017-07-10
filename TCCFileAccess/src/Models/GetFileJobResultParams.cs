using Newtonsoft.Json;

namespace VSS.Productivity3D.TCCFileAccess.Models
{
  public class GetFileJobResultParams
  {
    [JsonProperty(PropertyName = "fileid", Required = Required.Always)]
    public string fileid;
  }
}
