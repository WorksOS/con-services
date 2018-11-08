using System;
using Newtonsoft.Json;

namespace TagFileHarvester.Models
{
  public class DirResult : ApiResult
  {
    [JsonProperty(PropertyName = "createTime", Required = Required.Default)]
    public DateTime createTime;

    [JsonProperty(PropertyName = "entries", Required = Required.Default)]
    public DirResult[] entries;

    [JsonProperty(PropertyName = "entryName", Required = Required.Default)]
    public string entryName;

    [JsonProperty(PropertyName = "isFolder", Required = Required.Default)]
    public bool isFolder;

    [JsonProperty(PropertyName = "leaf", Required = Required.Default)]
    public bool leaf;

    [JsonProperty(PropertyName = "modifyTime", Required = Required.Default)]
    public DateTime modifyTime;
  }
}