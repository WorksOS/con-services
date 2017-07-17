using Newtonsoft.Json;

namespace VSS.TCCFileAccess.Models
{
  public class GetFileJobResultParams
  {
    [JsonProperty(PropertyName = "fileid", Required = Required.Always)]
    public string fileid;
  }
}
