using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace VSS.Productivity3D.TagFileHarvester.Models
{
  public class LastDirChangeParams
  {
    [JsonProperty(PropertyName = "filespaceid", Required = Required.Always)]
    public string filespaceid;
    [JsonProperty(PropertyName = "path", Required = Required.Always)]
    public string path;
    [JsonProperty(PropertyName = "recursive", Required = Required.Always)]
    public bool recursive;

  }
}
