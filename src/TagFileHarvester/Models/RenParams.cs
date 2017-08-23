using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace VSS.Productivity3D.TagFileHarvester.Models
{
  public class RenParams
  {
    [JsonProperty(PropertyName = "filespaceid", Required = Required.Always)]
    public string filespaceid;
    [JsonProperty(PropertyName = "path", Required = Required.Always)]
    public string path;
    [JsonProperty(PropertyName = "newfilespaceid", Required = Required.Always)]
    public string newfilespaceid;
    [JsonProperty(PropertyName = "newPath", Required = Required.Always)]
    public string newPath;
    [JsonProperty(PropertyName = "replace", Required = Required.Default)]
    public bool replace;
    [JsonProperty(PropertyName = "merge", Required = Required.Default)]
    public bool merge;
  }
}
