using Newtonsoft.Json;

namespace TagFileHarvester.Models
{
  public class LoginParams
  {
    [JsonProperty(PropertyName = "forcegmt  ", Required = Required.Default)]
    public bool forcegmt;

    [JsonProperty(PropertyName = "mode ", Required = Required.Default)]
    public string mode;

    [JsonProperty(PropertyName = "orgname ", Required = Required.Always)]
    public string orgname;

    [JsonProperty(PropertyName = "password", Required = Required.Always)]
    public string password;

    [JsonProperty(PropertyName = "username", Required = Required.Always)]
    public string username;
  }
}