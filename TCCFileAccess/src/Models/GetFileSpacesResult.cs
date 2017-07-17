using Newtonsoft.Json;

namespace VSS.TCCFileAccess.Models
{
  public class GetFileSpacesResult : ApiResult
  {
    [JsonProperty(PropertyName = "filespaces", Required = Required.Default)] 
    public FileSpace[] filespaces;
  }
}
