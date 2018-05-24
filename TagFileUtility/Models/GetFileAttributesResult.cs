using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TagFileHarvester.Models
{
  public class GetFileAttributesResult : ApiResult
  {
    [JsonProperty(PropertyName = "entryName", Required = Required.Default)]
    public string entryName;
    [JsonProperty(PropertyName = "attrHidden", Required = Required.Default)]
    public bool attrHidden;
  }
}
