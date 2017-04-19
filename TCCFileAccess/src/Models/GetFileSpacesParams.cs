using Newtonsoft.Json;

namespace TCCFileAccess.Models
{
  public class GetFileSpacesParams
  {
    [JsonProperty(PropertyName = "filter", Required = Required.Default)]
    public string filter;
  }
}
