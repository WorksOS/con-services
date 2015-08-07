using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TagFileHarvester.Models
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
