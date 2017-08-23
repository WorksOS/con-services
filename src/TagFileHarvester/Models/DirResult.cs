using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace VSS.Productivity3D.TagFileHarvester.Models
{
  public class DirResult : ApiResult
  {
    [JsonProperty(PropertyName = "createTime", Required = Required.Default)]
    public DateTime createTime;
    [JsonProperty(PropertyName = "entryName", Required = Required.Default)]
    public string entryName;
    [JsonProperty(PropertyName = "entries", Required = Required.Default)]
    public DirResult[] entries;
    [JsonProperty(PropertyName = "isFolder", Required = Required.Default)]
    public bool isFolder;
    [JsonProperty(PropertyName = "leaf", Required = Required.Default)]
    public bool leaf;
    [JsonProperty(PropertyName = "modifyTime", Required = Required.Default)]
    public DateTime modifyTime;
  }
}
