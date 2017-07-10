using Newtonsoft.Json;

namespace VSS.Productivity3D.TCCFileAccess.Models
{
  public class GetFileSpacesResult : ApiResult
  {
    [JsonProperty(PropertyName = "filespaces", Required = Required.Default)] 
    public FileSpace[] filespaces;
  }
}
