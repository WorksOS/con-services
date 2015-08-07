using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TagFileHarvester.Models
{
  public class DirParams
  {
    [JsonProperty(PropertyName = "filespaceid", Required = Required.Always)]
    public string filespaceid;
    [JsonProperty(PropertyName = "path", Required = Required.Always)]
    public string path;
    [JsonProperty(PropertyName = "recursive", Required = Required.Default)]
    public bool recursive;
    [JsonProperty(PropertyName = "filterfolders", Required = Required.Default)]
    public bool filterfolders;
    [JsonProperty(PropertyName = "filemasklist", Required = Required.Default)]
    public string filemasklist;
  }
}
