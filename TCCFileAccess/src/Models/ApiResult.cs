using Newtonsoft.Json;

namespace VSS.TCCFileAccess.Models
{
  public class ApiResult
  {
    [JsonProperty(PropertyName = "success", Required = Required.Default)]
    public bool success;
    [JsonProperty(PropertyName = "message", Required = Required.Default)]
    public string message;
    [JsonProperty(PropertyName = "errorid", Required = Required.Default)]
    public string errorid;
  }
}
