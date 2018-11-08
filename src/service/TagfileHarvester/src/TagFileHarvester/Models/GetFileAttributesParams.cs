using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace VSS.Productivity3D.TagFileHarvester.Models
{
  public class GetFileAttributesParams
  {
    [JsonProperty(PropertyName = "filespaceid", Required = Required.Default)]
    public string filespaceid;
    [JsonProperty(PropertyName = "path", Required = Required.Default)]
    public string path;

  }
}
