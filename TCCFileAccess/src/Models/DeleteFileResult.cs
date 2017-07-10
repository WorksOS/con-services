using Newtonsoft.Json;

namespace VSS.Productivity3D.TCCFileAccess.Models
{
  public class DeleteFileResult : ApiResult
  {
    [JsonProperty(PropertyName = "path", Required = Required.Default)]
    public string path;
  }
}
