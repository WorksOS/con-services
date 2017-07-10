using Newtonsoft.Json;

namespace VSS.Productivity3D.TCCFileAccess.Models
{
  public class GetFileSpacesParams
  {
    [JsonProperty(PropertyName = "filter", Required = Required.Default)]
    public string filter;
  }
}
