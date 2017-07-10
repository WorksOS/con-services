using Newtonsoft.Json;

namespace VSS.Productivity3D.TCCFileAccess.Models
{
  public class GetFileAttributesResult : ApiResult
  {
    [JsonProperty(PropertyName = "entryName", Required = Required.Default)]
    public string entryName;
    [JsonProperty(PropertyName = "attrHidden", Required = Required.Default)]
    public bool attrHidden;
  }
}
