using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace VSS.Productivity3D.TagFileHarvester.Models
{
  public class LoginParams
  {
    [JsonProperty(PropertyName = "username", Required = Required.Always)]
    public string username;
    [JsonProperty(PropertyName = "orgname ", Required = Required.Always)]
    public string orgname;
    [JsonProperty(PropertyName = "password", Required = Required.Always)]
    public string password;
    [JsonProperty(PropertyName = "mode ", Required = Required.Default)]
    public string mode;
    [JsonProperty(PropertyName = "forcegmt  ", Required = Required.Default)]
    public bool forcegmt;
  }
}
